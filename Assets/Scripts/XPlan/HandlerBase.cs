using System;

using XPlan.Interface;
using XPlan.UI;

namespace XPlan
{
	public class HandlerBase : IUIListener
	{
		protected void DirectCallUI<T>(string uniqueID, T value)
		{
			UISystem.DirectCall<T>(uniqueID, value);
		}

		protected void DirectCallUI(string uniqueID)
		{
			UISystem.DirectCall(uniqueID);
		}

		protected void DirectCallUI(string uniqueID, params object[] paramList)
		{
			UISystem.DirectCall(uniqueID, paramList);
		}

		protected void AddUIListener<T>(string uniqueID, Action<T> callback)
		{
			UISystem.RegisterCallback(uniqueID, this, (param)=> 
			{
				callback?.Invoke(param.GetValue<T>());
			});
		}

		protected void AddUIListener(string uniqueID, Action callback)
		{
			UISystem.RegisterCallback(uniqueID, this, (dump) =>
			{
				callback?.Invoke();
			});
		}

		protected void RemoveUIListener(string uniqueID)
		{
			UISystem.UnregisterCallback(uniqueID, this);
		}

		protected void RemoveAllUIListener()
		{
			UISystem.UnregisterAllCallback(this);
		}

		public void Dispose(bool bAppQuit)
		{
			RemoveAllUIListener();

			OnDispose(bAppQuit);
		}

		protected virtual void OnDispose(bool bAppQuit)
		{
			// for override
		}

		public void PostInitial()
		{
			OnPostInitial();
		}

		protected virtual void OnPostInitial()
		{
			// for override
		}
	}
}

