using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

using XPlan.Interface;
using XPlan.Utility;

namespace XPlan.Command
{

	public class LoadSceneCommand : ICommand
	{
		private int sceneIdx;
		private bool bAsyncLoad;
		private LoadSceneMode loadMode;
		private ICommand cmd;

		private AsyncOperation asyncOper;

		public LoadSceneCommand(int idx, bool bAsync = false, LoadSceneMode mode = LoadSceneMode.Single, ICommand c = null)
		{
			sceneIdx	= idx;
			bAsyncLoad	= bAsync;
			loadMode	= mode;
			cmd			= c;

			if (bAsyncLoad)
			{
				MonoBehaviourHelper.StartCoroutine(LoadScene());
			}
		}

		IEnumerator LoadScene()
		{
			asyncOper						= SceneManager.LoadSceneAsync(sceneIdx, loadMode);
			asyncOper.allowSceneActivation	= false;
			asyncOper.completed				+= LoadSceneComplete;

			yield return null;
		}

		private void LoadSceneComplete(AsyncOperation operation)
		{
			Debug.Log("場景加载完成");
		}

		public void Execute()
		{
			if(bAsyncLoad)
			{
				if (asyncOper == null)
				{
					return;
				}

				asyncOper.allowSceneActivation = true;

			}
			else
			{
				SceneManager.LoadScene(sceneIdx, loadMode);
			}

			if (cmd != null)
			{
				cmd.Execute();
			}
		}
	}
}