using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
//using UnityEngine.LightTransport;
using XPlan.Utility;

namespace XPlan.Net
{
    public class WebSocket: IConnectHandler, IEventHandler, ISendHandler
    {
        private ClientWebSocket ws                  = null;
        private CancellationTokenSource _cts        = null;
        private Task _recvTask;

        private bool bIsUserClose                   = false; // 判斷是否為使用者主動關閉

        private Exception errorEx                   = null;

        // 把背景執行緒事件丟回主執行緒
        private readonly ConcurrentQueue<Action> mainThreadActions = new();

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

            if (callbackRoutine != null)
			{
                callbackRoutine.StopCoroutine();
            }

            callbackRoutine = MonoBehaviourHelper.StartCoroutine(ProcessMainThreadActions());

            // 建立新 ws 與 CTS
            ws              = new ClientWebSocket();
            _cts?.Cancel();
            _cts            = new CancellationTokenSource();

            bIsUserClose    = false;
            errorEx         = null;

            // 直接啟動 async 任務（無 Task.Run）
            _recvTask       = ConnectAndPumpAsync(_cts.Token); // fire-and-forget
        }

        private async Task ConnectAndPumpAsync(CancellationToken ct)
        {            
            // 這邊有loop，因此不使用StartCoroutine避免效能受到影響
            try
            {
                await ws.ConnectAsync(Url, ct).ConfigureAwait(false);

                // 告知「已連線」
                EmitOnMain(() => Open(this));

                // 緩衝區
                using var ms                        = new MemoryStream();
                byte[] recvBuf                      = new byte[1024 * 64];
                WebSocketMessageType currentType    = WebSocketMessageType.Text;
                    
                while (ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
                {
                    WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(recvBuf), ct).ConfigureAwait(false);//监听Socket信息

                    // 使用者中斷
                    if (bIsUserClose) break;

                    // 意外終止判斷
                    if (result.MessageType == WebSocketMessageType.Close) break;

                    // 用於支援 分段訊息（fragmented message）
                    if (ms.Length == 0) currentType = result.MessageType; // 記住這則訊息的型別
                    if (result.Count > 0) ms.Write(recvBuf, 0, result.Count);

                    if (result.EndOfMessage)
                    {
                        byte[] data = ms.ToArray(); // 複製出乾淨 byte[]
                        ms.SetLength(0); // 清空，準備下一則訊息

                        // 直接在主緒發事件（不累積旗標/佇列）
                        if (currentType == WebSocketMessageType.Text)
                        {
                            EmitOnMain(() => Message(this, Encoding.UTF8.GetString(data)));
                        }
                        else
                        {
                            EmitOnMain(() => Binary(this, data));
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常: 取消/關閉流程
            }
            catch (Exception ex)
            {
                errorEx = ex;
                EmitOnMain(() => Error(this, ex.Message));
            }
            finally
            {
                if (!bIsUserClose)
                {
                    WebSocketCloseStatus status = ws.CloseStatus ?? WebSocketCloseStatus.Empty;
                    string desc                 = ws.CloseStatusDescription ?? "";

                    await CloseAsync(status, desc + (errorEx != null ? $" .Net错误 {errorEx.Message}" : ""))
                        .ConfigureAwait(false);
                }
                else
                {
                    await CloseAsync(WebSocketCloseStatus.NormalClosure, "用户主動關閉")
                        .ConfigureAwait(false);
                }
            }
        }

        public void CloseConnect()
        {
            bIsUserClose = true;
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
                    await ws.SendAsync(segment, type, true, _cts.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) { /* 關閉中忽略 */ }
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
                // 先發取消，讓接收迴圈收斂
                _cts?.Cancel();

                if (ws != null && (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived))
                {
                    using var closeCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    try
                    {
                        await ws.CloseAsync(closeStatus, statusDescription, closeCts.Token)
                                .ConfigureAwait(false);
                    }
                    catch
                    {
                        // 若 CloseAsync 卡住，最後保底 Abort
                        ws.Abort();
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Record($"CloseAsync failed: {ex.Message}", LogType.Assert);
            }
            finally
            {
                try { await (_recvTask ?? Task.CompletedTask).ConfigureAwait(false); } catch { }
                ws?.Dispose();
                ws = null;

                if (callbackRoutine != null)
                {
                    callbackRoutine.StopCoroutine();
                }

                bool bError = closeStatus != WebSocketCloseStatus.NormalClosure;
                EmitOnMain(() => Close(this, bError));
            }
        }

        private void EmitOnMain(Action action)
        {
            if (action == null) return;

            // 依你的工具替換這行即可
            mainThreadActions.Enqueue(action);
        }

        private IEnumerator ProcessMainThreadActions()
        {
            while (true)
            {
                yield return null;

                while (mainThreadActions.TryDequeue(out var action))
                {
                    action?.Invoke();
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
