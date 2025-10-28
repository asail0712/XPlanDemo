using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Net;
using static UnityEngine.Video.VideoPlayer;

namespace XPlan.Demo.Websocket
{

    public class WebSocketDemoScene : MonoBehaviour, IEventHandler, IConnectHandler
    {
        static private int MaxMsgCount = 15;

        [SerializeField] private Text showMsgTxt;
        [SerializeField] private InputField inputTxt;

        private WebSocket webSocket;
        private Queue<string> msgQueue = new Queue<string>();

        // Start is called before the first frame update
        void Start()
		{
            webSocket = new WebSocket(Url.ToString(), new ConnectionRecovery(this));
            webSocket.Connect();            
        }

        public void Open(IConnectHandler connectHandler)
        {
            Debug.Log("WebSocket opened");
        }

        public void Message(IConnectHandler connectHandler, string dataStr)
        {
            if (dataStr == null || dataStr == "")
            {
                return;
            }

            Debug.Log("Receiver Message is " + dataStr);

            msgQueue.Enqueue(dataStr);

            while(msgQueue.Count > MaxMsgCount)
			{
                msgQueue.Dequeue();
            }

            string[] msgArr = msgQueue.ToArray();

            showMsgTxt.text = "";

            for (int i = 0; i < msgArr.Length; ++i)
			{
                showMsgTxt.text += msgArr[i];
                showMsgTxt.text += '\n';
            }            
        }
        public void Binary(IConnectHandler handler, byte[] data)
        {
            // nothing to do
        }

        public void Error(IConnectHandler connectHandler, string errorTxt)
        {
            Debug.LogWarning("WebSocket error: " + errorTxt);
        }

        public void Close(IConnectHandler connectHandler, bool bErrorHappen)
        {
            Debug.Log("WebSocket closed with Url Is " + connectHandler.Url);
        }

        public void PressBtn()
		{
            if(inputTxt.text == "")
			{
                return;
			}

            SendMsg(inputTxt.text);
        }

        private void SendMsg(string msgStr)
        {
            if (webSocket == null)
            {
                Debug.LogWarning("Web Socket is Null !!");
                return;
            }

            try
            {
                webSocket.Send(msgStr);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Send msg 時發生異常, 原因為 {e.Message}");
            }
		}

		/*********************************
         * 實作IConnectHandler
         * *******************************/
		public Uri Url
		{
			get
			{
				// 公共 Websocket Test Server
				return new Uri("wss://echo.websocket.org");
			}
		}

		public void Connect()
		{
            webSocket.Connect();
        }

        public void InterruptConnect()
		{
            StartCoroutine(Reconnect(webSocket));
        }

		public void CloseConnect()
		{
            webSocket.CloseConnect();
        }

        private IEnumerator Reconnect(WebSocket ws)
        {
            yield return new WaitForSeconds(3);

            webSocket.Connect();
        }
    }
}
