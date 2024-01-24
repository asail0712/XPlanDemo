#if USE_OPENCV
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
#endif //USE_OPENCV
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Utility
{
    public struct ImgInfo
    {
        public float averageRGB;
        public float deviationRGB;
    }

    public static class ImageUtility
    {
#if USE_OPENCV
        static public void GetImgInfo(out ImgInfo imgInfo, Mat inputMat)
        {
            Texture2D imgTexture = new Texture2D(inputMat.cols(), inputMat.rows(), TextureFormat.RGBA32, false);

            Utils.fastMatToTexture2D(inputMat, imgTexture, false);

            Color32[] imgList = imgTexture.GetPixels32();

            float totalSum  = 0;
            int totalNum    = 0;

            foreach (Color32 img in imgList)
            {
                float pixelSum = img.r + img.g + img.b;

                totalSum += pixelSum;
                ++totalNum;
            }

            imgInfo.averageRGB  = totalSum / (float)(3 * totalNum);
            float tmpSum        = 0;

            foreach (Color32 img in imgList)
            {
                float pixelSum  = img.r + img.g + img.b;
                float pixelAvag = pixelSum / 3;
                tmpSum += Mathf.Pow(pixelAvag - imgInfo.averageRGB, 2);
            }

            imgInfo.deviationRGB = Mathf.Sqrt(tmpSum / totalNum);
        }
#endif //USE_OPENCV

        static public void GetImgInfo(out ImgInfo imgInfo, Texture2D imgTexture)
        {
            Color32[] imgList = imgTexture.GetPixels32();

            float totalSum  = 0;
            int totalNum    = 0;

            foreach (Color32 img in imgList)
            {
                float pixelSum = img.r + img.g + img.b;

                totalSum += pixelSum;
                ++totalNum;
            }

            imgInfo.averageRGB  = totalSum / (float)(3 * totalNum);
            float tmpSum        = 0;

            foreach (Color32 img in imgList)
            {
                float pixelSum  = img.r + img.g + img.b;
                float pixelAvag = pixelSum / 3;
                tmpSum += Mathf.Pow(pixelAvag - imgInfo.averageRGB, 2);
            }

            imgInfo.deviationRGB = Mathf.Sqrt(tmpSum / totalNum);
        }
    }
}
