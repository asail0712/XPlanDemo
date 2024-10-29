#if USE_OPENCV
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Utility;

// 參考資料
// https://blog.csdn.net/qq_39097425/article/details/110823333
// https://cloud.tencent.com/developer/article/1552619
// YUV亮度處理
// https://answers.opencv.org/question/117045/how-to-remove-bad-lighting-conditions-or-shadow-effects-in-images-using-opencv-for-processing-face-images/
// http://www.juzicode.com/opencv-python-equalizehist-createclahe/
// OpenCV範例
// https://www.cnblogs.com/wydxry/p/10902520.html
// 使用映射表美白
// https://walkonnet.com/archives/204309

namespace XPlan.Utility.Beauty
{
    public static class BrightnessExtension
    {
        static int[] ColorMapping = {
        1, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 31, 33, 35, 37, 39,
        41, 43, 44, 46, 48, 50, 52, 53, 55, 57, 59, 60, 62, 64, 66, 67, 69, 71, 73, 74,
        76, 78, 79, 81, 83, 84, 86, 87, 89, 91, 92, 94, 95, 97, 99, 100, 102, 103, 105,
        106, 108, 109, 111, 112, 114, 115, 117, 118, 120, 121, 123, 124, 126, 127, 128,
        130, 131, 133, 134, 135, 137, 138, 139, 141, 142, 143, 145, 146, 147, 149, 150,
        151, 153, 154, 155, 156, 158, 159, 160, 161, 162, 164, 165, 166, 167, 168, 170,
        171, 172, 173, 174, 175, 176, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187,
        188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203,
        204, 205, 205, 206, 207, 208, 209, 210, 211, 211, 212, 213, 214, 215, 215, 216,
        217, 218, 219, 219, 220, 221, 222, 222, 223, 224, 224, 225, 226, 226, 227, 228,
        228, 229, 230, 230, 231, 232, 232, 233, 233, 234, 235, 235, 236, 236, 237, 237,
        238, 238, 239, 239, 240, 240, 241, 241, 242, 242, 243, 243, 244, 244, 244, 245,
        245, 246, 246, 246, 247, 247, 248, 248, 248, 249, 249, 249, 250, 250, 250, 250,
        251, 251, 251, 251, 252, 252, 252, 252, 253, 253, 253, 253, 253, 254, 254, 254,
        254, 254, 254, 254, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 256};

        static public Mat CommonBrightness(this Mat inputMat)
        {
            //调亮
            Mat brightnessMat = new Mat(inputMat.height(), inputMat.width(), CvType.CV_8UC4);
            brightnessMat = inputMat.clone();
            inputMat.convertTo(brightnessMat, -1, 1.1f, 30);

            return brightnessMat;
        }


        static public Mat BrightnessWithEqualizeHist(this Mat inputMat)
        {
            int imageHeight = inputMat.height();
            int imageWidth  = inputMat.width();

            // BGRA轉BGR
            Mat inputMat_CV_8UC3 = new Mat(imageHeight, imageWidth, CvType.CV_8UC3);
            Imgproc.cvtColor(inputMat, inputMat_CV_8UC3, Imgproc.COLOR_BGRA2BGR);

            // BGR轉YUV
            Mat outputMat = new Mat(imageHeight, imageWidth, CvType.CV_8UC3);
            Imgproc.cvtColor(inputMat, outputMat, Imgproc.COLOR_BGR2YUV);

            // 亮度
            List<Mat> channels = new List<Mat>();
            Core.split(outputMat, channels);
            Imgproc.equalizeHist(channels[0], channels[0]);
            Core.merge(channels, outputMat);

            // YUV轉BGR
            Imgproc.cvtColor(outputMat, outputMat, Imgproc.COLOR_YUV2BGR);

            // BGR轉BGRA
            Mat outputMat_CV_8UC4 = new Mat(imageHeight, imageWidth, CvType.CV_8UC4);
            Imgproc.cvtColor(outputMat, outputMat_CV_8UC4, Imgproc.COLOR_BGR2BGRA);


            Texture2D demotexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGBA32, false);
            Utils.matToTexture2D(outputMat_CV_8UC4, demotexture, false);
            ImgInfo newImgInfo;
            ImageUtility.GetImgInfo(out newImgInfo, demotexture);

