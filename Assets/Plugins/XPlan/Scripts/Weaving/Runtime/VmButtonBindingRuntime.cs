using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;

namespace XPlan.Weaver.Runtime
{
    public static class VmButtonBindingRuntime
    {
        /// <summary>
        /// 1. 傳入 ViewBase 的衍生類別實例即可（型別用 object 即可）
        /// </summary>
        public static void BindButtons(object viewInstance)
        {
            if (viewInstance == null)
                return;

            var viewType = viewInstance.GetType();

            // 2. 掃描 View 裡的 Button 欄位，記錄名稱 → Button 實例
            var buttonMap = FindButtonsOnView(viewType, viewInstance);
            if (buttonMap.Count == 0)
                return;

            // ★ 這裡改成「透過 ViewBase<TViewModel> 的泛型參數取 vmType」
            var vmType = GetViewModelTypeFromView(viewType);
            if (vmType == null)
                return;

            // 3. 檢查 VM 裡所有有 [ButtonBinding] 的方法
            var methods = vmType.GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

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

                button.onClick.AddListener(() =>
                {
                    try
                    {
                        // ★ 點擊時才找一次 _viewModel，避免提前存取
                        var targetVm = GetViewModelInstance(viewType, viewInstance);

                        targetMethod.Invoke(targetVm, null);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError(
                            $"[VmButtonBindingRuntime] 執行 {targetMethod.DeclaringType.Name}.{targetMethod.Name} 時發生例外：{ex}");
                    }
                });
            }
        }

        public static void UnbindButtons(object viewInstance)
        {
            if (viewInstance == null)
                return;

            var viewType    = viewInstance.GetType();
            var buttonMap   = FindButtonsOnView(viewType, viewInstance);
            if (buttonMap.Count == 0)
                return;

            // 移除全部 Listener（Unity 最標準 safest 的作法）
            foreach (var kv in buttonMap)
            {
                var btn = kv.Value;
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                }
            }
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
