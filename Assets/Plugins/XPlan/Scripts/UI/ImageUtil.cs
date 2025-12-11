using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using XPlan.Recycle;
using XPlan.Utility;

namespace XPlan.UI
{
    public enum ResizeFormat
    {
        NoResize,
        MatchWidth,
        MatchHeight,
    }

    public static class ImageUtils
    {
        static public void LoadImageFromUrl(RawImage targetImage, string url, Action<Texture2D> finishAction = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                LogSystem.Record("避免使用空字串下載圖片", LogType.Warning);

                return;
            }

            MonoBehaviourHelper.StartCoroutine(LoadImageFromUrl_Internal(url, (texture) =>
            {
                targetImage.texture = texture;

                finishAction?.Invoke(texture);
            }));
        }

        static public void LoadImageFromUrl(Image targetImage, string url, Action<Sprite> finishAction = null, ResizeFormat resizeFormat = ResizeFormat.MatchWidth)
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

                Sprite sprite       = Sprite.Create(
                                                texture,
                                                new Rect(0, 0, texture.width, texture.height),
                                                new Vector2(0.5f, 0.5f),
                                                100f // pixelsPerUnit
                                            );

                targetImage.sprite          = sprite;
                targetImage.preserveAspect  = true;
                RectTransform rt            = targetImage.rectTransform;

                // ===== 依照 ResizeFormat 調整 RectTransform 尺寸 =====
                if (rt != null && resizeFormat != ResizeFormat.NoResize)
                {
                    float texW = texture.width;
                    float texH = texture.height;

                    if (texH <= 0f || texW <= 0f)
                    {
                        LogSystem.Record("LoadImageFromUrl：Texture 尺寸異常", LogType.Warning);
                    }
                    else
                    {
                        float aspect        = texW / texH;
                        Vector2 currentSize = rt.rect.size;
                        float newW          = currentSize.x;
                        float newH          = currentSize.y;

                        switch (resizeFormat)
                        {
                            case ResizeFormat.MatchWidth:
                                // 固定目前寬度，依比例算出高度
                                newW = currentSize.x;
                                newH = newW / aspect;
                                break;

                            case ResizeFormat.MatchHeight:
                                // 固定目前高度，依比例算出寬度
                                newH = currentSize.y;
                                newW = newH * aspect;
                                break;
                        }

                        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newW);
                        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newH);
                    }
                }

                finishAction?.Invoke(sprite);
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
