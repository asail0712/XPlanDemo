using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if AR_FOUNDATION
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;
#endif //AR_FOUNDATION

using XPlan.Utility;

namespace XPlan.AR
{
	public static class ARUtility
	{
		// 參考資料
		// https://github.com/Unity-Technologies/arfoundation-samples/issues/1086
		// 必須在載入場景前呼叫
		static public void XRReset()
		{
#if AR_FOUNDATION
			XRGeneralSettings xrSetting = XRGeneralSettings.Instance;
			if (xrSetting != null)
			{
				xrSetting.Manager.StopSubsystems();
				xrSetting.Manager.DeinitializeLoader();

				LogSystem.Record("XR Disabled", LogType.Log);
			}

			if (xrSetting != null)
			{
				xrSetting.Manager.InitializeLoaderSync();
				xrSetting.Manager.StartSubsystems();

				LogSystem.Record("XR Enabled", LogType.Log);
			}
#endif //AR_FOUNDATION
		}

		static public void CheckARSupport(Action<bool> finishAction)
		{
			MonoBehaviourHelper.StartCoroutine(CheckARSupport_Internal(finishAction));
		}

		static private IEnumerator CheckARSupport_Internal(Action<bool> finishAction)
		{
			bool bResult = false;

			LogSystem.Record("Check for AR Support", LogType.Log);

#if UNITY_EDITOR
			yield return new WaitForEndOfFrame();

			LogSystem.Record("Windows support AR", LogType.Log);

			bResult = true;
#elif AR_FOUNDATION
			yield return ARSession.CheckAvailability();

			switch (ARSession.state)
			{
				case ARSessionState.Unsupported:
					LogSystem.Record("Your device does Not support AR", LogType.Warning);
					bResult = false;
					break;
				case ARSessionState.NeedsInstall:
					LogSystem.Record("Your device Need To Install", LogType.Warning);
					bResult = false;
					break;
				case ARSessionState.Ready:
					LogSystem.Record("Your device support AR", LogType.Log);
					bResult = true;
					break;
			}
#endif
      finishAction?.Invoke(bResult);
		}
	}
}
