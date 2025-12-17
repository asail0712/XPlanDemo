#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XPlan.BuildTools.Runtime;
using XPlan.Utility;

namespace XPlan.BuildTools.Editors
{
    public class BuildPlayWindow : EditorWindow
    {
        private enum Tab { Build, Play }

        [SerializeField] private Tab tab;

        private Vector2 buildScroll;
        private Vector2 playScroll;

        // Work copy（暫存副本，編輯不會影響資產）
        private BuildConfigSO buildWork;
        private PlayConfigSO playWork;

        private SerializedObject buildSO;
        private SerializedObject playSO;

        private bool buildDirty;
        private bool playDirty;

        // -------------------------
        // Dropdown cache (Build/Play)
        // -------------------------
        private sealed class DropdownCache<T> where T : ScriptableObject
        {
            public T[] Assets = Array.Empty<T>();
            public string[] Labels = Array.Empty<string>();
            public int Index = 0; // 0 = None, 1..N = assets
            public double LastRefreshTime = -9999;
        }

        private readonly DropdownCache<BuildConfigSO> buildCache = new();
        private readonly DropdownCache<PlayConfigSO> playCache = new();

        private const double RefreshIntervalSeconds = 1.0; // 避免 OnGUI 每幀掃資產卡頓

        // -------------------------
        // Current*Config asset paths (auto create / overwrite)
        // -------------------------
        private const string CurrentBuildConfigAssetPath = "Assets/Resources/XPlan/BuildTools/CurrentBuildConfig.asset";
        private const string CurrentPlayConfigAssetPath = "Assets/Resources/XPlan/BuildTools/CurrentPlayConfig.asset";

        [MenuItem("XPlanTools/BuildTools/Build or Play", false, 9)]
        public static void Open()
        {
            var w = GetWindow<BuildPlayWindow>();
            w.titleContent = new GUIContent("XPlan Build & Play");
            w.Show();
        }

        private void OnDisable()
        {
            DestroyWorkCopy(buildWork);
            DestroyWorkCopy(playWork);
            buildWork = null;
            playWork = null;
            buildSO = null;
            playSO = null;
        }

        private void OnGUI()
        {
            var settings = ToolSettings.instance;

            tab = (Tab)GUILayout.Toolbar((int)tab, new[] { "Build", "Play" });
            EditorGUILayout.Space(8);

            switch (tab)
            {
                case Tab.Build:
                    DrawBuildTab(settings);
                    break;
                case Tab.Play:
                    DrawPlayTab(settings);
                    break;
            }

            // 保險：Inspector 外部改動也刷新
            if (Event.current.type == EventType.Layout) Repaint();
        }

        // -------------------------
        // Build Tab
        // -------------------------
        private void DrawBuildTab(ToolSettings settings)
        {
            // 1) 下拉式選單選擇 BuildConfig
            RefreshDropdown(buildCache, settings.buildConfig, "t:BuildConfigSO");

            EditorGUI.BeginChangeCheck();
            buildCache.Index = EditorGUILayout.Popup("Build Config", buildCache.Index, buildCache.Labels);
            if (EditorGUI.EndChangeCheck())
            {
                var newCfg = GetSelectedFromDropdown(buildCache);
                settings.buildConfig = newCfg;
                settings.Save();

                ResetBuildWorkCopy(settings.buildConfig);
                GUI.FocusControl(null);
            }

            DrawRightSideMiniButtons(
                settings.buildConfig,
                onPing: () => EditorGUIUtility.PingObject(settings.buildConfig),
                onSelect: () => Selection.activeObject = settings.buildConfig
            );

            if (!settings.buildConfig)
            {
                EditorGUILayout.HelpBox("請選擇 BuildConfigSO（或其子類）。", MessageType.Info);
                return;
            }

            // 2) 確保 WorkCopy 存在（例如剛開窗）
            EnsureBuildWorkCopy(settings.buildConfig);

            // 3) 顯示：標題用資產名，但欄位編輯是 WorkCopy
            EditorGUILayout.LabelField(settings.buildConfig.DisplayName, EditorStyles.boldLabel);

            buildSO.UpdateIfRequiredOrScript();

            EditorGUI.BeginChangeCheck();
            buildScroll = EditorGUILayout.BeginScrollView(buildScroll);
            DrawAllProperties(buildSO);
            EditorGUILayout.EndScrollView();
            if (EditorGUI.EndChangeCheck()) buildDirty = true;

            // 只套用到 WorkCopy（不會動到資產）
            buildSO.ApplyModifiedProperties();

            EditorGUILayout.Space(8);

            // 4) 套用 / 還原
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = buildDirty;

                if (GUILayout.Button("套用（寫回資產）", GUILayout.Height(28)))
                {
                    ApplyWorkToAsset(buildWork, settings.buildConfig);
                    buildDirty = false;
                    Debug.Log($"[XPlan] Applied BuildConfig changes: {settings.buildConfig.name}");
                }

                if (GUILayout.Button("還原（放棄修改）", GUILayout.Height(28)))
                {
                    ResetBuildWorkCopy(settings.buildConfig);
                    GUI.FocusControl(null);
                }

                GUI.enabled = true;
            }

