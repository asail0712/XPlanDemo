using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan;

namespace XPlan.DebugMode
{ 
    public class DebugInstallerBase : InstallerBase
    {
		sealed override protected void OnPreInitial()
		{

		}

		sealed override protected void OnInitialGameObject()
		{
			// Debug Manager有Initial的話，表示不是單一Scene獨立測試，就把該Debug物件關閉
			gameObject.SetActive(!DebugManager.IsInitial());
		}

		sealed override protected void OnInitialHandler()
		{
			if (DebugManager.IsInitial())
			{
				// Debug Manager有Initial的話，表示不是單一Scene獨立測試，就不需要做初始化
				return;
			}

			OnInitialDebugHandler();
		}

		//override protected void OnPreUpdate(float deltaTime)
		//{
		//	Debug.Log("XDDD!!!!");
		//}

		//override protected void OnRelease(bool bIsAppQuit)
		//{
		//	Debug.Log("XDDD");
		//}

		virtual protected void OnInitialDebugHandler()
		{

		}		
	}
}