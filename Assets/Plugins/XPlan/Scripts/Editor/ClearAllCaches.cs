using UnityEditor;
using UnityEngine;

#if ADDRESSABLES_EXISTS
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif //ADDRESSABLES_EXISTS

namespace XPlan.Editors
{
    public class ClearAllCaches : MonoBehaviour
    {
        [MenuItem("XPlanTools/Clear All Cache")]
        public static void ClearAllCache()
        {
            bool cacheCleared = Caching.ClearCache(); // 清除全部本地快取（包括 Addressables）
            Debug.Log($"🧹 快取清除結果：{cacheCleared}");

#if ADDRESSABLES_EXISTS
            // 這行可清除 Addressables 的載入狀態
            Addressables.CleanBundleCache();
            Debug.Log("Addressables 缓存已清除");
#endif //ADDRESSABLES_EXISTS
        }
    }
}