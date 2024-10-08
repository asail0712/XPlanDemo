﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

using XPlan.DebugMode;
using XPlan.Extensions;
using XPlan.Utility;

namespace XPlan.Scenes
{
	[Serializable]
	public class SceneData
	{
		[SerializeField]
		public string sceneName;

		[SerializeField]
		public int sceneLevel;
	}

	public struct SceneInfo
	{
		public int sceneType;
		public int level;
		public List<Action> triggerToFadeOutList;
		public List<Func<bool>> isFadeOutFinishList;

		public SceneInfo(int s, int l)
		{
			sceneType				= s;
			level					= l;
			triggerToFadeOutList	= new List<Action>();
			isFadeOutFinishList		= new List<Func<bool>>();
		}
	}

	public class ChangeInfo
	{
		public int sceneType;

		public ChangeInfo(int sceneType)
		{
			this.sceneType = sceneType;
		}
	}

	public class LoadInfo : ChangeInfo
	{
		public bool bActiveScene;
		public Action finishAction;

		public LoadInfo(int sceneType, bool bActiveScene, Action finishAction)
			: base(sceneType)
		{
			this.bActiveScene = bActiveScene;
			this.finishAction = finishAction;
		}
	}

	public class UnloadInfo : ChangeInfo
	{
		public UnloadInfo(int sceneType)
			: base(sceneType)
		{
		}
	}

	public class UnloadInfoImmediately : ChangeInfo
	{
		public UnloadInfoImmediately(int sceneIdx)
			: base(sceneIdx)
		{
		}
	}

	public class SceneController : CreateSingleton<SceneController>
	{
		[SerializeField] private List<SceneData> sceneDataList;
		[SerializeField] private string startSceneName;

		static private List<SceneInfo> sceneInfoList	= new List<SceneInfo>();
		private List<int> currSceneStack				= new List<int>();

		private List<ChangeInfo> changeQueue			= new List<ChangeInfo>();

		private Coroutine unloadRoutine					= null;
		private Coroutine loadRoutine					= null;

		/************************************
		* 初始化
		* **********************************/
		protected override void InitSingleton()
		{
			if(sceneDataList == null || sceneDataList.Count == 0)
			{
				return;
			}

			// 註冊Scene
			sceneDataList.ForEach((E04) => 
			{
				RegisterScene(E04.sceneName, E04.sceneLevel);
			});

			// 設定開始Scene
			if (startSceneName == "")
			{
				startSceneName = sceneDataList[0].sceneName;
			}

			StartScene(startSceneName);
		}

		protected override void OnRelease(bool bAppQuit)
		{
			if(sceneDataList == null)
			{
				return;
			}

			sceneDataList.Clear();
		}

		/************************************
		 * 場景切換處理
		 * **********************************/
		public bool StartScene(int sceneIdx)
		{
			return ChangeTo(sceneIdx);
		}

		public bool StartScene(string sceneName)
		{
			int buildIndex = GetBuildIndexByName(sceneName);

			return ChangeTo(buildIndex);
		}

		public bool BackTo(string sceneName)
		{
			int idx = currSceneStack.FindIndex((sceneIdx) => 
			{
				return sceneIdx == GetBuildIndexByName(sceneName);
			});

			if (idx == -1)
			{
				return false;
			}

			ChangeTo(currSceneStack[idx]);

			return true;
		}

		public bool BackFrom()
		{
			if (currSceneStack.Count < 2)
			{
				return false;
			}

			ChangeTo(currSceneStack[currSceneStack.Count - 2]);

			return true;
		}

		public bool ChangeTo(string sceneName, Action finishAction = null, bool bActiveScene = true, bool bForceChange = false)
		{
			int buildIndex = GetBuildIndexByName(sceneName);

			return ChangeTo(buildIndex, finishAction, bActiveScene, bForceChange);
		}

		public bool ChangeTo(int sceneType, Action finishAction = null, bool bActiveScene = true, bool bForceChange = false)
		{
			if (unloadRoutine != null || loadRoutine != null)
			{
				return false;
			}

			if (currSceneStack.Count == 0)
			{
				LoadScene(sceneType, finishAction, bActiveScene, true);
				return true;
			}

			for (int i = currSceneStack.Count - 1; i >= 0; --i)
			{
				int currSceneType	= currSceneStack[i];
				int currScenelevel	= GetLevel(currSceneType);
				int newScenelevel	= GetLevel(sceneType);

				if (currScenelevel > newScenelevel)
				{
					// 考慮到SceneLevel的差距，所以強制關閉，不用等回調
					UnloadScene(currSceneType, bForceChange);

				}
				else if (currScenelevel == newScenelevel)
				{
					if (sceneType == currSceneType)
					{
						return true;
					}
					else 
					{
						// 先loading 再做unload 避免畫面太空
						LoadScene(sceneType, finishAction, bActiveScene);
						UnloadScene(currSceneType, bForceChange);
						break;
					}
				}
				else
				{
					LoadScene(sceneType, finishAction, bActiveScene);
					break;
				}
			}

			return true;
		}

