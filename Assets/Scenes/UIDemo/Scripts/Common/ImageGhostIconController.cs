using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using XPlan;

namespace Demo.Inventory
{
    [Serializable]
    public class IconInfo
    {
        public string iconKey;
        public Sprite icon;
        public Vector2 size;
        public float alpha;
    }

    public class ImageGhostIconController : MonoBehaviour, IGhostIconController
    {
        [SerializeField] private GameObject ghostIcon;
        [SerializeField] private float lerpSpeed            = 20f; // 越大越快
        [SerializeField] private List<IconInfo> iconList    = new List<IconInfo>();

        private Image ghostIconImg          = null;
        private RectTransform rectTransform = null;
        private Coroutine moveRoutine       = null;

        private void Awake()
        {
            ghostIconImg    = ghostIcon.GetComponent<Image>();
            rectTransform   = ghostIcon.GetComponent<RectTransform>();
        }

        // ===============================
        // 實作 IGhostIconController
        // ===============================

        public void Hide()
        {
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
                moveRoutine = null;
            }

            ghostIcon.SetActive(false);
        }

        public void Move(Vector2 screenPos)
        {
            if (!ghostIcon.activeSelf)
                return;

            if (moveRoutine != null)
                StopCoroutine(moveRoutine);

            moveRoutine = StartCoroutine(LerpMove(screenPos));
        }

        public void Show(Vector2 screenPos)
        {
            rectTransform.position = screenPos;

            ghostIcon.SetActive(true);
        }

        public async Task SnapBackTo(Vector2 startScreenPos)
        {
            Move(startScreenPos);

            await Task.Delay(100);
        }

        public void Bind(IGhostPayload ghostPayload)
        {
            if (ghostPayload is GhostSpritePayload p)
            {
                IconInfo info = iconList.FirstOrDefault(e04 => e04.iconKey == p.iconKey);

                if(info == null)
                {
                    return;
                }

                ghostIconImg.sprite     = info.icon;
                //rectTransform.sizeDelta = info.size;

                var c                   = ghostIconImg.color; 
                c.a                     = info.alpha; 
                ghostIconImg.color      = c;
            }
        }

        // ===============================
        // Coroutine
        // ===============================
        private IEnumerator LerpMove(Vector2 targetScreenPos)
        {
            // 直接用 screen space（Overlay Canvas 最穩）
            Vector3 targetWorldPos = targetScreenPos;

            while (Vector3.Distance(rectTransform.position, targetWorldPos) > 0.1f)
            {
                yield return null;

                rectTransform.position = Vector3.Lerp(
                    rectTransform.position,
                    targetWorldPos,
                    Time.deltaTime * lerpSpeed
                );
            }

            rectTransform.position  = targetWorldPos;
            moveRoutine             = null;
        }
    }

    public class GhostSpritePayload : IGhostPayload
    {
        public readonly string iconKey;
        public GhostSpritePayload(string IconKey)
        {
            iconKey = IconKey;
        }
    }
}
