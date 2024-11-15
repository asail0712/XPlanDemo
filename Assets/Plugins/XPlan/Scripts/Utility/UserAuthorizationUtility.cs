using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_IOS
using System.Collections;
using UnityEngine.iOS;
#elif UNITY_ANDROID
using UnityEngine.Android;
#else
#endif

namespace XPlan.Utility
{ 
    public enum DevicePermission
	{
        Camera      = 0,
        Microphone,
    }


    public static class UserAuthorizationUtility
	{
		/*******************************************
		 * 相機權限
		 * *****************************************/
		static public void RequestAuthorization(DevicePermission devicePermission, Action<bool> finishAction)
		{
#if UNITY_IOS
            UserAuthorization authorization = UserAuthorization.WebCam;

			switch (devicePermission)
			{
                case DevicePermission.Camera:
                    authorization = UserAuthorization.WebCam;
                    break;
                case DevicePermission.Microphone:
                    authorization = UserAuthorization.Microphone;
                    break;
            }

			Debug.Log("判斷IOS是否有權限");
			if (!Application.HasUserAuthorization(authorization))
			{
				Debug.Log("開始要求IOS權限");
				MonoBehaviourHelper.StartCoroutine(RequestPermission(authorization, finishAction));
			}
			else
			{
				Debug.Log("一開始就有權限");
				finishAction?.Invoke(true);
			}
#elif UNITY_ANDROID
            string permission = Permission.Camera;

            switch (devicePermission)
            {
                case DevicePermission.Camera:
                    permission = Permission.Camera;
                    break;
                case DevicePermission.Microphone:
                    permission = Permission.Microphone;
                    break;
            }

            Debug.Log("判斷Android是否有權限");
            bool bHasCameraPermission = Permission.HasUserAuthorizedPermission(permission);

            if (!bHasCameraPermission)
            {
                Debug.Log("開始要求Android權限");
                // 如果沒有相機使用權限，則索取權限
                RequestPermission(permission, finishAction);
            }
            else
            {
                Debug.Log("一開始就有權限");
                finishAction?.Invoke(true);
            }
#else
            Debug.Log("無需判斷權限");
            finishAction?.Invoke(true);
#endif
        }

#if UNITY_IOS
        /****************************************
         * IOS 流程
         * *************************************/
        static private IEnumerator RequestPermission(UserAuthorization authorization, Action<bool> finishAction)
        {
            Debug.Log("IOS Request 相機權限");
            yield return Application.RequestUserAuthorization(authorization);

            if (Application.HasUserAuthorization(authorization))
            {
                Debug.Log("IOS 相機權限授予");
                finishAction?.Invoke(true);
            }
            else
            {
                Debug.Log("IOS 相機權限未被授予");

                // 開啟iOS要求權限頁面
                Application.OpenURL("app-settings:");

                finishAction?.Invoke(false);
            }
        }
#elif UNITY_ANDROID
        /****************************************
        * Abdroid 流程
        * *************************************/
        static private void RequestPermission(string permission, Action<bool> finishAction)
        {
            Debug.Log("Android Request 相機權限");

            PermissionCallbacks callbacks               = new PermissionCallbacks();

            callbacks.PermissionGranted                 += (permissionName) =>
            {
                Debug.Log($"{permissionName} PermissionCallbacks_PermissionGranted");

                finishAction.Invoke(true);
            };
            callbacks.PermissionDenied                  += (permissionName) => 
            {
                Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");

                finishAction.Invoke(false);
            };
            callbacks.PermissionDeniedAndDontAskAgain   += (permissionName) =>
            {
                Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");

                finishAction.Invoke(false);
            }; 

            // 索取相機使用權限
            Permission.RequestUserPermission(permission, callbacks);
        }

        static public void PermissionWhenDenied()
        {         
            AndroidJavaClass unityPlayer        = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity   = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaObject settingsIntent    = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS");
            string packageName                  = Application.identifier;

            AndroidJavaObject uriBuilder        = new AndroidJavaObject("android.net.Uri$Builder");
            uriBuilder.Call<AndroidJavaObject>("scheme", "package");
            uriBuilder.Call<AndroidJavaObject>("opaquePart", packageName);

            AndroidJavaObject uri               = uriBuilder.Call<AndroidJavaObject>("build");
            settingsIntent.Call<AndroidJavaObject>("setData", uri);

            currentActivity.Call("startActivity", settingsIntent);
		}
#else

#endif
    }
}