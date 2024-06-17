#if USE_OPENCV
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Extension.Beauty
{
    public static class WhilteBalanceExtension
    {

        static private float COLOR_RATIO = 1.2f;

        static public Mat WhiteBalance(this Mat inputMat, float colorRatio = 0.1f, float blueThredhold = 1.0f, float greenThredhold = 1.0f, float redThredhold = 1.0f)
        {
            int row         = inputMat.rows();
            int col         = inputMat.cols();
            int[] histRGB   = new int[767];

            Mat whiteBalanceMat = new Mat(row, col, CvType.CV_8UC4);
            int maxVal          = 0;
            int sampleNum       = 0;

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    double[] oriColor = inputMat.get(i, j);

                    int red     = (int)oriColor[0];
                    int green   = (int)oriColor[1];
                    int blue    = (int)oriColor[2];

                    maxVal = Mathf.Max(maxVal, red);
                    maxVal = Mathf.Max(maxVal, green);
                    maxVal = Mathf.Max(maxVal, blue);
                    histRGB[blue + green + red]++;
                    ++sampleNum;
                }
            }

            int threshold   = 0;
            int sum         = 0;

            double colorThreshold = sampleNum * colorRatio;

            for (int i = 766; i >= 0; i--)
            {
                sum += histRGB[i];
                if (sum > colorThreshold)
                {
                    threshold = i;
                    break;
                }
            }

            double AvgR = 0;
            double AvgG = 0;
            double AvgB = 0;

            int cnt = 0;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    double[] oriColor = inputMat.get(i, j);

                    double red      = oriColor[0];
                    double green    = oriColor[1];
                    double blue     = oriColor[2];

                    double sumP = blue + green + red;

                    if (sumP > threshold)
                    {
                        AvgB += blue;
                        AvgG += green;
                        AvgR += red;
                        cnt++;
                    }
                }
            }

            // Á×§K°£¥H0
            if (cnt == 0)
            {
                return inputMat;
            }

            AvgB /= cnt;
            AvgG /= cnt;
            AvgR /= cnt;

            float blueRatio     = Mathf.Pow((float)((double)maxVal / AvgB), blueThredhold);
            float greenRatio    = Mathf.Pow((float)((double)maxVal / AvgG), greenThredhold);
            float redRatio      = Mathf.Pow((float)((double)maxVal / AvgR), redThredhold);

            float smallestRatio = Mathf.Min(blueRatio, greenRatio, redRatio);

            blueRatio   = Mathf.Clamp(blueRatio, smallestRatio, smallestRatio * COLOR_RATIO);
            greenRatio  = Mathf.Clamp(greenRatio, smallestRatio, smallestRatio * COLOR_RATIO);
            redRatio    = Mathf.Clamp(redRatio, smallestRatio, smallestRatio * COLOR_RATIO);

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    double[] oriColor = inputMat.get(i, j);

                    double Blue     = oriColor[2] * blueRatio;
                    double Green    = oriColor[1] * greenRatio;
                    double Red      = oriColor[0] * redRatio;

                    Red     = Mathf.Clamp((int)Red, 0, 255);
                    Green   = Mathf.Clamp((int)Green, 0, 255);
                    Blue    = Mathf.Clamp((int)Blue, 0, 255);

                    double[] color = new double[4];
                    color[2] = Blue;
                    color[1] = Green;
                    color[0] = Red;
                    color[3] = inputMat.get(i, j)[3];

                    whiteBalanceMat.put(i, j, color);
                }
            }

            return whiteBalanceMat;
        }
    }
}
#endif //USE_OPENCV