using System.Collections;
using UnityEngine.SceneManagement;

using XPlan.Interface;
using XPlan.Utility;

namespace XPlan.Command
{

	public class UnloadSceneCommand : ICommand
	{
		private int sceneIdx;
		private ICommand cmd;

		public UnloadSceneCommand(int idx, ICommand c = null)
		{
			sceneIdx	= idx;
			cmd			= c;
		}

		IEnumerator UnloadScene()
		{
			SceneManager.UnloadSceneAsync(sceneIdx);

			if(cmd != null)
			{
				cmd.Execute();
			}

			yield return null;
		}

		public void Execute()
		{
			MonoBehaviourHelper.StartCoroutine(UnloadScene());
		}
	}
}