		/************************************
		* 場景載入與卸載
		* **********************************/

		protected override void OnPreUpdate(float deltaT)
		{
			ChangeSceneProcess(deltaT);
		}

		public void ChangeSceneProcess(float deltaTime)
		{
			if(changeQueue.Count == 0 || unloadRoutine != null || loadRoutine != null)
			{
				return;
			}

			ChangeInfo info = changeQueue[0];

			if(info is LoadInfo)
			{
				LoadInfo loadInfo = (LoadInfo)info;

				Debug.Log($"載入關卡 {info.sceneType}");
				AsyncOperation loadOperation	= SceneManager.LoadSceneAsync(loadInfo.sceneType, LoadSceneMode.Additive);
				loadRoutine						= StartCoroutine(WaitLoadingScene(loadOperation, loadInfo.sceneType, loadInfo.bActiveScene, loadInfo.finishAction));

				currSceneStack.Add(info.sceneType);
			}
			else if(info is UnloadInfo)
			{
				Debug.Log($"卸載關卡 {info.sceneType}");
				unloadRoutine = StartCoroutine(WaitAllFadeOut(UnloadScene_Internal, info.sceneType, false));

				currSceneStack.Remove(info.sceneType);
			}
			else if (info is UnloadInfoImmediately)
			{
				Debug.Log($"立刻卸載關卡 {info.sceneType}");
				unloadRoutine = StartCoroutine(WaitAllFadeOut(UnloadScene_Internal, info.sceneType, true));

				currSceneStack.Remove(info.sceneType);
			}
			else
			{
				Debug.LogError("目前沒有這種load型別 !");
			}

			// 移除掉執行的change info
			changeQueue.RemoveAt(0);
		}

		protected bool LoadScene(int sceneType, Action finishAction, bool bActiveScene, bool bImmediately = false)
		{
			Scene scene = SceneManager.GetSceneByBuildIndex(sceneType);

			// 檢查沒有被載入
			if (scene.isLoaded)
			{
				return false;
			}

			if(bImmediately)
			{
				Debug.Log($"載入關卡 {sceneType}");
				AsyncOperation loadOperation	= SceneManager.LoadSceneAsync(sceneType, LoadSceneMode.Additive);
				loadRoutine						= StartCoroutine(WaitLoadingScene(loadOperation, sceneType, bActiveScene, finishAction));

				currSceneStack.Add(sceneType);
			}
			else
			{
				changeQueue.Add(new LoadInfo(sceneType, bActiveScene, finishAction));
			}
			
			return true;
		}

		protected bool UnloadScene(int sceneType, bool bImmediately = false)
		{
			if(bImmediately)
			{
				changeQueue.Add(new UnloadInfoImmediately(sceneType));
			}
			else
			{
				changeQueue.Add(new UnloadInfo(sceneType));
			}
			
			return true;
		}

		protected void UnloadScene_Internal(int sceneIdx)
		{ 
			Scene scene = SceneManager.GetSceneByBuildIndex(sceneIdx);

			if (!scene.isLoaded)
			{
				return;
			}

			SceneManager.UnloadSceneAsync(sceneIdx);			
		}

		/************************************
		* UI Fade in/out流程處理
		* **********************************/
		static public void RegisterFadeCallback(int sceneType, Action FadeOutFunc, Func<bool> retFunc)
		{
			int idx = sceneInfoList.FindIndex((X) =>
			{
				return X.sceneType == sceneType;
			});

			if(idx != -1)
			{
				sceneInfoList[idx].triggerToFadeOutList.Add(FadeOutFunc);
				sceneInfoList[idx].isFadeOutFinishList.Add(retFunc);
			}
		}

		static public void UnregisterFadeCallback(int sceneType, Action func, Func<bool> retFunc)
		{
			int idx = sceneInfoList.FindIndex((X) =>
			{
				return X.sceneType == sceneType;
			});

			if (idx != -1)
			{
				sceneInfoList[idx].triggerToFadeOutList.Remove(func);
				sceneInfoList[idx].isFadeOutFinishList.Remove(retFunc);
			}
		}

		private IEnumerator WaitLoadingScene(AsyncOperation asyncOperation, int sceneType, bool bActiveScene, Action finishAction)
		{
			while (!asyncOperation.isDone)
			{
				float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f); // 0.9 是載入完成的標誌
				Debug.Log("關卡載入進度: " + (progress * 100) + "%");
				yield return null;
			}

