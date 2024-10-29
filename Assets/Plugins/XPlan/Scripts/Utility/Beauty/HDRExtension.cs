#if USE_OPENCV
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.PhotoModule;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Utility;

// 資料來源
// https://cloud.tencent.com/developer/article/2024958
// https://blog.51cto.com/stq054188/4537286
// https://linux.cn/article-9754-1.html

namespace XPlan.Utility.Beauty
{
	public static class HDRExtension
	{
		public static Mat GenerateHDR(this Mat inputMat, ref List<Mat> exposureList, Mat responseDebevec,  int numOfExposure = 3)
		{
			if(exposureList == null || responseDebevec == null)
			{
				return null;
			}

			int imageHeight			= inputMat.height();
			int imageWidth			= inputMat.width();
			Mat HDRMat				= new Mat(imageHeight, imageWidth, CvType.CV_8UC4);
			Mat times				= new Mat(1, numOfExposure, CvType.CV_32F);
			Texture2D imgTexture	= new Texture2D(imageWidth, imageHeight, TextureFormat.RGBA32, false);

			Utils.matToTexture2D(inputMat, imgTexture, false);

			ImgInfo imgInfo;
			ImageUtility.GetImgInfo(out imgInfo, imgTexture);

			int level	= Mathf.CeilToInt((float)(imgInfo.averageRGB / (256 / (numOfExposure + 2))));
			float time	= 0.04f;

			for (int i = 1; i < numOfExposure + 1; ++i)
			{
				double ratio = (double)i / (double)level;

				Mat exposureMat = new Mat (imageHeight, imageWidth, CvType.CV_8UC3);
				inputMat.convertTo(exposureMat, CvType.CV_8UC3, ratio, 0);

				times.put(0, i, time); // 设置曝光时间，这里使用1.0作为示例值

				exposureList.Add(exposureMat);

				time *= 1.5f;
			}

			// 查找CRF
			Photo.createCalibrateDebevec().process(exposureList, responseDebevec, times);

			// 合併為HDR
			Photo.createMergeDebevec().process(exposureList, HDRMat, times, responseDebevec);

			//进行色调映射
			Mat LDRMat = new Mat(imageHeight, imageWidth, CvType.CV_8UC4);
			Photo.createTonemap(2.2f).process(HDRMat, LDRMat);

			//最后合成处理
			Mat fusionMat = new Mat(imageHeight, imageWidth, CvType.CV_8UC4);
			Photo.createMergeMertens().process(exposureList, fusionMat, times, responseDebevec);

			return HDRMat;
		}
	}
}
#endif //USE_OPENCV