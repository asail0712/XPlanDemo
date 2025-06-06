﻿using System;
using System.Collections;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;
using UnityEngine;

#if ZXING 
using ZXing;
using ZXing.Common;
#endif // ZXING 

namespace XPlan.Utility
{

#if ZXING
    public static class QRCodeUtility
    {

        public static Texture2D EncodeQRCode(string content, int size = 256)
        {
            BarcodeWriter barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format        = BarcodeFormat.QR_CODE;

            // 設定生成的 QR Code 尺寸
            EncodingOptions encodingOptions = new EncodingOptions();
            encodingOptions.Width           = size;
            encodingOptions.Height          = size;

            barcodeWriter.Options = encodingOptions;

            // 將網址內容轉換為 QR Code
            Color32[] color32 = barcodeWriter.Write(content);

            // 創建一個 Unity Texture2D 並設定像素
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGB24, false);
            texture.SetPixels32(color32);
            texture.Apply();

            return texture;
        }

        public static void DecodeQRCodeFromTexture(Texture2D texture2D, Action<string> finishAction)
        {
            IBarcodeReader barcodeReader = new BarcodeReader
            {
                AutoRotate  = true,
                Options     = new DecodingOptions
                {
                    PossibleFormats = new[] { BarcodeFormat.QR_CODE },
                    TryInverted     = true
                }
            };

            Result result = barcodeReader.Decode(texture2D.GetPixels32(), texture2D.width, texture2D.height);
            
            if (result != null)
            {
                finishAction?.Invoke(result.Text);
            }
            else
			{
                finishAction?.Invoke("");
            }
        }

        public static QRCodeReader DecodeQRCodeFromCamera(WebCamTexture webCamTexture, Action<string> finishAction)
        {
            // 确保有一个 MonoBehaviour 来启动协程
            QRCodeReader reader = new GameObject("QRCodeReader").AddComponent<QRCodeReader>();
            reader.StartCoroutine(reader.ReadQRCodeCoroutine(webCamTexture, finishAction));

            return reader;
        }

    }

    public class QRCodeReader : MonoBehaviour
    {
        private IBarcodeReader barcodeReader;

        void Start()
        {
            // 初始化 ZXING barcode reader
            barcodeReader = new BarcodeReader
            {
                AutoRotate  = true,
                Options     = new DecodingOptions
                {
                    PossibleFormats = new[] { BarcodeFormat.QR_CODE },
                    TryInverted     = true
                }
            };
        }

        public IEnumerator ReadQRCodeCoroutine(WebCamTexture webCamTexture, Action<string> finishAction)
        {
            if(webCamTexture == null)
			{
                finishAction?.Invoke("");
                Destroy(gameObject);
			}

            yield return new WaitUntil(() => webCamTexture.width > 100 && webCamTexture.height > 100);

            bool isQRCodeFound = false;

            while (!isQRCodeFound)
            {
                yield return new WaitForSeconds(1f);

                // 捕获摄像头帧并进行解码
                try
                {
                    Result result = barcodeReader.Decode(webCamTexture.GetPixels32(), webCamTexture.width, webCamTexture.height);
                    if (result != null)
                    {
                        finishAction?.Invoke(result.Text);
                        isQRCodeFound = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex.Message);
                    finishAction?.Invoke("");
                    break;
                }
            }

            Destroy(gameObject);
        }
    }

#endif // ZXING 
}

