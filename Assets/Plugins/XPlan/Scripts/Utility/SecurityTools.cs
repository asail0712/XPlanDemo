// ==============================================================================
// XPlan Framework
//
// Copyright (c) 2026 Asail
// All rights reserved.
//
// Author  : Asail0712
// Project : XPlan
// Description:
//     A modular framework for Unity projects, focusing on MVVM architecture,
//     runtime tooling, event-driven design, and extensibility.
//
// Contact : asail0712@gmail.com
// GitHub  : https://github.com/asail0712/XPlanDemo
//
// Unauthorized copying, modification, or distribution of this file,
// via any medium, is strictly prohibited without prior permission.
// ==============================================================================
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace XPlan.Utility
{
    public static class SecurityTools
    {
		public static string ComputeSha256Hash(this string rawData, string salt)
		{
			// 使用 SHA256 加密算法
			using (SHA256 sha256Hash = SHA256.Create())
			{
				// 將 rawData 與 salt 合併，然後進行哈希計算
				string saltedData		= rawData + salt;
				// 計算哈希值 - 返回 byte 數組
				byte[] bytes			= sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(saltedData));
				// 將 byte 數組轉換成字符串
				StringBuilder builder	= new StringBuilder();

				for (int i = 0; i < bytes.Length; i++)
				{
					builder.Append(bytes[i].ToString("x2")); // 將 byte 轉換為 16 進制字符串
				}

				return builder.ToString();
			}
		}
	}
}