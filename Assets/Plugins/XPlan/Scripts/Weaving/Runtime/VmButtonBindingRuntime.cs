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
    public sealed class ButtonBindingHandle
    {
        // 用 button instance id 當 key，確保同顆按鈕不會重複疊 listener
        internal readonly Dictionary<int, (Button button, UnityAction action)> map = new();
        public int Count => map.Count;
        public void Clear() => map.Clear();
    }

    public static class VmButtonBindingRuntime
    {
        /// <summary>
        /// 1. 傳入 ViewBase 的衍生類別實例即可（型別用 object 即可）
        /// </summary>
        public static ButtonBindingHandle Bind(object viewInstance, MethodInfo[] methods)
        {
            var handle = new ButtonBindingHandle();

            if (viewInstance == null || methods == null || methods.Length == 0)
                return handle;

            var viewType    = viewInstance.GetType();

            // 2. 掃描 View 裡的 Button 欄位，記錄名稱 → Button 實例
            var buttonMap   = FindButtonsOnView(viewType, viewInstance);
            if (buttonMap.Count == 0)
                return handle;

            foreach (var method in methods)
            {
                // 沒有 attribute 就跳過
                if (!method.IsDefined(typeof(ButtonBindingAttribute), inherit: true))
                    continue;

                // 必須：無參數、無回傳 (void)
                if (method.ReturnType != typeof(void))
                    continue;

                if (method.GetParameters().Length != 0)
                    continue;

                // 名稱必須符合 On[ButtonName]Click
                var buttonNameCore = ExtractButtonNameFromMethod(method.Name);
                if (buttonNameCore == null)
                    continue;

                // 4. 根據 buttonNameCore 去對應 View 上的 Button 欄位名稱
                if (!TryFindButtonByName(buttonMap, buttonNameCore, out var button))
                    continue;

                // 實際綁定 onClick
                // 注意：這裡用閉包包住 method & viewModel
                var targetMethod = method;

                // 防同一次 Bind 對同顆 button 疊加（如果 methods 裡不小心重複）
                int btnId = button.GetInstanceID();
                if (handle.map.TryGetValue(btnId, out var old))
                {
                    // 先移掉舊的再換新的（避免同顆 button 疊兩個我們自己的 listener）
                    if (old.button != null && old.action != null)
                        old.button.onClick.RemoveListener(old.action);

                    handle.map.Remove(btnId);
                }

                UnityAction action = () =>
                {
                    try
                    {
                        var targetVm = GetViewModelInstance(viewType, viewInstance);
                        if (targetVm == null)
                            return;

                        targetMethod.Invoke(targetVm, null);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(
                            $"[VmButtonBindingRuntime] 執行 {targetMethod.DeclaringType?.Name}.{targetMethod.Name} 時發生例外：{ex}");
                    }
                };

                // 記錄並加上 listener
                handle.map[btnId] = (button, action);
                button.onClick.AddListener(action);
            }

            return handle;
        }

        public static void Unbind(ButtonBindingHandle handle)
        {
            if (handle == null || handle.map.Count == 0)
                return;

            foreach (var kv in handle.map)
            {
                var (button, action) = kv.Value;
                if (button == null || action == null)
                    continue;

                button.onClick.RemoveListener(action);
            }

            handle.Clear();
        }

        /// <summary>
        /// 往上爬型別階層，找所有 Button 欄位
        /// </summary>
        private static Dictionary<string, Button> FindButtonsOnView(Type viewType, object viewInstance)
        {
            var dict    = new Dictionary<string, Button>();
            var cur     = viewType;

            while (cur != null && cur != typeof(object))
            {
                var fields = cur.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var f in fields)
                {
                    if (typeof(Button).IsAssignableFrom(f.FieldType))
                    {
                        var btn             = f.GetValue(viewInstance) as Button;
                        var normalizeName   = NormalizeName(f.Name);

                        if (btn != null && !dict.ContainsKey(normalizeName))
                        {
                            dict.Add(normalizeName, btn);
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
        /// 解析方法名稱是否符合 On[ButtonName]Click，
        /// 有的話回傳 [ButtonName]（中間那段），否則回傳 null。
        /// </summary>
        private static string ExtractButtonNameFromMethod(string methodName)
        {
            const string prefix = "On";
            const string suffix = "Click";

            if (!methodName.StartsWith(prefix, StringComparison.Ordinal))
                return null;

            if (!methodName.EndsWith(suffix, StringComparison.Ordinal))
                return null;

            var core = methodName.Substring(
                prefix.Length,
                methodName.Length - prefix.Length - suffix.Length);

            if (string.IsNullOrEmpty(core))
                return null;

            return core;
        }

        /// <summary>
        /// 根據「中間那段 ButtonNameCore」去找 View 上的 Button 欄位。
        /// 支援幾種常見命名：
        ///  - demoTriggerBtn    <-> OnDemoTriggerClick
        ///  - DemoTrigger       <-> OnDemoTriggerClick
        ///  - demoTrigger       <-> OnDemoTriggerClick
        /// </summary>
        private static bool TryFindButtonByName(
            Dictionary<string, Button> map,
            string buttonNameCore,
            out Button button)
        {
            // 精準大小寫一致：DemoTrigger
            if (map.TryGetValue(buttonNameCore, out button))
                return true;

            // 首字小寫：demoTrigger / demoTriggerBtn
            var camel = char.ToLowerInvariant(buttonNameCore[0]) +
                        buttonNameCore.Substring(1);

            if (map.TryGetValue(camel, out button))
                return true;

            // 都沒有命中就失敗
            button = null;
            return false;
        }

        private static readonly string[] Prefixes =
        {
            "_",
            "m_"
        };

        private static readonly string[] Suffixes =
        {
            "Btn",
            "btn",
            "Button",
            "button"
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
