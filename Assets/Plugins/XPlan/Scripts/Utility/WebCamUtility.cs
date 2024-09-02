using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using XPlan.DebugMode;

namespace XPlan.Utility
{
	public class WebCamController : MonoBehaviour
	{
		private WebCamTexture webCamTex;

		public void InitialController(WebCamTexture webCamTex, RawImage camImg, bool bHighControllWidth = true)
		{
			this.webCamTex	= webCamTex;

			if (camImg.texture != null)
			{
				camImg.enabled = false;

				((WebCamTexture)camImg.texture).Stop();
				GameObject.Destroy(camImg.texture);
				camImg.texture = null;
			}

			camImg.texture = webCamTex;
			camImg.enabled = true;

			webCamTex.Play();

			MonoBehaviourHelper.StartCoroutine(WaitCameraDeviceInitial(camImg, webCamTex, bHighControllWidth));
		}

		private void OnDestroy()
		{
			if (webCamTex == null)
			{
				return;
			}

			webCamTex.Stop();
			GameObject.Destroy(webCamTex);
		}

		public void Play()
		{
			webCamTex.Play();
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

		private IEnumerator WaitCameraDeviceInitial(RawImage cameraImg, WebCamTexture webcamTexture, bool bHighControllWidth)
		{
			// 部分機器在剛開始執行時，webcamTexture會還沒有初始化完成
			// 因此使用這個方式等待
			if (webcamTexture.width <= 16)
			{
				LogSystem.Record($"webcamTexture need to initial !!");

				while (!webcamTexture.didUpdateThisFrame)
				{
					yield return new WaitForEndOfFrame();
				}

				LogSystem.Record($"webcamTexture Update Frame !!");
			}

			LogSystem.Record($"webcamTexture initial complete !!");

			// 先調整Img大小
			FitImageSizeToCamSize(cameraImg, webcamTexture, bHighControllWidth);

			// 翻轉處理
			RotationImg(cameraImg, webcamTexture);
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
			bool bNeedToMirror	= false;

			// IOS與Android要鏡像翻轉的情形不同
#if UNITY_IOS
			bNeedToMirror = webCamTexture.deviceName == WebCamTexture.devices[0].name;
#else
			bNeedToMirror = webCamTexture.deviceName != WebCamTexture.devices[0].name;
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
	}

	public static class WebCamUtility
	{
		static public WebCamController GenerateCamController(RawImage rawImg, bool bPriorityFrontFacing = false, string SceneName = "")
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

			for (int i = deviceList.Length - 1; i >= 0; --i)
			{
				// 優先考慮自拍鏡頭
				if (!(bPriorityFrontFacing ^ deviceList[i].isFrontFacing))
				{
					camIdx = i;
					break;
				}
			}

			for (int i = deviceList.Length - 1; i >= 0; --i)
			{
				LogSystem.Record($"第 {i + 1} 個鏡頭名稱為 {deviceList[i].name},是否為前鏡頭: {deviceList[i].isFrontFacing}");
			}

			LogSystem.Record($"使用第 {camIdx + 1} 個鏡頭");

			// 生成 webCamController
			WebCamTexture webCamTex				= new WebCamTexture(deviceList[camIdx].name);
			GameObject controllerGO				= new GameObject("WebCamController");
			WebCamController webCamController	= controllerGO.AddComponent<WebCamController>();
			Scene targetScene					= SceneName != "" ? GetTargetScene(SceneName) : rawImg.gameObject.scene;

			// 將物件搬移到對應的Scene
			SceneManager.MoveGameObjectToScene(controllerGO, targetScene);
			// 初始化Controller
			webCamController.InitialController(webCamTex, rawImg);

			return webCamController;
		}

		static private Scene GetTargetScene(string sceneName)
		{
			return SceneManager.GetSceneByName(sceneName);
		}


//		static public WebCamTexture FindWebCamTexture(bool bPriorityFrontFacing = false)
//		{
//			WebCamDevice[] deviceList = WebCamTexture.devices;
			
//			LogSystem.Record($"找到 {deviceList.Length} 個鏡頭");

//			// 沒有合適的camera
//			if (deviceList.Length <= 0)
//			{
//				return null;
//			}

//			int camIdx = 0;

//			if (bPriorityFrontFacing)
//			{
//				LogSystem.Record($"優先使用前鏡頭");
//			}

//			for (int i = deviceList.Length - 1; i >= 0; --i)
//			{
//				// 優先考慮自拍鏡頭
//				if (!(bPriorityFrontFacing ^ deviceList[i].isFrontFacing))
//				{
//					camIdx = i;
//					break;
//				}
//			}

//			for (int i = deviceList.Length - 1; i >= 0; --i)
//			{
//				LogSystem.Record($"第 {i + 1} 個鏡頭名稱為 {deviceList[i].name},是否為前鏡頭: {deviceList[i].isFrontFacing}");
//			}

//			LogSystem.Record($"使用第 {camIdx + 1} 個鏡頭");

//			return new WebCamTexture(deviceList[camIdx].name);
//		}

//		static public void InitialCameraImg(RawImage camImg, WebCamTexture webcamTexture, bool bHighControllWidth = true)
//		{
//			if (camImg.texture != null)
//			{
//				camImg.enabled = false;

//				((WebCamTexture)camImg.texture).Stop();
//				GameObject.Destroy(camImg.texture);
//				camImg.texture = null;
//			}

//			camImg.texture = webcamTexture;
//			camImg.enabled = true;

//			webcamTexture.Play();

//			MonoBehaviourHelper.StartCoroutine(WaitCameraDeviceInitial(camImg, webcamTexture, bHighControllWidth));
//		}

//		static public void ReleaseCameraImg(RawImage camImg)
//		{
//			if (camImg.texture == null)
//			{
//				return;
//			}

//			WebCamTexture webCamTexture = (WebCamTexture)camImg.texture;

//			if(webCamTexture == null)
//			{
//				return;
//			}

//			webCamTexture.Stop();
//			GameObject.Destroy(webCamTexture);

//			camImg.texture = null;
//		}

//		static private IEnumerator WaitCameraDeviceInitial(RawImage cameraImg, WebCamTexture webcamTexture, bool bHighControllWidth)
//		{
//			// 部分機器在剛開始執行時，webcamTexture會還沒有初始化完成
//			// 因此使用這個方式等待
//			if (webcamTexture.width <= 16)
//			{
//				LogSystem.Record($"webcamTexture need to initial !!");

//				while (!webcamTexture.didUpdateThisFrame)
//				{
//					yield return new WaitForEndOfFrame();
//				}

//				LogSystem.Record($"webcamTexture Update Frame !!");
//			}

//			LogSystem.Record($"webcamTexture initial complete !!");

//			// 先調整Img大小
//			FitImageSizeToCamSize(cameraImg, webcamTexture, bHighControllWidth);

//			// 翻轉處理
//			RotationImg(cameraImg, webcamTexture);
//		}

//		static private void FitImageSizeToCamSize(RawImage cameraImg, WebCamTexture webcamTexture, bool bHighControllWidth)
//		{
//			AspectRatioFitter ratioFitter = cameraImg.gameObject.GetComponent<AspectRatioFitter>();

//			if(ratioFitter == null)
//			{
//				ratioFitter = cameraImg.gameObject.AddComponent<AspectRatioFitter>();
//			}

//			if (ratioFitter != null)
//			{
//				AspectRatioFitter.AspectMode mode	= bHighControllWidth ? AspectRatioFitter.AspectMode.HeightControlsWidth : AspectRatioFitter.AspectMode.WidthControlsHeight;
//				float aspectRatio					= (float)webcamTexture.width / (float)webcamTexture.height;
//				ratioFitter.aspectRatio				= aspectRatio;
//				ratioFitter.aspectMode				= mode;
//			}
//		}

//		static private void RotationImg(RawImage cameraImg, WebCamTexture webCamTexture)
//		{
//			float angle			= webCamTexture.videoRotationAngle;
//			bool bNeedToMirror	= false;

//			// IOS與Android要鏡像翻轉的情形不同
//#if UNITY_IOS
//			bNeedToMirror = webCamTexture.deviceName == WebCamTexture.devices[0].name;
//#else
//			bNeedToMirror = webCamTexture.deviceName != WebCamTexture.devices[0].name;
//#endif

//			if (bNeedToMirror)
//			{
//				cameraImg.transform.localScale	= new Vector3(-1f, 1f, 1f);
//				cameraImg.transform.rotation	*= Quaternion.AngleAxis(angle, Vector3.forward);
//			}
//			else
//			{
//				cameraImg.transform.localScale	= new Vector3(1f, 1f, 1f);
//				cameraImg.transform.rotation	*= Quaternion.AngleAxis(angle, Vector3.back);
//			}
//		}
	}
}
