using UnityEngine;

// MonoBehaviour 函數的執行先後順序
// https://home.gamer.com.tw/creationDetail.php?sn=2491667

namespace XPlan
{
    public class InstallerBase : MonoBehaviour
    {
		private HandlerManager handlerManager = null;

		/**********************************************
		* Handler管理
		**********************************************/
		protected void RegisterHandler(HandlerBase handler)
		{
			handlerManager.RegisterScope(handler, this);
		}
		protected void UnregisterHandler(HandlerBase handler)
		{
			handlerManager.UnregisterScope(handler, this);
		}

		/**********************************************
		* 初始化
		**********************************************/
		protected void Awake()
		{
			handlerManager = new HandlerManager();

			OnPreInitial();
		}

		// Start is called before the first frame update
		void Start()
        {
			OnInitialGameObject();
			OnInitialHandler();

			PostInitial();
		}

		private void PostInitial()
		{
			handlerManager.PostInitial();

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
			if(handlerManager != null)
			{
				handlerManager.UnregisterScope(this, bAppQuit);
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

			if (handlerManager != null)
			{
				handlerManager.TickHandler(Time.deltaTime);
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
