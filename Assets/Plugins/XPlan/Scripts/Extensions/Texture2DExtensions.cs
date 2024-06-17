using System;
using System.Net.Mail;
using System.Text.RegularExpressions;

using UnityEngine;

namespace XPlan.Extensions
{
    public static class Texture2DExtensions
    {
		static public byte[] ToByteArray(this Texture2D texture, bool bIsJpeg = false)
		{
			Texture2D sourceTexReadable = null;
			RenderTexture rt			= RenderTexture.GetTemporary(texture.width, texture.height);
			RenderTexture activeRT		= RenderTexture.active;

			Graphics.Blit(texture, rt);
			RenderTexture.active		= rt;

			sourceTexReadable			= new Texture2D(texture.width, texture.height, bIsJpeg ? TextureFormat.RGB24 : TextureFormat.RGBA32, false);
			sourceTexReadable.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0, false);
			sourceTexReadable.Apply(false, false);

			RenderTexture.active		= activeRT;
			RenderTexture.ReleaseTemporary(rt);

			byte[] photoByte = null;

			if (bIsJpeg)
			{
				photoByte = sourceTexReadable.EncodeToJPG(100);
			}
			else
			{
				photoByte = sourceTexReadable.EncodeToPNG();
			}

			return photoByte;
		}

		static public string TexToBase64(this Texture2D texture)
		{
			byte[] jpgByte		= texture.EncodeToJPG();
			string base64Str	= Convert.ToBase64String(jpgByte);

			return base64Str;
		}


		static public Texture2D ResizeTexture(this Texture2D originalTex, int width, int height)
		{
			// 檢查原始紋理是否存在
			if (originalTex == null)
			{
				Debug.LogError("Original texture is missing.");
				return null;
			}

			// 創建一個新的Texture2D用於存儲調整大小後的圖片
			Texture2D resizedTexture	= new Texture2D(width, height, originalTex.format, originalTex.mipmapCount > 0);

			// 計算縮放因子
			float widthFactor			= (float)originalTex.width / width;
			float heightFactor			= (float)originalTex.height / height;

			// 遍歷新尺寸的每個像素
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					// 計算原始紋理中對應的坐標
					int originalX		= Mathf.FloorToInt(x * widthFactor);
					int originalY		= Mathf.FloorToInt(y * heightFactor);

					// 獲取原始紋理中的像素顏色
					Color pixelColor	= originalTex.GetPixel(originalX, originalY);

					// 設置新尺寸紋理中的像素顏色
					resizedTexture.SetPixel(x, y, pixelColor);
				}
			}

			// 应用修改
			resizedTexture.Apply();

			// 將調整大小後的紋理應用到Material上，或者進行其他操作
			// renderer.material.mainTexture = resizedTexture;

			return resizedTexture;
		}
	}
}

