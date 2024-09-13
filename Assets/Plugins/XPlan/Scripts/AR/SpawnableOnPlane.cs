using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if AR
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif //AR
// 參考資料
// https://www.youtube.com/watch?v=Fpw7V3oa4fs
// https://www.youtube.com/watch?v=yrbs0l6FZxI

namespace XPlan.AR
{ 
    public class SpawnableOnPlane : MonoBehaviour
    {
#if AR
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private float duration = 1f;

        private List<ARRaycastHit> hitList;
        private Coroutine tickToDetectCoroutine;

		private void Awake()
		{
            hitList = new List<ARRaycastHit>();
        }

		private void Start()
        {
            if(tickToDetectCoroutine != null)
			{
                StopCoroutine(tickToDetectCoroutine);
                tickToDetectCoroutine = null;
            }

            tickToDetectCoroutine = StartCoroutine(TickToDetect());
        }

		private void OnEnable()
		{
            if (tickToDetectCoroutine != null)
            {
                StopCoroutine(tickToDetectCoroutine);
                tickToDetectCoroutine = null;
            }

            tickToDetectCoroutine = StartCoroutine(TickToDetect());
        }

        private void OnDisable()
        {
            if (tickToDetectCoroutine != null)
            {
                StopCoroutine(tickToDetectCoroutine);
                tickToDetectCoroutine = null;
            }
        }

        private void OnDestroy()
		{
            if (tickToDetectCoroutine != null)
            {
                StopCoroutine(tickToDetectCoroutine);
                tickToDetectCoroutine = null;
            }
		}

		// Update is called once per frame
		private IEnumerator TickToDetect()
        {
            while(true)
            {
                yield return new WaitForSeconds(duration);
                yield return new WaitUntil(() => ARSession.state == ARSessionState.SessionTracking);

                // 获取屏幕中央下方 1/3 的位置
                Vector2 screenCenter    = new Vector2(Screen.width / 2, Screen.height / 3);
                Vector3 hitPos          = Vector3.zero;
                bool bFind              = raycastManager.Raycast(screenCenter, hitList, TrackableType.Planes);
                if (bFind)
                {
                    hitPos = hitList[0].pose.position;
                }

                XARPlaneMsg msg = new XARPlaneMsg(bFind, hitPos);

                msg.Send();
            }
        }
#endif //AR
    }
}
