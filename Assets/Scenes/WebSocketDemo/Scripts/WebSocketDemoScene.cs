using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Net;

namespace XPlan.Demo.Websocket
{

    public class WebSocketDemoScene : MonoBehaviour
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
            webSocket = new WebSocket(URL);
			webSocket.OnMessage += OnMessage;
            webSocket.OnOpen    += OnOpen;
            webSocket.OnError   += OnError;
            webSocket.OnClose   += OnClose;
            webSocket.Connect();            
        }

        private void OnOpen(object sender, System.EventArgs e)
        {
            Debug.Log("WebSocket opened");
        }

        private void OnMessage(object sender, string data)
        {
            if (data == null || data == "")
            {
                return;
            }

            Debug.Log("Receiver Message is " + data);

            msgQueue.Enqueue(data);

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

        private void OnError(object sender, Exception e)
        {
            Debug.LogWarning("WebSocket error: " + e.Message);
        }

        private void OnClose(object sender, EventArgs e)
        {
            Debug.Log("WebSocket closed with Url Is " + webSocket.Url + " reason is " + e.ToString());

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
