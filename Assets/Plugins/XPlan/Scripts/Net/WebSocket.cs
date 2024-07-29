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
        private ClientWebSocket ws      = null;
        private Uri uri                 = null;
        private bool bIsUserClose       = false;//是否最后由用户手动关闭

        private bool bTriggerOpen       = false;
        private bool bTriggerClose      = false;
        private Exception errorEx       = null;
        private Queue<string> msgQueue  = null;
        private List<byte> bs           = null;
        private byte[] buffer           = null;

        private MonoBehaviourHelper.MonoBehavourInstance callbackRoutine;
        
        public WebSocketState? State { get => ws?.State; }
        public Uri Url { get => uri; }

        public delegate void MessageEventHandler(object sender, string data);
        public delegate void ErrorEventHandler(object sender, Exception ex);

        public event EventHandler OnOpen;
        public event MessageEventHandler OnMessage;
        public event ErrorEventHandler OnError;
        public event EventHandler OnClose;

        public WebSocket(string wsUrl)
        {
            // 初始化
            uri         = new Uri(wsUrl);
            msgQueue    = new Queue<string>();

            // 緩衝區
            bs          = new List<byte>();
            buffer      = new byte[1024 * 4];
        }

        public void Connect()
        {
            callbackRoutine = MonoBehaviourHelper.StartCoroutine(Tick());

            Task.Run(async () =>
            {
                ws = new ClientWebSocket();

                if (ws.State == WebSocketState.Connecting || ws.State == WebSocketState.Open)
                {
                    return;
                }

                // reset數值
                string netErr   = string.Empty;
                bIsUserClose    = false;
                errorEx         = null;

                msgQueue.Clear();
                bs.Clear();
                Array.Clear(buffer, 0, buffer.Length);

                // 這邊有loop，因此不使用StartCoroutine避免效能受到影響
                try
                {
                    await ws.ConnectAsync(uri, CancellationToken.None);

                    // 等連線完成後觸發Connect
                    bTriggerOpen = true;

                    WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);//监听Socket信息
                    
                    while (!result.CloseStatus.HasValue)
                    {
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            bs.AddRange(buffer.Take(result.Count));

                            if (result.EndOfMessage)
                            {
                                // 收到的消息
                                string userMsg = Encoding.UTF8.GetString(bs.ToArray(), 0, bs.Count);

                                msgQueue.Enqueue(userMsg);

                                bs = new List<byte>();
                            }
                        }

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

        public bool Send(string mess)
        {
            if (ws.State != WebSocketState.Open)
            {
                return false;
            }

            Task.Run(async () =>
            {
                byte[] buffer               = Encoding.UTF8.GetBytes(mess);
                ArraySegment<byte> segment  = new ArraySegment<byte>(buffer);

                await ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);

                Debug.Log($"送出訊息 {mess}!!");
            });

            return true;
        }

        public bool Send(byte[] bytes)
        {
            if (ws.State != WebSocketState.Open)
            {
                return false;
            }

            Task.Run(async () =>
            {
                await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, CancellationToken.None);

                Debug.Log($"送出訊息 {bytes}!!");
            });

            return true;
        }

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
                        // 關閉WebSocket
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
            // 為了讓call back 都由主執行序觸發
            // 所以放在tick
            while (true)
            {
                // 等Frame的末尾再執行
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

                    if (OnClose != null)
                    {
                        OnClose(this, new EventArgs());
                    }

                    if (callbackRoutine != null)
                    {
                        callbackRoutine.StopCoroutine();
                        callbackRoutine = null;
                    }
                }

				while (msgQueue != null && msgQueue.Count > 0)
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
