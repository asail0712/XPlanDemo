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
	}
}

