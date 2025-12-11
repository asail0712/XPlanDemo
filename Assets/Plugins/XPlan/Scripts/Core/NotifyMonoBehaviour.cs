using System;
using UnityEngine;
using XPlan.Observe;
using XPlan.Weaver.Runtime;

namespace XPlan
{
    public class NotifyMonoBehaviour : MonoBehaviour, INotifyReceiver
    {
        private void Awake()
        {
            // 建構完成後，嘗試呼叫 IL Weaving 產生的 Hook
            WeaverHookInvoker.Invoke(this, "__LogicComponent_WeaverHook");
        }

        /*************************
        * Notify相關
        * ***********************/
        protected void RegisterNotify<T>(Action<T> notifyAction) where T : MessageBase
        {
            NotifyHelper.RegisterNotify<T>(this, notifyAction);
        }

        protected void RegisterNotify<T>(ReceiveOption option, Action<T> notifyAction) where T : MessageBase
        {
            NotifyHelper.RegisterNotify<T>(this, option, notifyAction);
        }

        protected void SendMsg<T>(params object[] args) where T : MessageBase
        {
            NotifyHelper.SendMsg<T>(false, args);
        }

        protected void SendMsgAsync<T>(params object[] args) where T : MessageBase
        {
            NotifyHelper.SendMsg<T>(true, args);
        }
    }
}
