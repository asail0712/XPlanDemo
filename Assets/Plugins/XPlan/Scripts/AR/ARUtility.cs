using System;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.Management;

using XPlan;

namespace XPlan.AR
{    
    public static class ARUtility
    {
		// 參考資料
		// https://github.com/Unity-Technologies/arfoundation-samples/issues/1086
		// 必須在載入場景前呼叫
		static public void XRReset()
		{
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
		}
	}
}