            return outputMat_CV_8UC4;
        }

        static public Mat BrightnessWithCLAHE(this Mat inputMat, Size tileGridSize, double clipLimit = 1)
        {
            int imageHeight = inputMat.height();
            int imageWidth = inputMat.width();

            // BGRA轉BGR
            Mat inputMat8UC3 = new Mat(imageHeight, imageWidth, CvType.CV_8UC3);
            Imgproc.cvtColor(inputMat, inputMat8UC3, Imgproc.COLOR_BGRA2BGR);

            // BGR轉YUV
            Imgproc.cvtColor(inputMat8UC3, inputMat8UC3, Imgproc.COLOR_BGR2YUV);

            // 亮度
            Mat outputMat8UC3 = new Mat(imageHeight, imageWidth, CvType.CV_8UC3);
            CLAHE clahe = Imgproc.createCLAHE(clipLimit, tileGridSize);
            List<Mat> channels = new List<Mat>();

            Core.split(inputMat8UC3, channels);
            clahe.apply(channels[0], channels[0]);
            Core.merge(channels, outputMat8UC3);

            // YUV轉BGR
            Imgproc.cvtColor(outputMat8UC3, outputMat8UC3, Imgproc.COLOR_YUV2BGR);

            // BGR轉BGRA
            Mat outputMat8UC4 = new Mat(imageHeight, imageWidth, CvType.CV_8UC4);
            Imgproc.cvtColor(outputMat8UC3, outputMat8UC4, Imgproc.COLOR_BGR2BGRA);


            return outputMat8UC4;
        }

        static public Mat BrightnessMapping(this Mat inputMat, float alpha = 1.0f, float beta = 0.0f)
        {
            int imageHeight = inputMat.height();
            int imageWidth = inputMat.width();

            Mat outputMat = new Mat(imageHeight, imageWidth, CvType.CV_8UC4);

            int width = inputMat.cols();
            int height = inputMat.rows();

            // 遍历图像的每个像素
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double[] pixelColor = inputMat.get(y, x);
                    double blue = pixelColor[0];
                    double green = pixelColor[1];
                    double red = pixelColor[2];

                    double[] newPixelColor =
                    {
                    Mathf.FloorToInt(ColorMapping[(int)blue] * alpha + beta),
                    Mathf.FloorToInt(ColorMapping[(int)green] * alpha + beta),
                    Mathf.FloorToInt(ColorMapping[(int)red] * alpha + beta),
                    pixelColor[3]
                };

                    outputMat.put(y, x, newPixelColor);
                }
            }


            return outputMat;
        }


        static public Mat BrightnessWithSimpleHDR(this Mat inputMat, float highestRGB = 200f, float lowestRGB = 80f, float deviatioRatio = 56f, float averageRatio = 1.1f)
        {
            int imageHeight = inputMat.height();
            int imageWidth  = inputMat.width();

            Texture2D imgTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGBA32, false);
            Utils.matToTexture2D(inputMat, imgTexture, false);

            // 原圖資訊
            ImgInfo imgInfo;
            ImageUtility.GetImgInfo(out imgInfo, imgTexture);

            float ratioF = imgInfo.deviationRGB > deviatioRatio ? 1f : Mathf.Min(1.2f, (56f / imgInfo.deviationRGB));
            float baseF = Mathf.Clamp(imgInfo.averageRGB * averageRatio, lowestRGB, highestRGB) - imgInfo.averageRGB * ratioF;

            //调亮 & HDR
            Mat brightnessMat = new Mat(imageHeight, imageWidth, CvType.CV_8UC4);
            brightnessMat = inputMat.clone();
            inputMat.convertTo(brightnessMat, -1, ratioF, baseF);


            Texture2D demotexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGBA32, false);
            Utils.matToTexture2D(brightnessMat, demotexture, false);
            ImgInfo newImgInfo2;
            ImageUtility.GetImgInfo(out newImgInfo2, demotexture);

            return brightnessMat;
        }
    }
}
#endif //USE_OPENCV