using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace XPlan.Components
{
    public class ImageSequencePlayer : MonoBehaviour
    {
        [Header("Image 設定")] 
        [SerializeField] private Image targetImage;
        
        [Header("設定")]
        [SerializeField] private float frameDuration        = 0.1f; // 每張顯示時間（秒）
        [SerializeField] private bool loop                  = true;
        [SerializeField] private bool bAutoAdjustmentSize   = true;

        [Header("播放圖片")]
        [SerializeField] private Sprite[] frames;

        private Coroutine playCoroutine;

        private void OnEnable()
        {
            if (frames.Length > 0 && targetImage != null)
            {
                playCoroutine = StartCoroutine(PlaySequence());
            }
        }

        private void OnDisable()
        {
            if (playCoroutine != null)
            {
                StopCoroutine(playCoroutine);
            }
        }

        private IEnumerator PlaySequence()
        {
            int index = 0;

            while (true)
            {
                targetImage.sprite = frames[index];

                // 自動調整 Image 尺寸符合原始圖片
                if(bAutoAdjustmentSize)
                { 
                    RectTransform rt = targetImage.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.sizeDelta = new Vector2(frames[index].texture.width, frames[index].texture.height);
                    }
                }

                yield return new WaitForSeconds(frameDuration);

                index++;

                if (index >= frames.Length)
                {
                    if (loop)
                    { 
                        index = 0;
                    }
                    else
                    { 
                        yield break;
                    }
                }
            }
        }
    }
}