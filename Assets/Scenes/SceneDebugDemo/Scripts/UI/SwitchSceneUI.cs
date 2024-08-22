using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Scenes;
using XPlan.UI;

namespace XPlan.Demo.SceneDebug
{ 
    public class SwitchSceneUI : UIBase
    {
        [SerializeField] Button switchSceneBtn;
        [SerializeField] string sceneName;
        [SerializeField] Text switchSceneTxt;
        [SerializeField] string showStr;

        // Start is called before the first frame update
        void Awake()
		{
			RegisterButton("", switchSceneBtn, () => 
            {
                SceneController.Instance.ChangeTo(sceneName);
            });

            switchSceneTxt.text = showStr;
        }
	}
}