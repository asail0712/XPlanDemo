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
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

#if ADDRESSABLES_EXISTS
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement;
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

        [Header("CCD 設定")]
        [Tooltip("設定 badge 名稱，如 stable、latest、review，空字串表示不使用動態 badge")]
        [SerializeField] public string ccdBadge     = "<你的Badge設定>";    // 空字串表示不用動態 badge

        [Tooltip("CCD 環境")]
        [SerializeField] public string ccdEnvirment = "<你的CCD環境>";

        [Tooltip("CCD 專案 ID")]
        [SerializeField] public string ccdProjectId = "<你的CCD專案ID>";

        [Tooltip("CCD Bucket ID")]
        [SerializeField] public string ccdBucketId  = "<你的BucketID>";

        [Tooltip("是否指定Badge")]
        [SerializeField] public bool bUsedBadge     = false;

        public Action<float> OnEachProgress;    // 資源名稱, 進度
        public Action OnAllDone;
        public Action<string> OnError;

        private List<string> pendingKeys    = new List<string>();
        private bool bLoadingFinish         = false;

#if ADDRESSABLES_EXISTS  
        private IResourceLocator _loadedCatalogLocator = null;
#endif // ADDRESSABLES_EXISTS  
        private void Awake()
        {
            DontDestroyOnLoad(this);
#if ADDRESSABLES_EXISTS  
            // 將Catalog裡面的badge由latest更換成指定的badge
            Addressables.InternalIdTransformFunc = (location) =>
            {            
                var id = location.InternalId;
                
                if (bUsedBadge)
                {
                    // 覆蓋 2 種常見寫法：release_by_badge/latest 以及 release_by_badge/<任一值>
                    id = id.Replace("/release_by_badge/latest/", $"/release_by_badge/{ccdBadge}/");
                }
                
                // 若你過去打包時用的是固定 release_id，也可以在這裡做對應改寫（選擇性）
                return id;
            };
#endif //ADDRESSABLES_EXISTS  
        }

        private void Start()
        {
            if (bAutoStart)
            {
                CheckAndUpdateCatalog((b) => 
                {
                    if(!b)
                    {
                        return;
                    }

                    DownloadAllAssets();
                });                
            }
        }

        public void CheckAndUpdateCatalog(Action<bool> finishAction)
        {
            if (bUsedBadge)
            {
                // 有設定 badge，走動態載入指定 badge catalog 流程
                StartCoroutine(LoadCatalogByBadgeAndStart(finishAction));
            }
            else
            {
                // 沒設定 badge，走原本 CCD 自動檢查更新流程
                StartCoroutine(CheckAndUpdateLatestCatalog(finishAction));
            }
        }

        private IEnumerator LoadCatalogByBadgeAndStart(Action<bool> finishAction)
        {
#if ADDRESSABLES_EXISTS            
            // 若之前載過，先移除舊的（只移除你自己加的）
            if (_loadedCatalogLocator != null)
            {
                Addressables.RemoveResourceLocator(_loadedCatalogLocator);
                _loadedCatalogLocator = null;
            }

            string catalogUrl = GetCatalogUrl(ccdProjectId, ccdBucketId, ccdBadge);
            Debug.Log($"載入 CCD catalog（badge: {ccdBadge}）: {catalogUrl}");

            var handle = Addressables.LoadContentCatalogAsync(catalogUrl, false);
            yield return handle;

            // 先檢查有效性
            if (!handle.IsValid())
            {
                Debug.LogError("❌ Catalog handle 已失效（可能被提前釋放）");
                finishAction?.Invoke(false);
                yield break;
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"✅ Catalog 載入成功：{ccdBadge}");
                _loadedCatalogLocator = handle.Result;
                finishAction?.Invoke(true);
            }
            else
            {
                Debug.LogError($"❌ Catalog 載入失敗：{catalogUrl}");
                finishAction?.Invoke(false);
            }

            // 手動釋放並清空
            Addressables.Release(handle);
#endif
            yield return null;
        }

        private string GetCatalogUrl(string projectId, string bucketId, string badge)
        {
            string catalogFileName = "catalog_" + Application.version + ".hash";

            return $"https://{projectId}.client-api.unity3dusercontent.com/client_api/v1/environments/{ccdEnvirment}/buckets/{bucketId}/release_by_badge/{badge}/entry_by_path/content/?path=" + catalogFileName;
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
            OnAllDone       = null;
            OnError         = null;
        }

        public bool IsAllDone()
        {
            return bLoadingFinish;
        }

        private IEnumerator CheckAndUpdateLatestCatalog(Action<bool> finishAction)
        {
#if ADDRESSABLES_EXISTS
            var checkHandle = Addressables.CheckForCatalogUpdates();

            yield return checkHandle;

            if (checkHandle.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError($"❌ Catalog 檢查失敗：{checkHandle.OperationException}");                
                finishAction?.Invoke(false);

                yield break;
            }

            if (checkHandle.Result.Count > 0)
            {
                var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result);
                yield return updateHandle;

                Addressables.Release(updateHandle);
                Debug.Log("Catalog 已更新");

                finishAction?.Invoke(true);
            }
            else
            {
                Debug.Log("Catalog 無需更新");

                finishAction?.Invoke(true);
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

            Debug.Log($"✅ 成功取得清單");

            var locations = locHandle.Result;
            foreach (var loc in locations)
            {
                pendingKeys.AddUnique(loc.PrimaryKey);
            }

            // Step 2: 檢查是否需要下載
            var sizeHandle = Addressables.GetDownloadSizeAsync(pendingKeys);
            yield return sizeHandle;

            if (sizeHandle.Status != AsyncOperationStatus.Succeeded)
            {
                OnError?.Invoke($"無法取得 下載大小");

                Addressables.Release(sizeHandle);
                yield break;
            }

            if (sizeHandle.Result <= 0)
            {
                Debug.Log($"已經快取");
                //yield return LoadAsset(key);
                Addressables.Release(sizeHandle);

                yield break;
            }

            // Step 3: 下載
            var downloadHandle = Addressables.DownloadDependenciesAsync(pendingKeys, Addressables.MergeMode.Union);
            while (!downloadHandle.IsDone)
            {
                OnEachProgress?.Invoke(downloadHandle.PercentComplete);
                yield return null;
            }

            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"下載完成");
                Addressables.Release(downloadHandle);
            }
            else
            {
                Addressables.Release(downloadHandle);
                OnError?.Invoke($"❌下載失敗");

                yield break;
            }

            bLoadingFinish = true;
            OnAllDone?.Invoke();

            Addressables.Release(sizeHandle);
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
