#if USE_OPENCV
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Extension.Beauty
{
	public static class BlurExtension
	{

		public static Mat GaussianBlur(this Mat inputMat)
		{
			int imageHeight = inputMat.height();
			int imageWidth	= inputMat.width();

			Mat GaussianBlurMat = new Mat(imageHeight, imageWidth, CvType.CV_8UC4);
			GaussianBlurMat		= inputMat.clone();

			Size size = new Size(9, 9);
			Imgproc.GaussianBlur(GaussianBlurMat, GaussianBlurMat, size, 0, 0); // 高斯模糊，消除椒盐噪声
			Core.flip(GaussianBlurMat, GaussianBlurMat, 0);

			return GaussianBlurMat;
		}

		public static Mat BilateralFilter(this Mat inputMat, int param1 = 25, double param2 = 50)
		{
			int imageHeight = inputMat.height();
			int imageWidth	= inputMat.width();

			Mat BGRMat = new Mat(imageHeight, imageWidth, CvType.CV_8UC3);
			Mat dstMat = new Mat(imageHeight, imageWidth, CvType.CV_8UC3);
			Mat bilateralFilterMat = new Mat(imageHeight, imageWidth, CvType.CV_8UC4);

			Imgproc.cvtColor(inputMat, BGRMat, Imgproc.COLOR_RGBA2BGR);
			Imgproc.bilateralFilter(BGRMat, dstMat, param1, param2, 75);
			Imgproc.cvtColor(dstMat, bilateralFilterMat, Imgproc.COLOR_BGR2RGBA);
			
			return bilateralFilterMat;
		}
	}
}
#endif //USE_OPENCV