            EditorGUILayout.Space(6);

            // 5) 真正 Build / Build&Run
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Build", GUILayout.Height(28)))
                {
                    // TODO：真正 Build 流程接這裡（用 settings.buildConfig 這顆資產）
                    Debug.Log($"[XPlan] Build with config asset: {settings.buildConfig.name}");
                }

                if (GUILayout.Button("Build & Run", GUILayout.Height(28)))
                {
                    // TODO：Build + Run
                    Debug.Log($"[XPlan] Build&Run with config asset: {settings.buildConfig.name}");
                }
            }
        }

        // -------------------------
        // Play Tab
        // -------------------------
        private void DrawPlayTab(ToolSettings settings)
        {
            // 1) 下拉式選單選擇 PlayConfig
            RefreshDropdown(playCache, settings.playConfig, "t:PlayConfigSO");

            EditorGUI.BeginChangeCheck();
            playCache.Index = EditorGUILayout.Popup("Play Config", playCache.Index, playCache.Labels);
            if (EditorGUI.EndChangeCheck())
            {
                var newCfg = GetSelectedFromDropdown(playCache);
                settings.playConfig = newCfg;
                settings.Save();

                ResetPlayWorkCopy(settings.playConfig);
                GUI.FocusControl(null);
            }

            DrawRightSideMiniButtons(
                settings.playConfig,
                onPing: () => EditorGUIUtility.PingObject(settings.playConfig),
                onSelect: () => Selection.activeObject = settings.playConfig
            );

            if (!settings.playConfig)
            {
                EditorGUILayout.HelpBox("請選擇 PlayConfigSO（或其子類）。", MessageType.Info);
                return;
            }

            // 2) 確保 WorkCopy 存在（例如剛開窗）
            EnsurePlayWorkCopy(settings.playConfig);

            // 3) 顯示：標題用資產名，但欄位編輯是 WorkCopy
            EditorGUILayout.LabelField(settings.playConfig.DisplayName, EditorStyles.boldLabel);

            playSO.UpdateIfRequiredOrScript();

            EditorGUILayout.Space(8);

            playDirty = true;

            // 4) 套用 / 還原
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = playDirty;

                if (GUILayout.Button("套用（寫回資產）", GUILayout.Height(28)))
                {
                    ApplyWorkToAsset(playWork, settings.playConfig);
                    playDirty = false;
                    Debug.Log($"[XPlan] Applied PlayConfig changes: {settings.playConfig.name}");
                }

                if (GUILayout.Button("還原（放棄修改）", GUILayout.Height(28)))
                {
                    ResetPlayWorkCopy(settings.playConfig);
                    GUI.FocusControl(null);
                }

                GUI.enabled = true;
            }

            EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Apply Config（執行套用邏輯）", GUILayout.Height(28)))
                {
                    if (playDirty)
                    {
                        ApplyWorkToAsset(playWork, settings.playConfig);
                        playDirty = false;
                    }

                    Debug.Log($"[XPlan] Apply play config logic with asset: {settings.playConfig.name}");
                }

                if (GUILayout.Button(EditorApplication.isPlaying ? "Stop" : "Apply And Play", GUILayout.Height(28)))
                {
                    ApplyWorkToAsset(playWork, settings.playConfig);
                    EditorApplication.isPlaying = !EditorApplication.isPlaying;
                }
            }

            EditorGUILayout.Space(6);

            EditorGUI.BeginChangeCheck();
            playScroll = EditorGUILayout.BeginScrollView(playScroll);
            DrawAllProperties(playSO);
            EditorGUILayout.EndScrollView();
            EditorGUI.EndChangeCheck();

            // 只套用到 WorkCopy（不會動到資產）
            playSO.ApplyModifiedProperties();
        }

        // -------------------------
        // Dropdown helpers (generic)
        // -------------------------
        private void RefreshDropdown<T>(DropdownCache<T> cache, T current, string findAssetsFilter)
            where T : ScriptableObject
        {
            var now = EditorApplication.timeSinceStartup;
            if (cache.Assets.Length > 0 && (now - cache.LastRefreshTime) < RefreshIntervalSeconds)
            {
                // 仍需要同步 Index（避免外部改動 settings）
                cache.Index = FindIndex(cache.Assets, current);
                if (cache.Labels == null || cache.Labels.Length == 0)
                    cache.Labels = BuildLabels(cache.Assets);
                return;
            }

            cache.LastRefreshTime = now;

            var guids = AssetDatabase.FindAssets(findAssetsFilter);
            var list = new List<T>(guids.Length);

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset) list.Add(asset);
            }

            // 排序：有 DisplayName 就用 DisplayName，沒有就用 name
            list.Sort((a, b) =>
            {
                string an = GetDisplayName(a);
                string bn = GetDisplayName(b);
                return string.Compare(an, bn, StringComparison.OrdinalIgnoreCase);
            });

            cache.Assets = list.ToArray();
            cache.Labels = BuildLabels(cache.Assets);
            cache.Index = FindIndex(cache.Assets, current);
        }

        private static string[] BuildLabels<T>(T[] assets) where T : ScriptableObject
        {
            var labels = new string[(assets?.Length ?? 0) + 1];
            labels[0] = "<None>";
            if (assets != null)
            {
                for (int i = 0; i < assets.Length; i++)
                    labels[i + 1] = GetDisplayName(assets[i]);
            }
            return labels;
        }

        private static int FindIndex<T>(T[] assets, T current) where T : ScriptableObject
        {
            if (!current || assets == null) return 0;
            for (int i = 0; i < assets.Length; i++)
                if (assets[i] == current) return i + 1;
            return 0;
        }

        private static T GetSelectedFromDropdown<T>(DropdownCache<T> cache) where T : ScriptableObject
        {
            if (cache.Index <= 0) return null;
            var i = cache.Index - 1;
            if (cache.Assets == null || i < 0 || i >= cache.Assets.Length) return null;
            return cache.Assets[i];
        }

        private static string GetDisplayName(UnityEngine.Object obj)
        {
            if (!obj) return "<None>";

            // 盡量用 DisplayName（你這邊 BuildConfigSO / PlayConfigSO 都有）
            var prop = obj.GetType().GetProperty("DisplayName");
            if (prop != null && prop.PropertyType == typeof(string))
            {
                try
                {
                    var v = prop.GetValue(obj) as string;
                    if (!string.IsNullOrEmpty(v)) return v;
                }
                catch { /* ignore */ }
            }

            return obj.name;
        }

        private static void DrawRightSideMiniButtons(UnityEngine.Object target, Action onPing, Action onSelect)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(!target))
                {
                    if (GUILayout.Button("Ping", GUILayout.Width(60))) onPing?.Invoke();
                    if (GUILayout.Button("Select", GUILayout.Width(60))) onSelect?.Invoke();
                }
            }
        }

        // -------------------------
        // WorkCopy helpers
        // -------------------------
        private void EnsureBuildWorkCopy(BuildConfigSO asset)
        {
            if (!asset) return;

            if (!buildWork)
            {
                buildWork = CreateWorkCopy(asset);
                buildSO = buildWork ? new SerializedObject(buildWork) : null;
                buildDirty = false;
                return;
            }

            if (buildSO == null || buildSO.targetObject != buildWork)
                buildSO = new SerializedObject(buildWork);
        }

        private void EnsurePlayWorkCopy(PlayConfigSO asset)
        {
            if (!asset) return;

            if (!playWork)
            {
                playWork = CreateWorkCopy(asset);
                playSO = playWork ? new SerializedObject(playWork) : null;
                playDirty = false;
                return;
            }

            if (playSO == null || playSO.targetObject != playWork)
                playSO = new SerializedObject(playWork);
        }

        private void ResetBuildWorkCopy(BuildConfigSO asset)
        {
            DestroyWorkCopy(buildWork);
            buildWork = CreateWorkCopy(asset);
            buildSO = buildWork ? new SerializedObject(buildWork) : null;
            buildDirty = false;
        }

        private void ResetPlayWorkCopy(PlayConfigSO asset)
        {
            DestroyWorkCopy(playWork);
            playWork = CreateWorkCopy(asset);
            playSO = playWork ? new SerializedObject(playWork) : null;
            playDirty = false;
        }

        private static T CreateWorkCopy<T>(T src) where T : ScriptableObject
        {
            if (!src) return null;

            var copy = Instantiate(src);
            copy.name = src.name;
            copy.hideFlags = HideFlags.HideAndDontSave;
            return copy;
        }

        private static void DestroyWorkCopy(UnityEngine.Object obj)
        {
            if (obj) DestroyImmediate(obj);
        }

        // -------------------------
        // Apply (+ auto update Current*Config asset)
        // -------------------------
        private static void ApplyWorkToAsset(ScriptableObject work, ScriptableObject asset)
        {
            if (!work || !asset) return;

            // 1) 把 WorkCopy 序列化欄位整包拷回資產（含子類新增欄位）
            EditorUtility.CopySerialized(work, asset);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();

            // 2) 自動更新 Current*Config（沒有就建，有就覆蓋）
            if (asset is BuildConfigSO b) SetCurrentBuildConfig(b);
            else if (asset is PlayConfigSO p) SetCurrentPlayConfig(p);
        }

        private static void SetCurrentBuildConfig(BuildConfigSO config)
        {
            if (!config) return;

            var current = EnsureOrCreateSingletonAsset<CurrentBuildConfig>(CurrentBuildConfigAssetPath);
            current.so = config;
            current.applierSO = config.applier;

            EditorUtility.SetDirty(current);
            AssetDatabase.SaveAssets();
        }

        private static void SetCurrentPlayConfig(PlayConfigSO config)
        {
            if (!config) return;

            var current = EnsureOrCreateSingletonAsset<CurrentPlayConfig>(CurrentPlayConfigAssetPath);
            current.so = config;
            current.applierSO = config.applier;

            EditorUtility.SetDirty(current);
            AssetDatabase.SaveAssets();
        }

        private static T EnsureOrCreateSingletonAsset<T>(string assetPath) where T : ScriptableObject
        {
            // 先嘗試從指定路徑載入（確保用固定那顆）
            var existed = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (existed) return existed;

            // 若路徑沒檔案，再找專案內是否已經有人建過（有就沿用第一顆）
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids != null && guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var found = AssetDatabase.LoadAssetAtPath<T>(path);
                if (found) return found;
            }

            // 都沒有：建立資料夾 + 建 asset
            EnsureFolderForAssetPath(assetPath);

            var created = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(created, assetPath);
            AssetDatabase.SaveAssets();
            return created;
        }

        private static void EnsureFolderForAssetPath(string assetPath)
        {
            // assetPath: Assets/Resources/XPlan/BuildTools/xxx.asset
            var dir = System.IO.Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
            if (string.IsNullOrEmpty(dir)) return;

            var parts = dir.Split('/');
            if (parts.Length == 0) return;

            string cur = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }

        // -------------------------
        // SerializedObject draw
        // -------------------------
        private static void DrawAllProperties(SerializedObject so)
        {
            if (so == null) return;

            var it = so.GetIterator();
            bool enterChildren = true;

            while (it.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (it.propertyPath == "m_Script") continue;
                EditorGUILayout.PropertyField(it, includeChildren: true);
            }
        }
    }
}
#endif
