using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using XPlan.Scenes;
using XPlan.InputMode;
using XPlan.Observe;

namespace XPlan.Demo.InputMode
{
    public class ParentSceneLogic : SystemBase, INotifyReceiver
    {
        public Func<string> GetLazySectorID
        {
            get; set;
        }

		protected override void OnInitialGameObject()
		{
            // 相同Level的場景只能同時存在一個，因此相同Level場景載入時，會同時卸載另一個相同Level的場景
            // 由於三個場景的level都不一致，所以允許三個場景並存，並使用Stack紀錄
            SceneController.Instance.RegisterScene("ChildScene1", 1);
            SceneController.Instance.RegisterScene("ChildScene2", 2);
            SceneController.Instance.RegisterScene("ChildScene3", 3);

            // 開始時，是ChildScene1優先
            SceneController.Instance.StartScene("ChildScene1");
        }

    }
}