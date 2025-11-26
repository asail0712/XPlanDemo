using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

using XPlan.Utility;

namespace XPlan.UI.Components
{
    [Serializable]
    public class ImageMapper
    {
        [SerializeField] private Image image;
        [SerializeField] private string fileName;

        [SerializeField] private Dictionary<string, Sprite> imageCache;

        private MonoBehaviourHelper.MonoBehavourInstance mbIns;

        public ImageMapper(Image image, string fileName)
        {
            this.image      = image;
            this.fileName   = fileName;
            this.imageCache = new Dictionary<string, Sprite>();
            this.mbIns      = null;
        }

        public void Refresh()
        {
            int currLang    = StringTable.Instance.CurrLanguage;
            string loadName = $"{fileName}_{currLang}.png";

            // StreamingAssets 完整路徑
            string fullPath = Path.Combine(Application.streamingAssetsPath, "I18N", loadName);

            // 先檢查 Cache
            if (imageCache.TryGetValue(fullPath, out Sprite cachedSp))
            {
                image.sprite = cachedSp;
                return;
            }

            if (mbIns != null)
            {
                mbIns.StopCoroutine();
                mbIns = null;
            }

            mbIns = MonoBehaviourHelper.StartCoroutine(LoadAndApply(fullPath));
        }

        private IEnumerator LoadAndApply(string fullPath)
        {
            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(fullPath))
            {
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[ImageMapper] 載入失敗: {fullPath}");
                    yield break;
                }

                Texture2D tex = DownloadHandlerTexture.GetContent(req);

                // 轉成 Sprite
                Sprite sp = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f // Pixel Per Unit 可依 UI 設定調
                );

                imageCache[fullPath]    = sp;   // 存入 Cache
                image.sprite            = sp;   // 套用
            }
        }
    }

    public class I18NSpriteProvider : MonoBehaviour
    {
        [SerializeField] private List<ImageMapper> imgMapper;

        public void Register(Image img, string fileName)
        {
            if (img == null || string.IsNullOrEmpty(fileName)) return;

            imgMapper       ??= new List<ImageMapper>();
            ImageMapper tmp = new ImageMapper(img, fileName);
            tmp.Refresh();

            imgMapper.Add(tmp);
        }

        public void RefreshImage()
        {
            imgMapper?.ForEach(e04 => e04.Refresh());
        }
    }
}