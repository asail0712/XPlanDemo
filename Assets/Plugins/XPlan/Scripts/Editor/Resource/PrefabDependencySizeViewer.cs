using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XPlan.Editors
{
    public class PrefabDependencySizeViewer
    {
        class AssetSizeInfo
        {
            public string Path;
            public string Type;
            public long Size;
        }

        [MenuItem("XPlanTools/Resource/Analyze Selected Prefab Dependencies")]
        public static void Analyze()
        {
            Object selected = Selection.activeObject;
            if (selected == null)
            {
                Debug.LogError("請先選 Prefab");
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(selected);
            if (string.IsNullOrEmpty(prefabPath) || !prefabPath.EndsWith(".prefab"))
            {
                Debug.LogError("目前選到的不是 Prefab");
                return;
            }

            Object prefab           = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
            Object[] dependencies   = EditorUtility.CollectDependencies(new Object[] { prefab });

            HashSet<string> uniquePaths = new HashSet<string>();
            List<AssetSizeInfo> results = new List<AssetSizeInfo>();

            foreach (Object dep in dependencies)
            {
                if (dep == null) continue;

                string path = AssetDatabase.GetAssetPath(dep);
                if (string.IsNullOrEmpty(path)) continue;
                if (Directory.Exists(path)) continue;
                if (!uniquePaths.Add(path)) continue;

                Object asset    = AssetDatabase.LoadAssetAtPath<Object>(path);
                long size       = GetFileSize(path);

                results.Add(new AssetSizeInfo
                {
                    Path = path,
                    Type = asset != null ? asset.GetType().Name : "Unknown",
                    Size = size
                });
            }

            results = results.OrderByDescending(x => x.Size).ToList();

            Debug.Log($"Prefab: {prefabPath}");
            Debug.Log($"依賴數量: {results.Count}");
            Debug.Log($"總大小: {FormatBytes(results.Sum(x => x.Size))}");

            foreach (var item in results)
            {
                Debug.Log($"{item.Type} | {FormatBytes(item.Size)} | {item.Path}");
            }
        }

        static long GetFileSize(string assetPath)
        {
            string fullPath = Path.GetFullPath(assetPath);
            if (!File.Exists(fullPath))
                return 0;

            return new FileInfo(fullPath).Length;
        }

        static string FormatBytes(long bytes)
        {
            string[] sizes  = { "B", "KB", "MB", "GB" };
            double len      = bytes;
            int order       = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}