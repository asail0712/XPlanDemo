using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.DebugMode;

// MonoBehaviour 函數的執行先後順序
// https://home.gamer.com.tw/creationDetail.php?sn=2491667

namespace XPlan
{
    public class SystemBase : MonoBehaviour
    {
		private LogicManager logicManager = null;

		/**********************************************
		* Handler管理
		**********************************************/
		protected void RegisterLogic(LogicComponentBase logicComp)
		{
			// 確認msg群發的group
			logicComp.LazyGroupID = () =>
			{ 
				return GetType().ToString();
			};

			logicManager.RegisterScope(logicComp, this);
		}
		protected void UnregisterLogic(LogicComponentBase logicComp)
		{
			logicManager.UnregisterScope(logicComp, this);
		}

		/**********************************************
		* 初始化
		**********************************************/
		protected void Awake()
		{
			logicManager = new LogicManager();

			OnPreInitial();
		}

		// Start is called before the first frame update
		void Start()
        {
			OnInitialGameObject();

			OnInitialHandler();

			StartCoroutine(PostInitial());
		}

		private IEnumerator PostInitial()
		{
			yield return new WaitForEndOfFrame();

			logicManager.PostInitial();

			OnPostInitial();
		}

		protected virtual void OnPreInitial()
		{
			// for override
		}

		protected virtual void OnInitialGameObject()
		{
			// for override
		}
		protected virtual void OnInitialHandler()
		{
			// for override
		}

		protected virtual void OnPostInitial()
		{
			// for override
		}

		/**********************************************
		* 資源釋放時的處理
		**********************************************/
		private bool bAppQuit;

		void OnDestroy()
		{
			if(logicManager != null)
			{
				logicManager.UnregisterScope(this, bAppQuit);
			}
			
			OnRelease(bAppQuit);
		}

		protected virtual void OnRelease(bool bAppQuit)
		{
			// for overrdie;
		}

		private void OnApplicationQuit()
		{
			bAppQuit = true;
		}

		/**********************************************
        * Tick相關功能
        **********************************************/
		void Update()
		{
			//Debug.Log($"Installbase Update !!");

			OnPreUpdate(Time.deltaTime);

			if (logicManager != null)
			{
				logicManager.TickLogic(Time.deltaTime);
			}

			OnPostUpdate(Time.deltaTime);
		}

		protected virtual void OnPreUpdate(float deltaTime)
		{
			// for override
		}

		protected virtual void OnPostUpdate(float deltaTime)
		{
			// for override
		}
	}
}
