using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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
			sceneIdx	= s;
			level		= l;
		}
	}

	public class SceneChangeInfo
	{
		public int sceneIdx;

		public SceneChangeInfo(int sceneIdx)
		{
			this.sceneIdx = sceneIdx;
		}
	}

	public class LoadInfo : SceneChangeInfo
	{
		// 設定活動場景 影響天空盒 光源 NavMesh 等
		public bool bActiveScene;
		public Action finishAction;

		public LoadInfo(int sceneIdx, bool bActiveScene, Action finishAction)
			: base(sceneIdx)
		{
			this.bActiveScene = bActiveScene;
			this.finishAction = finishAction;
		}
	}

	public class UnloadInfo : SceneChangeInfo
	{
		public UnloadInfo(int sceneIdx)
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

		private List<SceneChangeInfo> changeQueue		= new List<SceneChangeInfo>();

		private Coroutine loadRoutine					= null;
		private Coroutine unloadRoutine					= null;

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

			int stackIdx = currSceneStack.Count - 2;

			while (currSceneStack[stackIdx] == -1)
            {
				--stackIdx;
			}

			ChangeTo(currSceneStack[stackIdx]);

			return true;
		}

		public bool ChangeTo(string sceneName, bool bActiveScene = true, Action finishAction = null)
		{
			int buildIndex = GetBuildIndexByName(sceneName);

			return ChangeTo(buildIndex, bActiveScene, finishAction);
		}

		private void AddSceneStack(int sceneIdx)
		{
			int scenelevel = GetLevel(sceneIdx);

			while(currSceneStack.Count < scenelevel)
			{
				if(currSceneStack.Count == 0)
                {
					// stack的第一層一定為-999
					currSceneStack.Add(-999);
				}
				else
                {
					// 新添加的場景與stack階層有落差時，使用-1來填補
					currSceneStack.Add(-1);
				}				
			}

			currSceneStack.Add(sceneIdx);
		}

		private void RemoveSceneStack(int sceneIdx)
		{			
			// 移除scene時，要連同-1一起移除
			while(currSceneStack[currSceneStack.Count - 1] == sceneIdx
				|| currSceneStack[currSceneStack.Count - 1] == -1)
			{
				currSceneStack.RemoveAt(currSceneStack.Count - 1);
			}
		}

		private void ChangeSceneStack(int scenelevel, int sceneIdx)
		{
			currSceneStack[scenelevel] = sceneIdx;
		}

		public bool ChangeTo(int buildIndex, bool bActiveScene = true, Action finishAction = null)
		{
			if (currSceneStack.Count == 0)
			{
				// 立刻加載
				AsyncOperation loadOperation	= SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
				loadRoutine						= StartCoroutine(WaitLoadingScene(loadOperation, new LoadInfo(buildIndex, bActiveScene, finishAction)));

				AddSceneStack(buildIndex);
				return true;
			}

			for (int i = currSceneStack.Count - 1; i >= 0; --i)
			{
				if(i >= currSceneStack.Count)
                {
					continue;
                }

				int currSceneIndex	= currSceneStack[i];
				int currScenelevel	= GetLevel(currSceneIndex);
				int newScenelevel	= GetLevel(buildIndex);

				if (currScenelevel > newScenelevel)
				{
					// 考慮到SceneLevel的差距，所以強制關閉，不用等回調
					AddQueueUnload(currSceneIndex);

					RemoveSceneStack(currSceneIndex);
				}
				else if (currScenelevel == newScenelevel)
				{
					if (buildIndex == currSceneIndex)
					{
						return true;
					}
					else 
					{
						// 先loading 再做unload 避免畫面出現空白的過度
						AddQueueLoad(buildIndex, finishAction, bActiveScene);
						AddQueueUnload(currSceneIndex);

						ChangeSceneStack(currScenelevel, buildIndex);

						break;
					}
				}
				else
				{
					AddQueueLoad(buildIndex, finishAction, bActiveScene);

					AddSceneStack(buildIndex);
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
			if(changeQueue.Count == 0 || loadRoutine != null || unloadRoutine != null)
			{
				return;
			}

			SceneChangeInfo info = changeQueue[0];

			if(info is LoadInfo)
			{
				LoadInfo loadInfo	= (LoadInfo)info;
				Scene loadScene		= SceneManager.GetSceneByBuildIndex(loadInfo.sceneIdx);

				if (!loadScene.isLoaded)
				{
					Debug.Log($"載入關卡 {info.sceneIdx}");
					AsyncOperation loadOperation	= SceneManager.LoadSceneAsync(loadInfo.sceneIdx, LoadSceneMode.Additive);
					loadRoutine						= StartCoroutine(WaitLoadingScene(loadOperation, loadInfo));
				}
			}
			else if(info is UnloadInfo)
			{
				UnloadInfo unloadInfo	= (UnloadInfo)info;
				Scene unloadScene		= SceneManager.GetSceneByBuildIndex(unloadInfo.sceneIdx);

				if (unloadScene.isLoaded)
				{
					Debug.Log($"卸載關卡 {unloadInfo.sceneIdx}");
					AsyncOperation loadOperation	= SceneManager.UnloadSceneAsync(unloadInfo.sceneIdx);
					unloadRoutine					= StartCoroutine(WaitUnloadingScene(loadOperation));
				}
			}
			else
			{
				Debug.LogError("目前沒有這種load型別 !");
			}

			// 移除掉執行的change info
			changeQueue.RemoveAt(0);
		}

		protected void AddQueueLoad(int sceneIdx, Action finishAction, bool bActiveScene)
		{
			Debug.Log($"oo加入載入佇列oo {sceneIdx}");
			changeQueue.Add(new LoadInfo(sceneIdx, bActiveScene, finishAction));
		}

		protected void AddQueueUnload(int sceneIdx)
		{
			if(sceneIdx == -1)
            {
				return;
            }

			Debug.Log($"xx加入卸載佇列xx {sceneIdx}");
			changeQueue.Add(new UnloadInfo(sceneIdx));
		}

		private IEnumerator WaitLoadingScene(AsyncOperation asyncOperation, LoadInfo loadinfo)// int sceneIdx, bool bActiveScene, Action finishAction)
		{
			yield return new WaitUntil(() => asyncOperation.isDone);

			if(loadinfo.bActiveScene)
			{
				// 設定活動場景 影響天空盒 光源 NavMesh 等

				Scene scene = SceneManager.GetSceneByBuildIndex(loadinfo.sceneIdx);
				SceneManager.SetActiveScene(scene);
			}

			loadinfo.finishAction?.Invoke();

			loadRoutine = null;
		}

		private IEnumerator WaitUnloadingScene(AsyncOperation asyncOperation)
		{
			yield return new WaitUntil(() => asyncOperation.isDone);

			unloadRoutine = null;
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
				return currSceneStack.FindLastIndex((E04) => 
				{
					return E04 == -1;
				});
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
	}
}

