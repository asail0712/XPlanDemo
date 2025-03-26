using System;
using System.Net.Mail;
using System.Text.RegularExpressions;

using UnityEngine;

namespace XPlan.Utility
{
    public static class TextureExtensions
    {
		static public Texture2D ToTexture2D(this Texture texture, TextureFormat textureFormat = TextureFormat.RGB24, Material mat = null)
		{
			RenderTexture renderTexture = new RenderTexture(texture.width, texture.height, 0);

			if (mat != null)
			{
				Graphics.Blit(texture, renderTexture, mat);
			}
			else
			{
				Graphics.Blit(texture, renderTexture);
			}

			// 設定 RenderTexture.active
			RenderTexture.active	= renderTexture;
			// 創建一個新的 Texture2D 與 RenderTexture 相同的尺寸和格式
			Texture2D texture2D		= new Texture2D(renderTexture.width, renderTexture.height, textureFormat, false);

			// 從 RenderTexture 中讀取像素到 Texture2D
			texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
			texture2D.Apply();

			// 清理
			RenderTexture.active = null;
			renderTexture.Release();

			return texture2D;
		}

		static public Texture2D ToTexture2D(this RenderTexture renderTexture, TextureFormat textureFormat = TextureFormat.RGB24, Material mat = null)
		{
			// 設定 RenderTexture.active
			RenderTexture.active		= renderTexture;
			RenderTexture tmpTexture	= new RenderTexture(renderTexture.width, renderTexture.height, renderTexture.depth);

			if (mat != null)
			{ 
				Graphics.Blit(renderTexture, tmpTexture, mat);
			}

			// 創建一個新的 Texture2D 與 RenderTexture 相同的尺寸和格式
			Texture2D texture2D			= new Texture2D(tmpTexture.width, tmpTexture.height, textureFormat, false);

			// 從 RenderTexture 中讀取像素到 Texture2D
			texture2D.ReadPixels(new Rect(0, 0, tmpTexture.width, tmpTexture.height), 0, 0);
			texture2D.Apply();

			// 清理
			RenderTexture.active = null;

			return texture2D;
		}

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


		static public Texture2D ScaleTexture(this Texture2D originalTex, float scaleRatio)
        {
			int targetWidth		= (int)((float)originalTex.width * scaleRatio);
			int targetHeight	= (int)((float)originalTex.height * scaleRatio);
			RenderTexture rt	= RenderTexture.GetTemporary(targetWidth, targetHeight);

			Graphics.Blit(originalTex, rt);

			RenderTexture.active		= rt;
			Texture2D resizedTexture	= new Texture2D(targetWidth, targetHeight, originalTex.format, false);

			resizedTexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
			resizedTexture.Apply();

			RenderTexture.active		= null;
			RenderTexture.ReleaseTemporary(rt);

			return resizedTexture;
		}

		static public Texture2D ExtractAreaFromTexture(this Texture2D sourceTexture, Vector2 topLeftCorner, int width, int height, TextureFormat textureFormat = TextureFormat.RGBA32)
		{
			int startX				= Mathf.Clamp((int)topLeftCorner.x, 0, sourceTexture.width - 1);

			// Y 軸從左上角轉換到左下角的對應位置
			int startY				= Mathf.Clamp(sourceTexture.height - (int)topLeftCorner.y - height, 0, sourceTexture.height - 1);

			// 確保擷取的寬度和高度不超過源 Texture 邊界
			int actualWidth			= Mathf.Clamp(width, 0, sourceTexture.width - startX);
			int actualHeight		= Mathf.Clamp(height, 0, sourceTexture.height - startY);

			// 從 sourceTexture 取得指定區域的像素數據
			Color[] pixels			= sourceTexture.GetPixels(startX, startY, actualWidth, actualHeight);
			// 創建一個新的 Texture2D 來存放擷取的像素
			Texture2D newTexture	= new Texture2D(actualWidth, actualHeight, textureFormat, false);

			newTexture.SetPixels(pixels);
			newTexture.Apply();  // 應用更改

			return newTexture;
		}

		static public Texture2D ExtractAreaFromTexture(this RenderTexture sourceTexture, Vector2 topLeftCorner, int width, int height, TextureFormat textureFormat = TextureFormat.RGBA32)
		{
			if (sourceTexture == null)
			{
				return null;
			}

			// 將當前的 RenderTexture 設置為活動的
			RenderTexture.active		= sourceTexture;
			// 創建一個新的 Texture2D，大小與 Rect 一致
			Texture2D extractedTexture	= new Texture2D((int)sourceTexture.width, (int)sourceTexture.height, textureFormat, false);

			// 從 RenderTexture 中讀取指定 Rect 的像素
			extractedTexture.ReadPixels(new Rect(topLeftCorner.x, topLeftCorner.y, width, height), 0, 0);
			extractedTexture.Apply();

			// 完成後重置 active RenderTexture
			RenderTexture.active = null;

			// 你現在可以將 extractedTexture 用於其他地方，如顯示在 UI 上
			return extractedTexture;
		}

		static public Texture2D Clone(this Texture2D original)
		{
			// 創建一個新的 Texture2D，與原始的 Texture2D 尺寸和格式相同
			Texture2D newTexture = new Texture2D(original.width, original.height, original.format, false);

			// 複製原始的數據
			newTexture.LoadRawTextureData(original.GetRawTextureData());
			newTexture.Apply();  // 應用更改

			return newTexture;
		}

		static public Sprite CreateSprite(this Texture2D texture, Vector2 pivot = default)
		{
			if (texture == null) return null;

			if (pivot == default)
			{
				pivot = new Vector2(0.5f, 0.5f);
			}

			return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot);
		}
	}
}

