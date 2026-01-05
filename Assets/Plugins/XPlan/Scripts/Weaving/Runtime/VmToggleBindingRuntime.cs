// ==============================================================================
// XPlan Framework
//
// Copyright (c) 2026 Asail
// All rights reserved.
//
// Author  : Asail0712
// Project : XPlan
// Description:
//     A modular framework for Unity projects, focusing on MVVM architecture,
//     runtime tooling, event-driven design, and extensibility.
//
// Contact : asail0712@gmail.com
// GitHub  : https://github.com/asail0712/XPlanDemo
//
// Unauthorized copying, modification, or distribution of this file,
// via any medium, is strictly prohibited without prior permission.
// ==============================================================================
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace XPlan.Weaver.Runtime
{
    /// <summary>
    /// 儲存這次 Bind 加上的所有 Listener，讓 View 自己掌控生命週期。
    /// </summary>
    [Serializable]
    public sealed class ToggleBindingHandle
    {
        // 用 button instance id 當 key，確保同顆按鈕不會重複疊 listener
        internal readonly Dictionary<int, (Toggle tgl, UnityAction<bool> action)> map = new();
        public int Count => map.Count;
        public void Clear() => map.Clear();
    }

    public static class VmToggleBindingRuntime
    {
        /// <summary>
        /// 1. 傳入 ViewBase 的衍生類別實例即可（型別用 object 即可）
        /// </summary>
        public static ToggleBindingHandle Bind(object viewInstance, MethodInfo[] methods)
        {
            var handle = new ToggleBindingHandle();

            if (viewInstance == null || methods == null || methods.Length == 0)
                return handle;

            var viewType = viewInstance.GetType();

            // 2. 掃描 View 裡的 Toggle 欄位，記錄名稱 → Toggle 實例
            var toggleMap = FindTogglesOnView(viewType, viewInstance);
            if (toggleMap.Count == 0)
                return handle;

            foreach (var method in methods)
            {
                // 沒有 attribute 就跳過
                if (!method.IsDefined(typeof(ToggleBindingAttribute), inherit: true))
                    continue;

                // 必須：無回傳 (void)
                if (method.ReturnType != typeof(void))
                    continue;

                // 參數要有一個 並且為bool型別
                var parameters = method.GetParameters();
                if (parameters.Length != 1 || parameters[0].ParameterType != typeof(bool))
                    continue;

                // 名稱必須符合 On[ToggleName]Trigger
                var toggleNameCore = ExtractToggleNameFromMethod(method.Name);
                if (toggleNameCore == null)
                    continue;

                // 4. 根據 toggleNameCore 去對應 View 上的 Toggle 欄位名稱
                if (!TryFindToggleByName(toggleMap, toggleNameCore, out var tgl))
                    continue;

                // 實際綁定 onValueChanged
                // 注意：這裡用閉包包住 method & viewModel
                var targetMethod = method;
                
                // 防同一次 Bind 對同顆 button 疊加（如果 methods 裡不小心重複）
                int tglId = tgl.GetInstanceID();
                if (handle.map.TryGetValue(tglId, out var old))
                {
                    // 先移掉舊的再換新的（避免同顆 button 疊兩個我們自己的 listener）
                    if (old.tgl != null && old.action != null)
                        old.tgl.onValueChanged.RemoveListener(old.action);

                    handle.map.Remove(tglId);
                }

                UnityAction<bool> action = (b) =>
                {
                    try
                    {
                        var targetVm = GetViewModelInstance(viewType, viewInstance);
                        if (targetVm == null)
                            return;

                        targetMethod.Invoke(targetVm, new object[] { b });
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(
                            $"[VmToggleBindingRuntime] 執行 {targetMethod.DeclaringType?.Name}.{targetMethod.Name} 時發生例外：{ex}");
                    }
                };

                // 記錄並加上 listener
                handle.map[tglId] = (tgl, action);
                tgl.onValueChanged.AddListener(action);
            }

            return handle;
        }

        public static void Unbind(ToggleBindingHandle handle)
        {
            if (handle == null || handle.map.Count == 0)
                return;

            foreach (var kv in handle.map)
            {
                var (tgl, action) = kv.Value;
                if (tgl == null || action == null)
                    continue;

                tgl.onValueChanged.RemoveListener(action);
            }

            handle.Clear();
        }

        /// <summary>
        /// 往上爬型別階層，找所有 Button 欄位
        /// </summary>
        private static Dictionary<string, Toggle> FindTogglesOnView(Type viewType, object viewInstance)
        {
            var dict    = new Dictionary<string, Toggle>();
            var cur     = viewType;

            while (cur != null && cur != typeof(object))
            {
                var fields = cur.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var f in fields)
                {
                    if (typeof(Toggle).IsAssignableFrom(f.FieldType))
                    {
                        var tgl             = f.GetValue(viewInstance) as Toggle;
                        var normalizeName   = NormalizeName(f.Name);

                        if (tgl != null && !dict.ContainsKey(normalizeName))
                        {
                            dict.Add(normalizeName, tgl);
                        }
                    }
                }

                cur = cur.BaseType;
            }

            return dict;
        }

        /// <summary>
        /// 在 ViewBase&lt;TViewModel&gt; 上找到 _viewModel 欄位並取值
        /// </summary>
        private static object GetViewModelInstance(Type viewType, object viewInstance)
        {
            // 可能 ViewBase 在幾層上面，所以一樣往上爬
            var cur = viewType;
            while (cur != null && cur != typeof(object))
            {
                // 直接用欄位名稱找 _viewModel
                var field = cur.GetField("_viewModel",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (field != null)
                {
                    return field.GetValue(viewInstance);
                }

                cur = cur.BaseType;
            }

            return null;
        }

        /// <summary>
        /// 解析方法名稱是否符合 On[ToggleName]Trigger，
        /// 有的話回傳 [ToggleName]（中間那段），否則回傳 null。
        /// </summary>
        private static string ExtractToggleNameFromMethod(string methodName)
        {
            const string prefix = "On";
            const string suffix = "Trigger";

            if (!methodName.StartsWith(prefix, StringComparison.Ordinal))
                return null;

            if (!methodName.EndsWith(suffix, StringComparison.Ordinal))
                return null;

            int coreLength = methodName.Length - prefix.Length - suffix.Length;
            if (coreLength <= 0)
                return null;

            var core = methodName.Substring(
                prefix.Length,
                methodName.Length - prefix.Length - suffix.Length);

            if (string.IsNullOrEmpty(core))
                return null;

            return core;
        }

        private static bool TryFindToggleByName(
            Dictionary<string, Toggle> map,
            string tglNameCore,
            out Toggle tgl)
        {
            // 精準大小寫一致：DemoTrigger
            if (map.TryGetValue(tglNameCore, out tgl))
                return true;

            // 首字小寫：demoTrigger / demoTriggerBtn
            var camel = char.ToLowerInvariant(tglNameCore[0]) +
                        tglNameCore.Substring(1);

            if (map.TryGetValue(camel, out tgl))
                return true;

            // 都沒有命中就失敗
            tgl = null;
            return false;
        }

        private static readonly string[] Prefixes =
        {
            "_",
            "m_"
        };

        private static readonly string[] Suffixes =
        {
            "Tgl",
            "tgl",
            "Toggle",
            "toggle"
        };

        private static string NormalizeName(string raw)
        {
            string name = raw;

            // 移除前綴（Prefix）
            foreach (var p in Prefixes)
            {
                if (name.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(p.Length);
                    break;
                }
            }

            // 移除後綴（Suffix）
            foreach (var s in Suffixes)
            {
                if (name.EndsWith(s, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(0, name.Length - s.Length);
                    break;
                }
            }

            return name;
        }
    }
}
