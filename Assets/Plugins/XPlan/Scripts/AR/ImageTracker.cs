using System;
using System.Collections.Generic;
using UnityEngine;

#if AR
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif //AR

// 參考資料
// https://www.youtube.com/watch?v=Fpw7V3oa4fs

namespace XPlan.AR
{
	public class ImageTracker : MonoBehaviour
	{
#if AR
		[SerializeField] private ARTrackedImageManager trackedImageMgr;

		private void OnEnable()
		{
			trackedImageMgr.trackedImagesChanged += OnTrakedImgChanged;
		}

		private void OnDisable()
		{
			trackedImageMgr.trackedImagesChanged -= OnTrakedImgChanged;
		}

		private void OnTrakedImgChanged(ARTrackedImagesChangedEventArgs eventArgs)
		{
			foreach (ARTrackedImage imgTracker in eventArgs.added)
			{
				string imgKey			= imgTracker.referenceImage.name;
				Vector3 spawnPos		= imgTracker.transform.position;
				XARModelSpawnMsg msg	= new XARModelSpawnMsg(imgKey, spawnPos);

				msg.Send();
			}

			foreach (ARTrackedImage imgTracker in eventArgs.updated)
			{
				string imgKey			= imgTracker.referenceImage.name;
				bool bOn				= imgTracker.trackingState == TrackingState.Tracking;
				Vector3 spawnPos		= imgTracker.transform.position;
				XARModelTrackMsg msg	= new XARModelTrackMsg(imgKey, bOn, spawnPos);

				msg.Send();
			}
		}
#endif //AR
	}
}
