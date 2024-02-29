using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using XPlan.Utility;

namespace XPlan.Net
{ 
    public class WebSocket
    {
        private ClientWebSocket ws  = null;
        private Uri uri             = null;
        private bool bIsUserClose   = false;//是否最后由用户手动关闭

        private List<byte> bs       = null;
        private byte[] buffer       = null;

        private MonoBehaviourHelper.MonoBehavourInstance connectRoutine;
        /// <summary>
        /// WebSocket状态
        /// </summary>
        public WebSocketState? State { get => ws?.State; }
        public Uri Url { get => uri; }

        /// <summary>
        /// 包含一个数据的事件
        /// </summary>
        public delegate void MessageEventHandler(object sender, string data);
        public delegate void ErrorEventHandler(object sender, Exception ex);

        /// <summary>
        /// 连接建立时触发
        /// </summary>
        public event EventHandler OnOpen;
        /// <summary>
        /// 客户端接收服务端数据时触发
        /// </summary>
        public event MessageEventHandler OnMessage;
        /// <summary>
        /// 通信发生错误时触发
        /// </summary>
        public event ErrorEventHandler OnError;
        /// <summary>
        /// 连接关闭时触发
        /// </summary>
        public event EventHandler OnClose;

        public WebSocket(string wsUrl)
        {
            // 初始化
            uri         = new Uri(wsUrl);

            // 緩衝區
            bs          = new List<byte>();
            buffer      = new byte[1024 * 4];
        }

        /// <summary>
        /// 打开链接
        /// </summary>
        public void Connect()
        {
            connectRoutine = MonoBehaviourHelper.StartCoroutine(connect_Internal());
        }

        private IEnumerator connect_Internal()
        { 
            ws = new ClientWebSocket();

            if (ws.State == WebSocketState.Connecting || ws.State == WebSocketState.Open)
            { 
                yield break;
            }

            // reset數值
            string netErr   = string.Empty;
            bIsUserClose    = false;

            bs.Clear();
            Array.Clear(buffer, 0, buffer.Length);
            
            Task connectTask = ws.ConnectAsync(uri, CancellationToken.None);

            yield return new WaitUntil(() => connectTask.IsCompleted);

            if(connectTask.IsFaulted)
			{
                OnError?.Invoke(this, new Exception(connectTask.Exception.ToString()));
                DoingClose();
                yield break;
			}

            OnOpen?.Invoke(this, new EventArgs());

            //全部消息容器                  
            Task<WebSocketReceiveResult> receiveTask = ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);//监听Socket信息

            yield return new WaitUntil(() => receiveTask.IsCompleted);

            if (receiveTask.IsFaulted)
            {
                OnError?.Invoke(this, new Exception(receiveTask.Exception.ToString()));
                DoingClose();
                yield break;
            }

            WebSocketReceiveResult result = receiveTask.Result;
            //是否关闭
            while (!result.CloseStatus.HasValue)
            {
                //文本消息
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    bs.AddRange(buffer.Take(result.Count));

                    //消息是否已接收完全
                    if (result.EndOfMessage)
                    {
                        //发送过来的消息
                        string userMsg = Encoding.UTF8.GetString(bs.ToArray(), 0, bs.Count);

                        OnMessage(this, userMsg);

                        //清空消息容器
                        bs = new List<byte>();
                    }
                }
                //继续监听Socket信息
                receiveTask = ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);//监听Socket信息

                yield return new WaitUntil(() => receiveTask.IsCompleted);

                if (receiveTask.IsFaulted)
                {
                    OnError?.Invoke(this, new Exception(receiveTask.Exception.ToString()));
                    DoingClose();
                    yield break;
                }

                result = receiveTask.Result;
            }
		}

        private void DoingClose()
		{
            if (!bIsUserClose)
            {
                WebSocketCloseStatus status;

                if (ws.CloseStatus == null)
                {
                    status = WebSocketCloseStatus.Empty;
                }
                else
                {
                    status = ws.CloseStatus.Value;
                }

                string desc = ws.CloseStatusDescription == null ? "" : ws.CloseStatusDescription;

                MonoBehaviourHelper.StartCoroutine(Close_Internal(status, desc));
            }
        }

        /// <summary>
        /// 使用连接发送文本消息
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="mess"></param>
        /// <returns>是否尝试了发送</returns>
        public bool Send(string mess)
        {
            if (ws.State != WebSocketState.Open)
            { 
                return false;
            }

            // 创建 WebSocket 发送数据的缓冲区
            byte[] buffer = Encoding.UTF8.GetBytes(mess);
            
            MonoBehaviourHelper.StartCoroutine(Send_Internal(buffer));

            return true;
        }
        private IEnumerator Send_Internal(byte[] buffer)
		{
            // 创建 WebSocket 发送数据的缓冲区
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer);

            // 发送消息
            Task sendTask = ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);

            yield return new WaitUntil(() => sendTask.IsCompleted);

            if (sendTask.IsFaulted)
            {
                OnError?.Invoke(this, new Exception(sendTask.Exception.ToString()));
                DoingClose();
                yield break;
            }

            Debug.Log($"送出訊息 !!");
        }

        /// <summary>
        /// 使用连接发送字节消息
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="mess"></param>
        /// <returns>是否尝试了发送</returns>
        public bool Send(byte[] bytes)
        {
            if (ws.State != WebSocketState.Open)
            {
                return false;
            }

            MonoBehaviourHelper.StartCoroutine(Send_Internal(bytes));

            return true;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            bIsUserClose = true;
            MonoBehaviourHelper.StartCoroutine(Close_Internal(WebSocketCloseStatus.NormalClosure, "用户主動關閉"));
        }

        public IEnumerator Close_Internal(WebSocketCloseStatus closeStatus, string statusDescription)
        {
			if (bIsUserClose)
			{
				//关闭WebSocket（客户端发起）
				Task closeTask = ws.CloseAsync(closeStatus, statusDescription, CancellationToken.None);

                if (closeTask.IsFaulted)
                {
                    OnError?.Invoke(this, new Exception(closeTask.Exception.ToString()));
                    DoingClose();
                    yield break;
                }
            }

			ws.Abort();
            ws.Dispose();

            if (OnClose != null)
            {
                OnClose(this, new EventArgs());
            }

            if (connectRoutine != null)
            {
                connectRoutine.StopCoroutine();
                connectRoutine = null;
            }  
        }
    }
}