			if(bActiveScene)
			{
				Scene scene = SceneManager.GetSceneByBuildIndex(sceneType);
				SceneManager.SetActiveScene(scene);
			}

			finishAction?.Invoke();

			loadRoutine = null;
		}

		private IEnumerator WaitAllFadeOut(Action<int> ReallyUnload, int sceneType, bool bImmediately)
		{
			if (!bImmediately)
			{
				List<Func<bool>> isFadeOutCallback		= GetIsFadeOutCallback(sceneType);
				List<Action> triggerToFadeOutCallback	= GetTriggerToFadeOutCallback(sceneType);

				int numOfCallbacks = triggerToFadeOutCallback == null ? 0 : triggerToFadeOutCallback.Count;
				int numOfCompleted = 0;

				foreach (Action callback in triggerToFadeOutCallback)
				{
					callback?.Invoke();
				}

				while (numOfCompleted < numOfCallbacks)
				{
					numOfCompleted = 0;

					foreach (Func<bool> UnloadResult in isFadeOutCallback)
					{
						// 判斷fade out 表演是否結束

						if (UnloadResult == null)
						{
							++numOfCompleted;
						}
						else if (UnloadResult.Invoke())
						{
							++numOfCompleted;
						}
					}

					yield return null;
				}
			}

			ReallyUnload(sceneType);

			unloadRoutine = null;
		}

		/************************************
		* Scene添加
		* **********************************/
		public void RegisterScene(int sceneType, int level)
		{
			List<SceneInfo> sceneList = sceneInfoList.FindAll((X)=> 
			{
				return X.sceneType == sceneType;
			});

			if(sceneList.Count == 0)
			{
				sceneInfoList.Add(new SceneInfo(sceneType, level));
			}			
		}

		public void RegisterScene(string sceneName, int level)
		{
			int buildIndex = GetBuildIndexByName(sceneName);

			RegisterScene(buildIndex, level);
		}

		public void  UnregisterScene(int sceneType)
		{
			sceneInfoList.RemoveAll((X) =>
			{
				return X.sceneType == sceneType;
			});
		}

		/************************************
		* 其他
		* **********************************/
		public bool IsInScene<T>(T sceneType) where T : struct, IConvertible
		{
			// 將型態轉換成整數會是多少
			int sceneInt = sceneType.ToInt32(CultureInfo.InvariantCulture);

			if (sceneInt >= 0)
			{
				return sceneInt == GetCurrSceneIdx();
			}

			return false;
		}

		
		private int GetLevel(int sceneType)
		{
			int idx = sceneInfoList.FindIndex((X)=> 
			{
				return X.sceneType == sceneType;
			});

			if(idx == -1)
			{
				return -1;
			}

			return sceneInfoList[idx].level;
		}

		private List<Action> GetTriggerToFadeOutCallback(int sceneType)
		{
			int idx = sceneInfoList.FindIndex((X) =>
			{
				return X.sceneType == sceneType;
			});

			if (idx == -1)
			{
				return null;
			}

			return sceneInfoList[idx].triggerToFadeOutList;
		}

		private List<Func<bool>> GetIsFadeOutCallback(int sceneType)
		{
			int idx = sceneInfoList.FindIndex((X) =>
			{
				return X.sceneType == sceneType;
			});

			if (idx == -1)
			{
				return null;
			}

			return sceneInfoList[idx].isFadeOutFinishList;
		}

		public int GetCurrSceneIdx()
		{
			int currScene = currSceneStack.Count - 1;

			if(currSceneStack.IsValidIndex<int>(currScene))
			{
				return currSceneStack[currScene];
			}
			else
			{
				Debug.LogWarning("Level Error");
				return -1;
			}			
		}

		public string GetCurrSceneName()
		{
			int currSceneIdx = currSceneStack.Count - 1;

			if (currSceneStack.IsValidIndex<int>(currSceneIdx))
			{
				Scene currScene = SceneManager.GetSceneByBuildIndex(currSceneStack[currSceneIdx]);

				return currScene.name;
			}
			else
			{
				Debug.LogWarning("Level Error");
				return "";
			}
		}

		private int GetBuildIndexByName(string sceneName)
		{
			for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
			{
				string path = SceneUtility.GetScenePathByBuildIndex(i);
				string name = Path.GetFileNameWithoutExtension(path);

				if (name == sceneName)
				{
					return i;
				}
			}

			LogSystem.Record($"{sceneName} 不在Build List裡面", LogType.Error);

			return -1; // 返回 -1 表示未找到该场景
		}
	}
}

