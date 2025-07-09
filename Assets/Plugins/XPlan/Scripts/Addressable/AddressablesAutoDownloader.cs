using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

#if ADDRESSABLES_EXISTS
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif //ADDRESSABLES_EXISTS

using XPlan.Utility;

namespace XPlan.Addressable
{
    /// <summary>
    /// 自動下載 Addressables 資源的工具類別
    /// </summary>
    /// <remarks>
    /// 這個類別會自動下載所有 Addressables 資源，並提供進度回調和錯誤處理。
    /// </remarks>
    /// </summary>

    public class AddressablesAutoDownloader : MonoBehaviour
    {
        [SerializeField] public bool bAutoStart     = true;
        [SerializeField] public List<string> keys   = new List<string>();

        public Action<string, float> OnEachProgress;    // 資源名稱, 進度
        public Action<string> OnEachDone;               // 資源名稱
        public Action OnAllDone;
        public Action<string> OnError;

        private List<string> pendingKeys    = new List<string>();
        private bool bLoadingFinish         = false;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            if (bAutoStart)
            {
                CheckAndUpdateCatalog((b) => 
                {
                    DownloadAllAssets();
                });                
            }
        }

        public void CheckAndUpdateCatalog(Action<bool> finishAction)
        {
            StartCoroutine(CheckAndUpdateCatalog_Internal(finishAction));
        }

        public void DownloadAllAssets(float delay = 0f)
        {            
            StartCoroutine(DownloadAllRoutine_Internal(delay));
        }

        public void ResetLoader()
        {
            bLoadingFinish = false;
            keys.Clear();
            pendingKeys.Clear();

            OnEachProgress  = null;
            OnEachDone      = null;
            OnAllDone       = null;
            OnError         = null;
        }

        public bool IsAllDone()
        {
            return bLoadingFinish;
        }

        private IEnumerator CheckAndUpdateCatalog_Internal(Action<bool> finishAction)
        {
#if ADDRESSABLES_EXISTS
            var checkHandle = Addressables.CheckForCatalogUpdates();

            yield return checkHandle;

            if (checkHandle.Result.Count > 0)
            {
                var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result);
                yield return updateHandle;

                Debug.Log("Catalog 已更新");

                finishAction?.Invoke(true);
            }
            else
            {
                Debug.Log("Catalog 無需更新");

                finishAction?.Invoke(false);
            }
#endif //ADDRESSABLES_EXISTS
            yield return null;
        }

        private IEnumerator DownloadAllRoutine_Internal(float delay)
        {
            yield return new WaitForSeconds(delay);

#if ADDRESSABLES_EXISTS
#if UNITY_EDITOR
            Addressables.ClearDependencyCacheAsync(keys.ToArray());
#endif //UNITY_EDITOR

            // Step 1: 取得所有資源 key
            var locHandle = Addressables.LoadResourceLocationsAsync(keys.ToArray(), Addressables.MergeMode.Union);
            yield return locHandle;

            if (locHandle.Status != AsyncOperationStatus.Succeeded)
            {
                OnError?.Invoke("❌ 無法取得資源清單");
                yield break;
            }

            var locations = locHandle.Result;
            foreach (var loc in locations)
            {
                pendingKeys.AddUnique(loc.PrimaryKey);
            }

            // Step 2: 遍歷每一個資源，檢查是否需要下載
            foreach (var key in pendingKeys)
            {
                var sizeHandle = Addressables.GetDownloadSizeAsync(key);
                yield return sizeHandle;

                if (sizeHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    OnError?.Invoke($"📦 無法取得 {key} 的大小");

                    Addressables.Release(sizeHandle);
                    continue;
                }

                if (sizeHandle.Result == 0)
                {
                    Debug.Log($"✅ {key} 已經快取");
                    //yield return LoadAsset(key);
                    Addressables.Release(sizeHandle);

                    OnEachDone?.Invoke(key);
                    continue;
                }

                // Step 3: 下載
                var downloadHandle = Addressables.DownloadDependenciesAsync(key);
                while (!downloadHandle.IsDone)
                {
                    OnEachProgress?.Invoke(key, downloadHandle.PercentComplete);
                    yield return null;
                }

                if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log($"✅ {key} 下載完成");
                    Addressables.Release(downloadHandle);

                    OnEachDone?.Invoke(key);
                    //yield return LoadAsset(key);
                }
                else
                {
                    Addressables.Release(downloadHandle);

                    OnError?.Invoke($"❌ {key} 下載失敗");
                }

                Addressables.Release(sizeHandle);
            }

            bLoadingFinish = true;

            OnAllDone?.Invoke();
            Addressables.Release(locHandle);
#endif //ADDRESSABLES_EXISTS
            yield return null;
        }

        static public void LoadAsset<T>(string key, Action<T> finishAction)
        {
#if ADDRESSABLES_EXISTS
            var loadHandle = Addressables.LoadAssetAsync<GameObject>(key);

            loadHandle.Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    // 3. 載入成功，產生物件
                    finishAction?.Invoke((T)(object)handle.Result);
                }
                else
                {
#endif //ADDRESSABLES_EXISTS
                    finishAction?.Invoke(default(T));
#if ADDRESSABLES_EXISTS
                }
            };
#endif //ADDRESSABLES_EXISTS
        }
    }
}
