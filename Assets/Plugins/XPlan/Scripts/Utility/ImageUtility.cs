using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if USE_OPENCV
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
#endif //USE_OPENCV

#if USE_OPENCV_PLUS
using OpenCvSharp;
#endif //USE_OPENCV_PLUS

using UnityEngine;

namespace XPlan.Utility
{
    public struct ImgInfo
    {
        public float averageRGB;
        public float deviationRGB;
    }

#if USE_OPENCV_PLUS
    public class FindFaceParam
    {
        public CascadeClassifier cascade    = null;
        public int minNeighbor              = 5;
        public double scalorFactor          = 1.03;
        public int cameraIdx                = 0;
        public HaarDetectionType type       = HaarDetectionType.ScaleImage;
        public Size minSize                 = new Size(250, 250);
        public Size maxSize                 = new Size(400, 400);
    }
#endif // USE_OPENCV_PLUS

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


#if USE_OPENCV_PLUS
        // 影片介紹
        // https://www.youtube.com/watch?v=lXvt66A0i3Q
        // https://github.com/opencv/opencv/tree/master/data/haarcascades

        static public bool FindFace(out List<OpenCvSharp.Rect> rectList, Texture2D texture2D, FindFaceParam param)
        {
            Mat frame                   = OpenCvSharp.Unity.TextureToMat(texture2D);
            OpenCvSharp.Rect[] faces    = param.cascade.DetectMultiScale(frame
                    , param.scalorFactor
                    , param.minNeighbor
                    , param.type
                    , param.minSize, param.maxSize);

            bool bFind  = faces.Length > 0;
            rectList    = faces.ToList();

            return bFind;
        }

        static public bool FindFace(out List<OpenCvSharp.Rect> rectList, WebCamTexture webCamTexture, FindFaceParam param)
        {
            if (param.cascade == null || !webCamTexture.didUpdateThisFrame)
            {
                rectList = null;
                return false;
            }

            Mat frame                   = OpenCvSharp.Unity.TextureToMat(webCamTexture);
            OpenCvSharp.Rect[] faces    = param.cascade.DetectMultiScale(frame
                    , param.scalorFactor
                    , param.minNeighbor
                    , param.type
                    , param.minSize, param.maxSize);

            bool bFind  = faces.Length > 0;
            rectList    = faces.ToList();

            return bFind;
        }
#endif //USE_OPENCV_PLUS
    }
}
