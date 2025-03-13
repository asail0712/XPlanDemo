using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XPlan.Utility
{ 
	public static class LocalPref
	{
		public static bool HasKey(string key)
		{
#if UNITY_EDITOR
			return EditorPrefs.HasKey(key);
#else
			return PlayerPrefs.HasKey(key);
#endif //UNITY_EDITOR
		}

		public static void DeleteKey(string key)
		{
#if UNITY_EDITOR
			EditorPrefs.DeleteKey(key);
#else
			PlayerPrefs.DeleteKey(key);
#endif //UNITY_EDITOR
		}

		public static void ClearAll()
		{
#if UNITY_EDITOR
			EditorPrefs.DeleteAll();
#else
			PlayerPrefs.DeleteAll();
#endif //UNITY_EDITOR
		}

		public static void SetValue<T>(string key, T value)
		{
	#if UNITY_EDITOR
			if (typeof(T) == typeof(int))
			{
				EditorPrefs.SetInt(key, Convert.ToInt32(value));
			}
			else if (typeof(T) == typeof(bool))
			{
				EditorPrefs.SetBool(key, Convert.ToBoolean(value));
			}
			else if (typeof(T) == typeof(float))
			{
				EditorPrefs.SetFloat(key, Convert.ToSingle(value));
			}
			else if (typeof(T) == typeof(string))
			{
				EditorPrefs.SetString(key, Convert.ToString(value));
			}
			else if (typeof(T) == typeof(Vector3))
			{
				string vecStr	= value.ToString();
				vecStr			= vecStr.Substring(1, vecStr.Length - 2);

				EditorPrefs.SetString(key, vecStr);
			}
#else
			if (typeof(T) == typeof(int))
			{
				PlayerPrefs.SetInt(key, Convert.ToInt32(value));
			}
			else if (typeof(T) == typeof(float))
			{
				PlayerPrefs.SetFloat(key, Convert.ToSingle(value));
			}
			else if (typeof(T) == typeof(string))
			{
				PlayerPrefs.SetString(key, Convert.ToString(value));
			}
			else if (typeof(T) == typeof(bool))
			{
				PlayerPrefs.SetInt(key, Convert.ToBoolean(value) ? 1 : 0);
			}
			else if (typeof(T) == typeof(Vector3))
			{
				string vecStr	= value.ToString();
				vecStr			= vecStr.Substring(1, vecStr.Length - 2);

				PlayerPrefs.SetString(key, vecStr);
			}
#endif //UNITY_EDITOR
		}

		public static T GetValue<T>(string key, T defaultValue = default(T))
		{
	#if UNITY_EDITOR
			if(!EditorPrefs.HasKey(key))
			{
				return defaultValue;
			}

			if (typeof(T) == typeof(int))
			{
				return (T)Convert.ChangeType(EditorPrefs.GetInt(key), typeof(T));
			}
			else if (typeof(T) == typeof(bool))
			{
				return (T)Convert.ChangeType(EditorPrefs.GetBool(key), typeof(T));
			}
			else if (typeof(T) == typeof(float))
			{
				return (T)Convert.ChangeType(EditorPrefs.GetFloat(key), typeof(T));
			}
			else if (typeof(T) == typeof(string))
			{
				return (T)Convert.ChangeType(EditorPrefs.GetString(key), typeof(T));
			}
			else if (typeof(T) == typeof(Vector3))
			{
				string[] vecStrList = EditorPrefs.GetString(key).Split(",");

				return (T)Convert.ChangeType(new Vector3(float.Parse(vecStrList[0]), float.Parse(vecStrList[1]), float.Parse(vecStrList[2])), typeof(T));
			}
#else
			if (!PlayerPrefs.HasKey(key))
			{
				return defaultValue;
			}

			if (typeof(T) == typeof(int))
			{
				return (T)Convert.ChangeType(PlayerPrefs.GetInt(key), typeof(T));
			}
			else if (typeof(T) == typeof(float))
			{
				return (T)Convert.ChangeType(PlayerPrefs.GetFloat(key), typeof(T));
			}
			else if (typeof(T) == typeof(string))
			{
				return (T)Convert.ChangeType(PlayerPrefs.GetString(key), typeof(T));
			}
			else if (typeof(T) == typeof(bool))
			{
				return (T)(object)(PlayerPrefs.GetInt(key) == 1);
			}
			else if (typeof(T) == typeof(Vector3))
			{				
				string[] vecStrList = PlayerPrefs.GetString(key).Split(",");

				return (T)Convert.ChangeType(new Vector3(float.Parse(vecStrList[0]), float.Parse(vecStrList[1]), float.Parse(vecStrList[2])), typeof(T));
			}
#endif //UNITY_EDITOR

			Debug.LogError("LocalPref型態不支援");

			return default(T);
		}
	}
}