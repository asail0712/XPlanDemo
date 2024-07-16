using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

using XPlan.Extensions;
using XPlan.Utility;

namespace XPlan.Scenes
{
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
		public LoadInfo(int sceneType)
			: base(sceneType)
		{
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
		}

		protected override void OnRelease(bool bAppQuit)
		{
		}

		/************************************
		 * 場景切換處理
		 * **********************************/
		public void StartScene(int sceneIdx)
		{
			ChangeTo(sceneIdx);
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

		public bool ChangeTo(int sceneType, bool bForceChange = false)
		{
			if (currSceneStack.Count == 0)
			{
				LoadScene(sceneType, true);
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
						LoadScene(sceneType);
						UnloadScene(currSceneType, bForceChange);
						break;
					}
				}
				else
				{
					LoadScene(sceneType);
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
				Debug.Log($"載入關卡 {info.sceneType}");
				AsyncOperation loadOperation	= SceneManager.LoadSceneAsync(info.sceneType, LoadSceneMode.Additive);
				loadRoutine						= StartCoroutine(WaitLoadingScene(loadOperation, info.sceneType));

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

		protected bool LoadScene(int sceneType, bool bImmediately = false)
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
				loadRoutine						= StartCoroutine(WaitLoadingScene(loadOperation, sceneType));

				currSceneStack.Add(sceneType);
			}
			else
			{
				changeQueue.Add(new LoadInfo(sceneType));
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

		private IEnumerator WaitLoadingScene(AsyncOperation asyncOperation, int sceneType)
		{
			while (!asyncOperation.isDone)
			{
				float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f); // 0.9 是載入完成的標誌
				Debug.Log("關卡載入進度: " + (progress * 100) + "%");
				yield return null;
			}

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
	}
}

