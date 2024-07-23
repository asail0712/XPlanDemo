using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using XPlan.Scenes;
using XPlan.InputMode;
using XPlan.Observe;

namespace XPlan.Demo.Input
{
    public class ParentSceneLogic : SystemBase, INotifyReceiver
    {
        public Func<string> LazyGroupID
        {
            get; set;
        }

		protected override void OnInitialGameObject()
		{
            SceneController.Instance.RegisterScene("ChildScene1", 1);
            SceneController.Instance.RegisterScene("ChildScene2", 2);
            SceneController.Instance.RegisterScene("ChildScene3", 3);

            SceneController.Instance.StartScene("ChildScene1");
        }

    }
}