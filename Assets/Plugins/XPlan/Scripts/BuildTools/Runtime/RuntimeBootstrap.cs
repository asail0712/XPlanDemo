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
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using XPlan.Utility;

namespace XPlan.BuildTools.Runtime
{
    public interface IRuntimeConfigApplier
    {
        void Apply(string configText);
        int Order { get; }
    }

    public static class RuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnRuntimeStart()
        {
            Debug.Log("[RuntimeBootstrap] Applying Runtime Build Config...");

#if UNITY_ANDROID && !UNITY_EDITOR
            // Android StreamingAssets 需要用 UnityWebRequest，所以改走 coroutine
            RunAndroidLoadAndApply();
#else
            ApplyCurrentBuildConfig(LoadText());
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void RunAndroidLoadAndApply()
        {
            var go = new GameObject("__RuntimeBootstrapRunner__");
            GameObject.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            MonoBehaviourHelper.StartCoroutine(LoadTextAndroid(text =>
            {
                ApplyCurrentBuildConfig(text);
                GameObject.Destroy(go);
            }));
        }

        private static IEnumerator LoadTextAndroid(Action<string> onDone)
        {
            // 1) StreamingAssets（Android：jar 路徑，必須 UnityWebRequest）
            var path = Path.Combine(Application.streamingAssetsPath, "xplan_runtime_config.json");

            using (var req = UnityWebRequest.Get(path))
            {
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    onDone(req.downloadHandler.text);
                    yield break;
                }

                Debug.LogWarning($"[RuntimeBootstrap] StreamingAssets read failed: {req.error} path={path}");
            }

            // 2) Resources fallback（可選）
            var ta = Resources.Load<TextAsset>("XPlan/BuildTools/xplan_runtime_config");
            onDone(ta ? ta.text : null);
        }
#endif

        private static void ApplyCurrentBuildConfig(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            // 找出所有實作
            var appliers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Type.EmptyTypes; }
                })
                .Where(t => !t.IsAbstract && typeof(IRuntimeConfigApplier).IsAssignableFrom(t))
                .Select(t => Activator.CreateInstance(t) as IRuntimeConfigApplier)
                .Where(x => x != null)
                .OrderBy(x => x.Order);

            foreach (var applier in appliers)
                applier.Apply(text);
        }

#if !UNITY_ANDROID || UNITY_EDITOR
        private static string LoadText()
        {
            // 1) StreamingAssets（最常用）
            var path = Path.Combine(Application.streamingAssetsPath, "xplan_runtime_config.json");
            if (File.Exists(path))
                return File.ReadAllText(path);

            // 2) Resources fallback（可選）
            var ta = Resources.Load<TextAsset>("XPlan/BuildTools/xplan_runtime_config");
            return ta ? ta.text : null;
        }
#endif
    }
}