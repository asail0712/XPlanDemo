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
using UnityEngine.UI;

namespace XPlan.Weaver.Runtime
{
    public static class VmInputTfBindingRuntime
    {
        /// <summary>
        /// 1. 傳入 ViewBase 的衍生類別實例即可（型別用 object 即可）
        /// </summary>
        public static void Bind(object viewInstance, MethodInfo[] methods)
        {
            if (viewInstance == null)
                return;

            var viewType = viewInstance.GetType();

            // 2. 掃描 View 裡的 InputField 欄位，記錄名稱 → InputField 實例
            var tfMap = FindInputTfOnView(viewType, viewInstance);
            if (tfMap.Count == 0)
                return;

            foreach (var method in methods)
            {
                // 沒有 attribute 就跳過
                if (!method.IsDefined(typeof(InputTfBindingAttribute), inherit: true))
                    continue;

                // 必須：無回傳 (void)
                if (method.ReturnType != typeof(void))
                    continue;

                // 參數要有一個 並且為string型別
                var parameters = method.GetParameters();
                if (parameters.Length != 1 || parameters[0].ParameterType != typeof(string))
                    continue;

                // 名稱必須符合 On[ToggleName]Trigger
                var tfNameCore = ExtractTfNameFromMethod(method.Name);
                if (tfNameCore == null)
                    continue;

                // 4. 根據 tfNameCore 去對應 View 上的 tf 欄位名稱
                if (!TryFindTfByName(tfMap, tfNameCore, out var tf))
                    continue;

                // 實際綁定 onValueChanged
                // 注意：這裡用閉包包住 method & viewModel
                var targetMethod = method;

                tf.onValueChanged.AddListener((s) =>
                {
                    try
                    {
                        // ★ 點擊時才找一次 _viewModel，避免提前存取
                        var targetVm = GetViewModelInstance(viewType, viewInstance);

                        targetMethod.Invoke(targetVm, new object[] { s });
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError(
                            $"[VmInputTfBindingRuntime] 執行 {targetMethod.DeclaringType.Name}.{targetMethod.Name} 時發生例外：{ex}");
                    }
                });
            }
        }

        public static void Unbind(object viewInstance)
        {
            if (viewInstance == null)
                return;

            var viewType    = viewInstance.GetType();
            var tfMap       = FindInputTfOnView(viewType, viewInstance);
            if (tfMap.Count == 0)
                return;

            // 移除全部 Listener（Unity 最標準 safest 的作法）
            foreach (var kv in tfMap)
            {
                var tf = kv.Value;
                if (tf != null)
                {
                    tf.onValueChanged.RemoveAllListeners();
                }
            }
        }

        /// <summary>
        /// 往上爬型別階層，找所有 InputField 欄位
        /// </summary>
        private static Dictionary<string, InputField> FindInputTfOnView(Type viewType, object viewInstance)
        {
            var dict    = new Dictionary<string, InputField>();
            var cur     = viewType;

            while (cur != null && cur != typeof(object))
            {
                var fields = cur.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var f in fields)
                {
                    if (typeof(InputField).IsAssignableFrom(f.FieldType))
                    {
                        var tf             = f.GetValue(viewInstance) as InputField;
                        var normalizeName   = NormalizeName(f.Name);

                        if (tf != null && !dict.ContainsKey(normalizeName))
                        {
                            dict.Add(normalizeName, tf);
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
        /// 解析方法名稱是否符合 On[TfName]Modify，
        /// 有的話回傳 [TfName]（中間那段），否則回傳 null。
        /// </summary>
        private static string ExtractTfNameFromMethod(string methodName)
        {
            const string prefix = "On";
            const string suffix = "Modify";

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

        private static bool TryFindTfByName(
            Dictionary<string, InputField> map,
            string tfNameCore,
            out InputField tf)
        {
            // 精準大小寫一致：DemoChange
            if (map.TryGetValue(tfNameCore, out tf))
                return true;
            
            var camel = char.ToLowerInvariant(tfNameCore[0]) +
                        tfNameCore.Substring(1);

            if (map.TryGetValue(camel, out tf))
                return true;

            // 都沒有命中就失敗
            tf = null;
            return false;
        }

        /// <summary>
        /// 從 View 的繼承鏈中找到 ViewBase&lt;TViewModel&gt;，並取出 TViewModel 型別。
        /// </summary>
        private static Type GetViewModelTypeFromView(Type viewType)
        {
            var cur = viewType;
            while (cur != null && cur != typeof(object))
            {
                if (cur.IsGenericType)
                {
                    var def = cur.GetGenericTypeDefinition();

                    // 用 FullName 避免組件引用問題
                    if (def.FullName == "XPlan.UI.ViewBase`1")
                    {
                        var args = cur.GetGenericArguments();
                        if (args != null && args.Length == 1)
                            return args[0];
                    }
                }

                cur = cur.BaseType;
            }

            return null;
        }

        private static readonly string[] Prefixes =
        {
            "_",
            "m_"
        };

        private static readonly string[] Suffixes =
        {
            "Txt",
            "txt",
            "Text",
            "text"
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
