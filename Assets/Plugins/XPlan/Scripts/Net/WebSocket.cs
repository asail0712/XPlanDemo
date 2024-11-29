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
    public class WebSocket: IConnectHandler, IEventHandler, ISendHandler
    {
        private ClientWebSocket ws      = null;
        private Uri uri                 = null;
        private List<byte> bs           = null;
        private byte[] buffer           = null;
        private bool bIsUserClose       = false; // 判斷是否為使用者主動關閉
        private bool bInterruptConnect  = false;

        private bool bTriggerOpen       = false;
        private bool bTriggerClose      = false;
        private Exception errorEx       = null;
        private Queue<string> msgQueue  = null;

        private MonoBehaviourHelper.MonoBehavourInstance callbackRoutine;
        private IEventHandler eventHandler;

        public WebSocketState? State { get => ws?.State; }
        public Uri Url { get => uri; }

        public WebSocket(string wsUrl, IEventHandler handler)
        {
            // 初始化
            uri             = new Uri(wsUrl);
            eventHandler    = handler;
        }

        /***********************************
         * 實作IConnectHandler
         * ********************************/
        public void Connect()
        {
            if (ws != null && (ws.State == WebSocketState.Connecting || ws.State == WebSocketState.Open))
			{
                return;
            }

            msgQueue        = new Queue<string>();

            // 緩衝區
            bs              = new List<byte>();
            buffer          = new byte[1024 * 4];

            if(callbackRoutine != null)
			{
                callbackRoutine.StopCoroutine();
            }

            callbackRoutine = MonoBehaviourHelper.StartCoroutine(Tick());

            Task.Run(async () =>
            {
                // reset數值
                ws              = new ClientWebSocket();
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
                    bTriggerOpen        = true;
                    bInterruptConnect   = false;

                    WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);//监听Socket信息
                    
                    while (!result.CloseStatus.HasValue)
                    {
                        if (bInterruptConnect)
                        {
                            bInterruptConnect = false;

                            throw new Exception("強制觸發例外導致連線中斷");
                        }

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

        public void Reconnect()
        {
            bInterruptConnect = true;
        }

        public void CloseConnect()
        {
            bIsUserClose = true;
            Close(WebSocketCloseStatus.NormalClosure, "用户主動關閉");
        }

        /***********************************
         * 實作ISendHandler
         * ********************************/
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

        private void Close(WebSocketCloseStatus closeStatus, string statusDescription)
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
                        LogSystem.Record(ex.Message, LogType.Assert);
                    }
                }

                ws.Abort();
                ws.Dispose();

                bTriggerClose = true;
            });
        }

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

                    Open(this);
                }

                if (errorEx != null)
                {
                    Error(this, errorEx.Message);                 
                }

                if (bTriggerClose)
                {
                    Close(this, errorEx != null);

                    bTriggerClose   = false;
                    errorEx         = null;

                    if (callbackRoutine != null)
                    {
                        callbackRoutine.StopCoroutine();
                        callbackRoutine = null;
                    }
                }

				while (msgQueue != null && msgQueue.Count > 0)
				{
					Message(this, msgQueue.Dequeue());
				}
            }
        }

        /********************************************
         * 實作 IStatefulConnection
         * *****************************************/
        public void Open(IEventHandler handler)
		{
            eventHandler?.Open(handler);
		}

        public void Close(IEventHandler handler, bool bErrorHappen)
		{
            eventHandler?.Close(handler, bErrorHappen);
        }

        public void Error(IEventHandler handler, string errorTxt)
		{
            eventHandler?.Error(handler, errorTxt);
        }

        public void Message(IEventHandler handler, string msgTxt)
		{
            eventHandler?.Message(handler, msgTxt);
        }
    }
}
