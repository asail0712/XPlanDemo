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

        private bool bTriggerOpen       = false;
        private bool bTriggerClose      = false;
        private Exception errorEx       = null;
        private Queue<string> msgQueue  = null;

        private MonoBehaviourHelper.MonoBehavourInstance callbackRoutine;
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
            uri = new Uri(wsUrl);
            ws  = new ClientWebSocket();

            callbackRoutine = MonoBehaviourHelper.StartCoroutine(Tick());
        }

        /// <summary>
        /// 打开链接
        /// </summary>
        public void Connect()
        {
            Task.Run(async () =>
            {
                if (ws.State == WebSocketState.Connecting || ws.State == WebSocketState.Open)
                { 
                    return;
                }

                string netErr = string.Empty;

                try
                {
                    //初始化链接
                    bIsUserClose    = false;
                    ws              = new ClientWebSocket();
                    errorEx         = null;
                    msgQueue        = new Queue<string>();

                    await ws.ConnectAsync(uri, CancellationToken.None);

                    bTriggerOpen = true;

                    //全部消息容器
                    List<byte> bs                   = new List<byte>();                    
                    var buffer                      = new byte[1024 * 4]; //缓冲区                    
                    WebSocketReceiveResult result   = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);//监听Socket信息
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

                                msgQueue.Enqueue(userMsg);

                                //清空消息容器
                                bs = new List<byte>();
                            }
                        }
                        //继续监听Socket信息
                        result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    netErr  = " .Net发生错误" + ex.Message;
                    errorEx = ex;
                }
				finally
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

						Close(status, desc + netErr);
					}
				}
			});
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

            Task.Run(async () =>
            {
                // 将要发送的数据转换为字节数组
                byte[] buffer = Encoding.UTF8.GetBytes(mess);

                // 创建 WebSocket 发送数据的缓冲区
                ArraySegment<byte> segment = new ArraySegment<byte>(buffer);

                // 发送消息
                await ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);

                Debug.Log($"送出訊息 {mess}!!");
            });

            return true;
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

            Task.Run(async () =>
            {
                //发送消息
                await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, CancellationToken.None);

                Debug.Log($"送出訊息 {bytes}!!");
            });

            return true;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            bIsUserClose = true;
            Close(WebSocketCloseStatus.NormalClosure, "用户主動關閉");
        }

        public void Close(WebSocketCloseStatus closeStatus, string statusDescription)
        {
            Task.Run(async () =>
            {
				if (bIsUserClose)
				{
					try
					{
						//关闭WebSocket（客户端发起）
						await ws.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
					}
					catch (Exception ex)
					{
                        errorEx = ex;
                    }
				}

				ws.Abort();
                ws.Dispose();

                bTriggerClose = true;
            });          
        }

        /****************************
         * 實作ITickable
         * *************************/
        private IEnumerator Tick()
		{
            while(true)
			{
                // 為了讓call back 都由主執行序觸發
                yield return new WaitForEndOfFrame();

                if (bTriggerOpen)
			    {
                    bTriggerOpen = false;

                    if (OnOpen != null)
                    {
                        OnOpen(this, new EventArgs());
                    }
                }

                if (errorEx != null)
                {
                    if (OnError != null)
                    {
                        OnError(this, errorEx);
                    }

                    errorEx = null;
                }

                if (bTriggerClose)
                {
                    bTriggerClose = false;

                    if (callbackRoutine != null)
                    {
                        callbackRoutine.StopCoroutine();
                        callbackRoutine = null;
                    }

                    if (OnClose != null)
                    {
                        OnClose(this, new EventArgs());
                    }
                }

                while(msgQueue != null && msgQueue.Count > 0)
			    {
                    if (OnMessage != null)
                    {
                        OnMessage(this, msgQueue.Dequeue());
                    }
                }
            }
        }
    }
}
