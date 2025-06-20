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

        static DefineSymbolChecker()
        {
            IEnumerable<Type> typeList          = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());

            bool bArFoundationInstalled         = typeList.Any(type => type.Namespace == "UnityEngine.XR.ARFoundation");
            bool bAddressableAssetsInstalled    = typeList.Any(type => type.Namespace == "UnityEngine.AddressableAssets");

            if (bArFoundationInstalled)
            {
                AddDefineSymbols(arFoundationSymbol);
            }

            if (bAddressableAssetsInstalled)
            {
                AddDefineSymbols(addressableSymbol);
            }
        }

        static private void AddDefineSymbols(string symbol)
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbols(GetBuildTarget());

            if(!symbols.Contains(symbol))
            {
                symbols += ";" + symbol;
                PlayerSettings.SetScriptingDefineSymbols(GetBuildTarget(), symbols);
                Debug.Log($"✅ 已自動加入 symbol: {symbol}");
            }
        }

        static private NamedBuildTarget GetBuildTarget()
        {
#if UNITY_ANDROID
            return NamedBuildTarget.Android;
#elif UNITY_IOS
            return NamedBuildTarget.iOS;
#else
            return NamedBuildTarget.Standalone;
#endif
        }
    }
}
#endif
