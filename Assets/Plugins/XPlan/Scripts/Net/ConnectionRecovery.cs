using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Utility;

namespace XPlan.Net
{
    public class ConnectionRecovery : IEventHandler
    {
        private int timesToMaxReconnect     = 0;
        private int timesToCurrReconnect    = 0;
        private float timeToWait            = 5f;
        private IEventHandler eventHandler  = null;
        private bool bIsFailure             = false;

        private MonoBehaviourHelper.MonoBehavourInstance reconnectCoroutine;

        // Start is called before the first frame update
        public ConnectionRecovery(IEventHandler handler, int numOfReconnect = 3, float timeToWait = 5f)
        {
            this.eventHandler           = handler;
            this.timeToWait             = timeToWait;
            this.timesToCurrReconnect   = 0;
            this.timesToMaxReconnect    = numOfReconnect;
            this.bIsFailure             = false;
        }

        public void Open(IEventHandler handler)
		{
            bIsFailure              = false;
            timesToCurrReconnect    = 0;

            eventHandler?.Open(handler);
        }
        public void Close(IEventHandler handler)
		{
            if(!bIsFailure)
            {
                eventHandler?.Close(handler);
                return;
            }

            if (++timesToCurrReconnect > timesToMaxReconnect)
            {
                eventHandler?.Close(handler);
                return;
            }

            Debug.Log($"Reconnect in {timesToCurrReconnect} times");

            IConnectHandler connectHandler = handler as IConnectHandler;

            if (connectHandler != null)
            {
                reconnectCoroutine = MonoBehaviourHelper.StartCoroutine(Reconnect(connectHandler));
            }
        }
        public void Error(IEventHandler handler, string errorTxt)
		{
            bIsFailure = true;

            eventHandler?.Error(handler, errorTxt);
        }
        public void Message(IEventHandler handler, string msgTxt)
		{
            eventHandler?.Message(handler, msgTxt);
        }

        /**********************************
         * 重連機制
         * *******************************/
        private IEnumerator Reconnect(IConnectHandler connectHandler)
		{
            yield return new WaitForSeconds(timeToWait);

            connectHandler?.Connect();

            reconnectCoroutine.StopCoroutine();
            reconnectCoroutine = null;
        }
    }
}
