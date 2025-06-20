using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XPlan.Editors
{
    public class UnusedAssetFinder : EditorWindow
    {
        private List<string> unusedImages   = new List<string>();
        private List<string> unusedFBXs     = new List<string>();
        private List<string> unusedPrefabs  = new List<string>();
        private Vector2 scrollPos;
        private bool checkOnlyBuildIncludedAssets   = true;
        private bool checkUnusedFBX                 = false;
        private bool checkUnusedImage               = true;
        private bool checkUnusedPrefab              = false;

        [MenuItem("XPlanTools/Resource/Find Unused Assets")]
        public static void ShowWindow()
        {
            GetWindow<UnusedAssetFinder>("Unused Asset Finder");
        }

        private void OnGUI()
        {
            checkOnlyBuildIncludedAssets    = GUILayout.Toggle(checkOnlyBuildIncludedAssets, "只檢查會被包進 APK 的資產");
            checkUnusedFBX                  = GUILayout.Toggle(checkUnusedFBX, "檢查未使用的 FBX 模型");
            checkUnusedImage                = GUILayout.Toggle(checkUnusedImage, "檢查未使用的 Image 模型");
            checkUnusedPrefab               = GUILayout.Toggle(checkUnusedPrefab, "檢查未使用的 Prefab");

            if (GUILayout.Button("開始掃描未使用的資產"))
            {
                FindUnusedAssets();
            }

            if (unusedImages.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("🖼️ 未使用的圖片:", EditorStyles.boldLabel);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(250));
                foreach (string path in unusedImages)
                { 
                    EditorGUILayout.LabelField(path);
                }
                EditorGUILayout.EndScrollView();
            }

            if (checkUnusedFBX && unusedFBXs.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("📦 未使用的 FBX 模型:", EditorStyles.boldLabel);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(250));
                foreach (string path in unusedFBXs)
                { 
                    EditorGUILayout.LabelField(path);
                }
                EditorGUILayout.EndScrollView();
            }

            if (checkUnusedPrefab && unusedPrefabs.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("🧱 未使用的 Prefab:", EditorStyles.boldLabel);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(250));
                foreach (string path in unusedPrefabs)
                {
                    EditorGUILayout.LabelField(path);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void FindUnusedAssets()
        {
            unusedImages.Clear();
            unusedFBXs.Clear();

            // 取得所有圖片
            string[] allTextures = AssetDatabase.FindAssets("t:Texture")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !p.StartsWith("Packages/"))
                .ToArray();

            // 取得所有 FBX（Model）
            string[] allFBXs = checkUnusedFBX
                ? AssetDatabase.FindAssets("t:Model")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(p => p.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                    .ToArray()
                : new string[0];

            string[] allPrefabs = checkUnusedPrefab
                ? AssetDatabase.FindAssets("t:Prefab")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(p => !p.StartsWith("Packages/"))
                    .ToArray()
                : new string[0];

            // 收集參考資產
            HashSet<string> referencedAssets = new HashSet<string>();

            if (checkOnlyBuildIncludedAssets)
            {
                string[] buildScenes = EditorBuildSettings.scenes
                    .Where(s => s.enabled)
                    .Select(s => s.path).ToArray();

                string[] dependencies = EditorUtility.CollectDependencies(
                    buildScenes.Select(AssetDatabase.LoadAssetAtPath<SceneAsset>).ToArray()
                ).Select(AssetDatabase.GetAssetPath)
                 .Where(path => !string.IsNullOrEmpty(path))
                 .ToArray();

                foreach (string dep in dependencies)
                {
                    referencedAssets.Add(dep);
                }
            }
            else
            {
                string[] allAssets = AssetDatabase.GetAllAssetPaths();
                foreach (string asset in allAssets)
                {
                    string[] deps = AssetDatabase.GetDependencies(asset);
                    foreach (string dep in deps)
                    { 
                        referencedAssets.Add(dep);
                    }
                }
            }

            // 篩選未使用的資產
            if (checkUnusedImage)
            { 
                unusedImages = allTextures.Where(tex => !referencedAssets.Contains(tex)).ToList();
            }
            if (checkUnusedFBX)
            { 
                unusedFBXs = allFBXs.Where(fbx => !referencedAssets.Contains(fbx)).ToList();
            }
            if (checkUnusedPrefab)
            {
                unusedPrefabs = allPrefabs.Where(pf => !referencedAssets.Contains(pf)).ToList();
            }

            Debug.Log($"✅ 掃描完成，共找到 {unusedImages.Count} 張未使用圖片，{unusedFBXs.Count} 個未使用 FBX，{unusedPrefabs.Count} 個未使用 Prefab。");
        }
    }
}
