#if USE_OPENCV
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Utility.Beauty
{
    public static class SharpenExtension
    {
        public static Mat UnsharpMask(this Mat inputMat)
        {
            int imageHeight = inputMat.height();
            int imageWidth  = inputMat.width();

            Mat matFinal    = new Mat(imageHeight, imageWidth, CvType.CV_8UC4);
            Imgproc.cvtColor(inputMat, matFinal, Imgproc.COLOR_RGB2RGBA);
            Mat matGaussian = new Mat(imageHeight, imageWidth, CvType.CV_8UC4);
            Imgproc.GaussianBlur(matFinal, matGaussian, new Size(0, 0), 9);
            Core.addWeighted(matFinal, 1.5f, matGaussian, -0.5f, 0, matFinal);
            
            return matFinal;
        }

        public static Mat Sharpen(Mat inputMat)
        {
            int imageHeight = inputMat.height();
            int imageWidth  = inputMat.width();

            Mat filter2DMat = new Mat(imageHeight, imageWidth, CvType.CV_8UC4);
            Imgproc.cvtColor(inputMat, filter2DMat, Imgproc.COLOR_RGB2RGBA);
            Mat kernel      = new Mat(3, 3, CvType.CV_32F, new Scalar(-1));
            kernel.put(0, 0, 0, -1, 0, -1, 5, -1, 0, -1, 0);
            //对图像srcMat和自定义核kernel做卷积，输出到dstMat
            Imgproc.filter2D(filter2DMat, filter2DMat, filter2DMat.depth(), kernel);
            Core.flip(filter2DMat, filter2DMat, 0);

            return filter2DMat;
        }
    }
}
#endif //USE_OPENCV