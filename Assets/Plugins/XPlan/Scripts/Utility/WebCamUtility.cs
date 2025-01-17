using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace XPlan.Utility
{
	public class WebCamController : MonoBehaviour
	{
		private WebCamTexture webCamTex;
		private Coroutine waitCameraCoroutine;
        private bool bDeviceReady;

        public int Width
        {
            get
            {
                return webCamTex.width;
            }
        }

        public int Height
        {
            get
            {
                return webCamTex.height;
            }
        }

        public float FPS
        {
            get
            {
                return webCamTex.requestedFPS;
            }
        }

        public void InitialController(WebCamTexture webCamTex)
		{
			this.webCamTex      = webCamTex;
            this.bDeviceReady   = false;
        }

		private void OnDestroy()
		{
			if(waitCameraCoroutine != null)
			{
                StopCoroutine(waitCameraCoroutine);
				waitCameraCoroutine = null;
			}

			if (webCamTex != null)
			{
				if(webCamTex.isPlaying)
				{
					Debug.LogWarning("Stop Web camera Texture");
					webCamTex.Stop();
				}

				Debug.LogWarning("Destory Web camera Texture");
				GameObject.DestroyImmediate(webCamTex);
				webCamTex = null;
			}
		}

        public IEnumerator InitialWithRawImage(RawImage camImg, bool bHighControllWidth = true)
        {
            if (camImg == null)
            {
                yield break;
            }

            // reset 原本的Texture
            if (camImg.texture != null && camImg.texture is WebCamTexture)
            {
                camImg.enabled = false;

                WebCamTexture webCamTexture = ((WebCamTexture)camImg.texture);
                webCamTexture.Stop();

                GameObject.DestroyImmediate(camImg.texture);
                camImg.texture = null;
            }

			camImg.texture = webCamTex;
			camImg.enabled = true;

			// 先調整Img大小
			FitImageSizeToCamSize(camImg, webCamTex, bHighControllWidth);

			// 翻轉處理
			RotationImg(camImg, webCamTex);

			yield return WaitCameraDeviceInitial(webCamTex);
		}

		public void Play()
		{            
            webCamTex.Play();
            waitCameraCoroutine = StartCoroutine(WaitCameraDeviceInitial(webCamTex));
        }

		public void Play(RawImage camImg, bool bHighControllWidth = true)
		{
			webCamTex.Play();
			waitCameraCoroutine = StartCoroutine(InitialWithRawImage(camImg, bHighControllWidth));
		}

		public void Pause()
		{
			webCamTex.Pause();
		}

		public void Stop()
		{
			webCamTex.Stop();
		}

		public bool IsPlaying()
		{
			return webCamTex.isPlaying;
		}

		public Texture GetTexture()
		{
			return webCamTex;
		}

        public string GetDeviceName()
        {
            return webCamTex.deviceName;
        }

        public bool IsDeviceReady()
        {
            return bDeviceReady;
        }

        private IEnumerator WaitCameraDeviceInitial(WebCamTexture webCamTexture)
		{
            // 部分機器在剛開始執行時，webcamTexture會還沒有初始化完成
            // 因此使用這個方式等待
            const int timeoutFrame  = 2000;
            var count               = 0;

            LogSystem.Record("Waiting for WebCamTexture to start");

            yield return new WaitUntil(() => count++ > timeoutFrame || webCamTexture.width > 16);

            if (webCamTexture.width <= 16)
            {
                throw new TimeoutException("Failed to start WebCam");
            }

            LogSystem.Record($"webcamTexture initial complete !!");

            bDeviceReady = true;
		}

		private void FitImageSizeToCamSize(RawImage cameraImg, WebCamTexture webcamTexture, bool bHighControllWidth)
		{
			AspectRatioFitter ratioFitter = cameraImg.gameObject.GetComponent<AspectRatioFitter>();

			if (ratioFitter == null)
			{
				ratioFitter = cameraImg.gameObject.AddComponent<AspectRatioFitter>();
			}

			if (ratioFitter != null)
			{
				AspectRatioFitter.AspectMode mode	= bHighControllWidth ? AspectRatioFitter.AspectMode.HeightControlsWidth : AspectRatioFitter.AspectMode.WidthControlsHeight;
				float aspectRatio					= (float)webcamTexture.width / (float)webcamTexture.height;
				ratioFitter.aspectRatio				= aspectRatio;
				ratioFitter.aspectMode				= mode;
			}
		}

		private void RotationImg(RawImage cameraImg, WebCamTexture webCamTexture)
		{
			float angle			= webCamTexture.videoRotationAngle;
#if UNITY_IOS
			bool bNeedToMirror	= true; // IOS刻意翻轉
#else
			bool bNeedToMirror	= IsFrontFacing(webCamTexture); // 只有前鏡頭需要鏡像
#endif

			if (bNeedToMirror)
			{
				cameraImg.transform.localScale	= new Vector3(-1f, 1f, 1f);
				cameraImg.transform.rotation	*= Quaternion.AngleAxis(angle, Vector3.forward);
			}
			else
			{
				cameraImg.transform.localScale	= new Vector3(1f, 1f, 1f);
				cameraImg.transform.rotation	*= Quaternion.AngleAxis(angle, Vector3.back);
			}
		}

		private bool IsFrontFacing(WebCamTexture webCamTexture)
		{
			foreach(WebCamDevice webCamDevice in WebCamTexture.devices)
			{
				if(webCamTexture.deviceName == webCamDevice.name)
				{
					return webCamDevice.isFrontFacing;
				}
			}

			return false;
		}
	}

	public static class WebCamUtility
	{
		static public WebCamController GenerateCamController(bool bPriorityFrontFacing = false, string sceneName = "")
		{
			WebCamDevice[] deviceList = WebCamTexture.devices;

			LogSystem.Record($"找到 {deviceList.Length} 個鏡頭");

			// 沒有合適的camera
			if (deviceList.Length <= 0)
			{
				return null;
			}

			int camIdx = 0;

			if (bPriorityFrontFacing)
			{
				LogSystem.Record($"優先使用前鏡頭");
			}

			for (int i = 0; i < deviceList.Length; ++i)
			{
				// 優先考慮自拍鏡頭
				if (!(bPriorityFrontFacing ^ deviceList[i].isFrontFacing))
				{
					camIdx = i;
					break;
				}
			}

			//for (int i = deviceList.Length - 1; i >= 0; --i)
			//{
			//	LogSystem.Record($"第 {i + 1} 個鏡頭名稱為 {deviceList[i].name},是否為前鏡頭: {deviceList[i].isFrontFacing}");
			//}

			LogSystem.Record($"使用第 {camIdx + 1} 個鏡頭");

			// 生成 webCamController
			WebCamTexture webCamTex				= new WebCamTexture(deviceList[camIdx].name);
			GameObject controllerGO				= new GameObject("WebCamController");
			WebCamController webCamController	= controllerGO.AddComponent<WebCamController>();
			Scene targetScene					= sceneName != "" ? GetTargetScene(sceneName) : SceneManager.GetSceneAt(0);

			// 將物件搬移到對應的Scene
			SceneManager.MoveGameObjectToScene(controllerGO, targetScene);
			// 初始化Controller
			webCamController.InitialController(webCamTex);

			return webCamController;
		}

		static private Scene GetTargetScene(string sceneName)
		{
			return SceneManager.GetSceneByName(sceneName);
		}
	}
}
