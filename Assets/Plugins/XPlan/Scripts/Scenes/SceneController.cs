using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

using XPlan.DebugMode;
using XPlan.Utilitys;
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
		public int sceneIdx;
		public int level;

		public SceneInfo(int s, int l)
		{
			sceneIdx				= s;
			level					= l;
		}
	}

	public class ChangeInfo
	{
		public int sceneIdx;

		public ChangeInfo(int sceneIdx)
		{
			this.sceneIdx = sceneIdx;
		}
	}

	public class LoadInfo : ChangeInfo
	{
		public bool bActiveScene;
		public Action finishAction;

		public LoadInfo(int sceneIdx, bool bActiveScene, Action finishAction)
			: base(sceneIdx)
		{
			this.bActiveScene = bActiveScene;
			this.finishAction = finishAction;
		}
	}

	public class UnloadInfo : ChangeInfo
	{
		public UnloadInfo(int sceneIdx)
			: base(sceneIdx)
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

		private Coroutine loadRoutine					= null;
		private int loadingSceneIdx						= -1;

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

		public bool ChangeTo(int buildIndex, Action finishAction = null, bool bActiveScene = true, bool bForceChange = false)
		{
			if (!CanLoad(buildIndex))
			{
				Debug.LogWarning($"{buildIndex} 此場景無法載入");

				return false;
			}

			if (currSceneStack.Count == 0)
			{
				LoadScene(buildIndex, finishAction, bActiveScene, true);
				return true;
			}

			for (int i = currSceneStack.Count - 1; i >= 0; --i)
			{
				int currSceneIndex	= currSceneStack[i];
				int currScenelevel	= GetLevel(currSceneIndex);
				int newScenelevel	= GetLevel(buildIndex);

				if (currScenelevel > newScenelevel)
				{
					// 考慮到SceneLevel的差距，所以強制關閉，不用等回調
					UnloadScene(currSceneIndex, bForceChange);

				}
				else if (currScenelevel == newScenelevel)
				{
					if (buildIndex == currSceneIndex)
					{
						return true;
					}
					else 
					{
						// 先loading 再做unload 避免畫面太空
						LoadScene(buildIndex, finishAction, bActiveScene);
						UnloadScene(currSceneIndex, bForceChange);
						break;
					}
				}
				else
				{
					LoadScene(buildIndex, finishAction, bActiveScene);
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
			if(changeQueue.Count == 0 || loadRoutine != null)
			{
				return;
			}

			ChangeInfo info = changeQueue[0];

			if(info is LoadInfo)
			{
				LoadInfo loadInfo = (LoadInfo)info;

				Debug.Log($"載入關卡 {info.sceneIdx}");
				AsyncOperation loadOperation	= SceneManager.LoadSceneAsync(loadInfo.sceneIdx, LoadSceneMode.Additive);
				loadRoutine						= StartCoroutine(WaitLoadingScene(loadOperation, loadInfo.sceneIdx, loadInfo.bActiveScene, loadInfo.finishAction));

				currSceneStack.Add(info.sceneIdx);
			}
			else if(info is UnloadInfo)
			{
				Debug.Log($"卸載關卡 {info.sceneIdx}");
				UnloadScene_Internal(info.sceneIdx);

				currSceneStack.Remove(info.sceneIdx);
			}
			else if (info is UnloadInfoImmediately)
			{
				Debug.Log($"立刻卸載關卡 {info.sceneIdx}");
				UnloadScene_Internal(info.sceneIdx);

				currSceneStack.Remove(info.sceneIdx);
			}
			else
			{
				Debug.LogError("目前沒有這種load型別 !");
			}

			// 移除掉執行的change info
			changeQueue.RemoveAt(0);
		}

		protected bool LoadScene(int sceneIdx, Action finishAction, bool bActiveScene, bool bImmediately = false)
		{
			Scene scene = SceneManager.GetSceneByBuildIndex(sceneIdx);

			// 檢查沒有被載入
			if (scene.isLoaded)
			{
				return false;
			}

			if(bImmediately)
			{
				Debug.Log($"載入關卡 {sceneIdx}");
				AsyncOperation loadOperation	= SceneManager.LoadSceneAsync(sceneIdx, LoadSceneMode.Additive);
				loadRoutine						= StartCoroutine(WaitLoadingScene(loadOperation, sceneIdx, bActiveScene, finishAction));

				currSceneStack.Add(sceneIdx);
			}
			else
			{
				changeQueue.Add(new LoadInfo(sceneIdx, bActiveScene, finishAction));
			}
			
			return true;
		}

		protected bool UnloadScene(int sceneIdx, bool bImmediately = false)
		{
			if(bImmediately)
			{
				changeQueue.Add(new UnloadInfoImmediately(sceneIdx));
			}
			else
			{
				changeQueue.Add(new UnloadInfo(sceneIdx));
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

		private IEnumerator WaitLoadingScene(AsyncOperation asyncOperation, int sceneIdx, bool bActiveScene, Action finishAction)
		{
			loadingSceneIdx = sceneIdx;

			while (!asyncOperation.isDone)
			{
				float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f); // 0.9 是載入完成的標誌
				//Debug.Log("關卡載入進度: " + (progress * 100) + "%");
				yield return null;
			}

			if(bActiveScene)
			{
				Scene scene = SceneManager.GetSceneByBuildIndex(sceneIdx);
				SceneManager.SetActiveScene(scene);
			}

			finishAction?.Invoke();

			loadRoutine = null;
		}

		/************************************
		* Scene添加
		* **********************************/
		public void RegisterScene(int sceneIdx, int level)
		{
			List<SceneInfo> sceneList = sceneInfoList.FindAll((X)=> 
			{
				return X.sceneIdx == sceneIdx;
			});

			if(sceneList.Count == 0)
			{
				sceneInfoList.Add(new SceneInfo(sceneIdx, level));
			}			
		}

		public void RegisterScene(string sceneName, int level)
		{
			int buildIndex = GetBuildIndexByName(sceneName);

			RegisterScene(buildIndex, level);
		}

		public void  UnregisterScene(int sceneIdx)
		{
			sceneInfoList.RemoveAll((X) =>
			{
				return X.sceneIdx == sceneIdx;
			});
		}

		/************************************
		* 其他
		* **********************************/
		public bool IsInScene<T>(T sceneIdx) where T : struct, IConvertible
		{
			// 將型態轉換成整數會是多少
			int sceneInt = sceneIdx.ToInt32(CultureInfo.InvariantCulture);

			if (sceneInt >= 0)
			{
				return sceneInt == GetCurrSceneIdx();
			}

			return false;
		}

		
		private int GetLevel(int sceneIdx)
		{
			int idx = sceneInfoList.FindIndex((X)=> 
			{
				return X.sceneIdx == sceneIdx;
			});

			if(idx == -1)
			{
				return -1;
			}

			return sceneInfoList[idx].level;
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

		private bool CanLoad(int sceneIdx)
		{
			int newSceneLevel		= GetLevel(sceneIdx);
			int loadingSceneLevel	= GetLevel(loadingSceneIdx);

			return loadRoutine == null || (newSceneLevel > loadingSceneLevel);
		}
	}
}

