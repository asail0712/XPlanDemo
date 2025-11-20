using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using XPlan.Recycle;
using XPlan.Utility;

namespace XPlan.UI
{
    public static class ImageUtils
    {
        static public void LoadImageFromUrl(RawImage targetImage, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                LogSystem.Record("避免使用空字串下載圖片", LogType.Warning);

                return;
            }

            MonoBehaviourHelper.StartCoroutine(LoadImageFromUrl_Internal(url, (texture) =>
            {
                targetImage.texture = texture;
            }));
        }

        static public void LoadImageFromUrl(Image targetImage, string url, bool bResize = true)
        {
            if (string.IsNullOrEmpty(url))
            {
                LogSystem.Record("避免使用空字串下載圖片", LogType.Warning);

                return;
            }

            MonoBehaviourHelper.StartCoroutine(LoadImageFromUrl_Internal(url, (texture) =>
            {
                if (texture == null)
                {
                    return;
                }

                // 建議：設定取樣
                texture.filterMode  = FilterMode.Bilinear; // 或 Point（像素風）
                texture.wrapMode    = TextureWrapMode.Clamp;

                Sprite sprite = Sprite.Create(
                                    texture,
                                    new Rect(0, 0, texture.width, texture.height),
                                    new Vector2(0.5f, 0.5f),
                                    100f // pixelsPerUnit
                                    );

                targetImage.sprite          = sprite;
                targetImage.preserveAspect  = true;

                if (bResize)
                {
                    // 自動調整 Image 尺寸符合原始圖片
                    RectTransform rt = targetImage.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.sizeDelta = new Vector2(texture.width, texture.height);
                    }
                }
            }));
        }

        static private IEnumerator LoadImageFromUrl_Internal(string url, Action<Texture2D> finishAction)
        {
            Texture2D texture = null;

            if (CacheManager.LoadFromCache<Texture2D>(url, out texture))
            {
                finishAction?.Invoke(texture);

                yield break;
            }

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                LogSystem.Record("載入圖片失敗: " + request.error, LogType.Error);

                texture = null;
            }
            else
            {
                texture = DownloadHandlerTexture.GetContent(request);

                CacheManager.SaveToCache<Texture2D>(url, texture);
            }

            finishAction?.Invoke(texture);
        }

    }
}
