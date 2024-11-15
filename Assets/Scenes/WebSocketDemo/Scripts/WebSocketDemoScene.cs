using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Net;

namespace XPlan.Demo.Websocket
{

    public class WebSocketDemoScene : MonoBehaviour, IEventHandler
    {
        [SerializeField] private Text showMsgTxt;
        [SerializeField] private InputField inputTxt;

        // 公共 Websocket Test Server
        static private string URL       = "wss://echo.websocket.org";
        static private int MaxMsgCount  = 15;

        private WebSocket webSocket;
        private Queue<string> msgQueue = new Queue<string>();

        // Start is called before the first frame update
        void Start()
		{
            webSocket = new WebSocket(URL, new ConnectionRecovery(this));
            webSocket.Connect();            
        }

        public void Open(IEventHandler eventHandler)
        {
            Debug.Log("WebSocket opened");
        }

        public void Message(IEventHandler eventHandler, string dataStr)
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

        public void Error(IEventHandler eventHandler, string errorTxt)
        {
            Debug.LogWarning("WebSocket error: " + errorTxt);
        }

        public void Close(IEventHandler eventHandler)
        {
            Debug.Log("WebSocket closed with Url Is " + webSocket.Url);

            if (webSocket != null)
            {
                StartCoroutine(Reconnect(webSocket));
            }
        }

        private IEnumerator Reconnect(WebSocket ws)
        {
            yield return new WaitForSeconds(3);

            webSocket.Connect();
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
    }
}
