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
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace XPlan.Editors.Initial
{
    [InitializeOnLoad]
    public static class DefineSymbolChecker
    {
        const string addressableSymbol  = "ADDRESSABLES_EXISTS";
        const string arFoundationSymbol = "AR_FOUNDATION";
        const string editorPrefKey      = "XPlan.DefineSymbolChecker.Enabled";

        static bool Enabled
        {
            get => EditorPrefs.GetBool(editorPrefKey, false);
            set => EditorPrefs.SetBool(editorPrefKey, value);
        }

        static DefineSymbolChecker()
        {
            if (!Enabled)
                return;

            RunCheck();
        }

        static void RunCheck()
        {
            IEnumerable<Type> typeList = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Type.EmptyTypes; }
                });

            bool bArFoundationInstalled =
                typeList.Any(t => t.Namespace == "UnityEngine.XR.ARFoundation");

            bool bAddressableAssetsInstalled =
                typeList.Any(t => t.Namespace == "UnityEngine.AddressableAssets");

            if (bArFoundationInstalled)
                AddDefineSymbols(arFoundationSymbol);

            if (bAddressableAssetsInstalled)
                AddDefineSymbols(addressableSymbol);
        }

        static void AddDefineSymbols(string symbol)
        {
            var target = GetBuildTarget();
            string defs = PlayerSettings.GetScriptingDefineSymbols(target);

            if (!defs.Split(';').Contains(symbol))
            {
                defs = string.IsNullOrEmpty(defs) ? symbol : $"{defs};{symbol}";
                PlayerSettings.SetScriptingDefineSymbols(target, defs);
                Debug.Log($"✅ 已自動加入 symbol: {symbol}");
            }
        }

        static NamedBuildTarget GetBuildTarget()
        {
#if UNITY_ANDROID
            return NamedBuildTarget.Android;
#elif UNITY_IOS
            return NamedBuildTarget.iOS;
#else
            return NamedBuildTarget.Standalone;
#endif
        }

        // =========================
        // Menu
        // =========================

        [MenuItem("XPlanTools/Symbol Checker Enabled", false, 9)]
        private static void Toggle()
        {
            Enabled = !Enabled;

            if (Enabled)
            {
                Debug.Log("🟢 DefineSymbolChecker 已啟用");
                RunCheck();
            }
            else
            {
                Debug.Log("🔴 DefineSymbolChecker 已停用");
            }
        }

        [MenuItem("XPlanTools/Symbol Checker Enabled", true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked("XPlanTools/Symbol Checker Enabled", Enabled);
            return true;
        }
    }
}
#endif
