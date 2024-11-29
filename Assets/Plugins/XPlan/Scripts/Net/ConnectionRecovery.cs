using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Utility;

namespace XPlan.Net
{
    public class ConnectionRecovery : IEventHandler
    {
        private int timesToMaxReconnect                 = 0;
        private int timesToCurrReconnect                = 0;
        private float timeToWait                        = 5f;
        private IEventHandler eventHandler              = null;
        private Lazy<IConnectHandler> connectLazyGetter = null;
        private IConnectHandler connectHandler          = null;

        private MonoBehaviourHelper.MonoBehavourInstance reconnectCoroutine;

        // Start is called before the first frame update
        public ConnectionRecovery(Lazy<IConnectHandler> connectLazyGetter, IEventHandler handler = null, int numOfReconnect = 30, float timeToWait = 4f)
        {
            this.eventHandler           = handler;
            this.connectLazyGetter      = connectLazyGetter;
            this.timeToWait             = timeToWait;
            this.timesToCurrReconnect   = 0;
            this.timesToMaxReconnect    = numOfReconnect;            
        }

        public ConnectionRecovery(IConnectHandler connectHandler, IEventHandler handler = null, int numOfReconnect = 30, float timeToWait = 4f)
        {
            this.eventHandler           = handler;
            this.connectHandler         = connectHandler;
            this.timeToWait             = timeToWait;
            this.timesToCurrReconnect   = 0;
            this.timesToMaxReconnect    = numOfReconnect;
        }

        public void Open(IEventHandler handler)
		{
            timesToCurrReconnect    = 0;

            eventHandler?.Open(handler);
        }
        public void Close(IEventHandler handler, bool bErrorHappen)
		{
            // 判斷是否是使用者自行關閉
            if(!bErrorHappen)
            {
                eventHandler?.Close(handler, false);
                return;
            }

            // 判斷斷線次數來決定是否要結束
            if (++timesToCurrReconnect > timesToMaxReconnect)
            {
                eventHandler?.Close(handler, true);
                return;
            }

            Debug.Log($"Reconnect in {timesToCurrReconnect} times");

            if(reconnectCoroutine != null)
			{
                return;
			}

            reconnectCoroutine = MonoBehaviourHelper.StartCoroutine(Reconnect());
        }
        public void Error(IEventHandler handler, string errorTxt)
		{
            eventHandler?.Error(handler, errorTxt);
        }
        public void Message(IEventHandler handler, string msgTxt)
		{
            eventHandler?.Message(handler, msgTxt);
        }

        /**********************************
         * 重連機制
         * *******************************/
        private IEnumerator Reconnect()
		{
            yield return new WaitForSeconds(timeToWait);

            if(connectHandler != null)
			{
                connectHandler?.Connect();
            }
            else
			{
                connectLazyGetter.Value?.Connect();
            }

            reconnectCoroutine.StopCoroutine();
            reconnectCoroutine  = null;
        }
    }
}
