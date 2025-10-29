using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.LightTransport;
using XPlan.Utility;

namespace XPlan.Net
{
    public readonly struct WsInboundMessage
    {
        public WebSocketMessageType Type { get; }
        public byte[] Binary { get; }
        public string Text { get => Encoding.UTF8.GetString(Binary, 0, Binary.Length);}
        public WsInboundMessage(WebSocketMessageType type, byte[] byteArr)
        {
            Type    = type;
            Binary  = byteArr;
        }
    }

    public class WebSocket: IConnectHandler, IEventHandler, ISendHandler
    {
        private ClientWebSocket ws                  = null;
        private bool bIsUserClose                   = false; // 判斷是否為使用者主動關閉
        private bool bInterruptConnect              = false;

        private bool bTriggerOpen                   = false;
        private bool bTriggerClose                  = false;
        private Exception errorEx                   = null;
        private Queue<WsInboundMessage> msgQueue    = null;

        private MonoBehaviourHelper.MonoBehavourInstance callbackRoutine;
        private IEventHandler eventHandler;

        // 避免多執行緒同時送出訊息
        private readonly SemaphoreSlim _sendLock    = new(1, 1);

        public WebSocketState? State { get => ws?.State; }
        public Uri Url { get; set; }

        public WebSocket(string wsUrl, IEventHandler handler)
        {
            // 初始化
            Url             = new Uri(wsUrl);
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

            msgQueue = new Queue<WsInboundMessage>();

            if (callbackRoutine != null)
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

                // 這邊有loop，因此不使用StartCoroutine避免效能受到影響
                try
                {
                    await ws.ConnectAsync(Url, CancellationToken.None);

                    // 等連線完成後觸發Connect
                    bTriggerOpen        = true;
                    bInterruptConnect   = false;

                    // 緩衝區
                    using var ms                        = new MemoryStream();
                    byte[] recvBuf                      = new byte[1024 * 4];
                    WebSocketMessageType currentType    = WebSocketMessageType.Text;
                    
                    while (ws.State == WebSocketState.Open)
                    {
                        WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(recvBuf), CancellationToken.None);//监听Socket信息
                    
                        // 意外終止判斷
                        if (bInterruptConnect) throw new Exception("強制觸發例外導致連線中斷");
                        if (result.MessageType == WebSocketMessageType.Close) break;

                        // 用於支援 分段訊息（fragmented message）
                        if (ms.Length == 0) currentType = result.MessageType; // 記住這則訊息的型別
                        if (result.Count > 0) ms.Write(recvBuf, 0, result.Count);

                        if (result.EndOfMessage)
                        {
                            byte[] data = ms.ToArray(); // 複製出乾淨 byte[]
                            msgQueue.Enqueue(new WsInboundMessage(currentType, data));
                            ms.SetLength(0); // 清空，準備下一則訊息
                        }
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
                        WebSocketCloseStatus status = ws.CloseStatus ?? WebSocketCloseStatus.Empty;
                        string desc                 = ws.CloseStatusDescription ?? "";

                        _ = CloseAsync(status, desc + netErr);
                    }
                }
            });
        }

        public void InterruptConnect()
        {
            bInterruptConnect = true;
        }

        public void CloseConnect()
        {
            bIsUserClose = true;
            _ = CloseAsync(WebSocketCloseStatus.NormalClosure, "用户主動關閉");
        }

        /***********************************
         * 實作ISendHandler
         * ********************************/
        public bool Send(string mess)
        {
            if (ws == null || ws.State != WebSocketState.Open) return false;

            byte[] bytes = Encoding.UTF8.GetBytes(mess);
            // 不要用 Task.Run 亂丟；改成 fire-and-forget，但內部會序列化 + 捕例外
            _ = SendInternalAsync(bytes, WebSocketMessageType.Text);
            return true;
        }

        public bool Send(byte[] bytes)
        {
            if (ws == null || ws.State != WebSocketState.Open) return false;

            _ = SendInternalAsync(bytes, WebSocketMessageType.Binary);
            return true;
        }

        private async Task SendInternalAsync(byte[] bytes, WebSocketMessageType type)
        {
            var segment = new ArraySegment<byte>(bytes);

            await _sendLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (ws != null && ws.State == WebSocketState.Open)
                {
                    await ws.SendAsync(segment, type, true, CancellationToken.None).ConfigureAwait(false);
                }
            }
            catch (Exception ex) { Debug.LogWarning($"Send(Binary) error: {ex.Message}"); }
            finally
            {
                _sendLock.Release();
            }
        }

        private async Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription)
        {
            try
            {
                if (bIsUserClose && ws != null)
                {
                    await ws.CloseAsync(closeStatus, statusDescription, CancellationToken.None)
                                 .ConfigureAwait(false);
                
                }
            }
            catch (Exception ex)
            {
                LogSystem.Record($"CloseAsync failed: {ex.Message}", LogType.Assert);
            }
            finally
            {
                ws?.Abort();
                ws?.Dispose();
                bTriggerClose = true;
            }
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

				while (msgQueue != null && msgQueue.TryDequeue(out var item))
				{
                    if(item.Type == WebSocketMessageType.Text)
                    {
                        Message(this, item.Text);
                    }
                    else
                    {
                        Binary(this, item.Binary);
                    }
				}
            }
        }

        /********************************************
         * 實作 IStatefulConnection
         * *****************************************/
        public void Open(IConnectHandler handler)
		{
            eventHandler?.Open(handler);
		}

        public void Close(IConnectHandler handler, bool bErrorHappen)
		{
            eventHandler?.Close(handler, bErrorHappen);
        }

        public void Error(IConnectHandler handler, string errorTxt)
		{
            eventHandler?.Error(handler, errorTxt);
        }

        public void Message(IConnectHandler handler, string text)
		{
            eventHandler?.Message(this, text);
        }

        public void Binary(IConnectHandler handler, byte[] data)
        {
            eventHandler?.Binary(handler, data);
        }
    }
}
