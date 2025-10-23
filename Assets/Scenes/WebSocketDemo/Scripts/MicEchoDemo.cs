using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Audio;
using XPlan.Net;

using Object = System.Object;

namespace XPlan.Demo.Websocket
{

    public class MicEchoDemo : MonoBehaviour, IEventHandler, IConnectHandler
    {
        [SerializeField] private Button speakBtn;
        [SerializeField] private string urlStr;

        // streaming相關
        private readonly Object streamLock  = new Object();
        private Queue<float> streamQueue    = new Queue<float>(1 << 16);        
        private bool bIsRecording           = false;/********** Mic 控制 **********/

        private int TARGET_SR               = 16000;
        private const int TARGET_CHANNELS   = 1; // 強制單聲道播放
        // 送出一個 frame 的樣本數（越小延遲越低；越大傳輸效率越好）
        // 這裡選 20ms 一包 => 16000 * 0.02 = 320 samples
        private const int PACKET_SAMPLES    = 320;

        private WebSocket webSocket;
        private AudioSource output;     // 用於播放 echo 回來的音訊

        // WebSocket 傳輸的封包格式（文字訊息 JSON）
        [Serializable]
        private class AudioFrame
        {
            public string t = "audio";   // 類型
            public int sr;               // sample rate
            public int ch;               // channels
            public string dtype = "f32"; // 資料型別
            public string data;          // Base64 的 PCM (float32 LE)
        }

        private void Awake()
        {
            // 建立播放用 AudioSource
            output                      = gameObject.GetComponent<AudioSource>();
            if (output == null) output  = gameObject.AddComponent<AudioSource>();
            output.playOnAwake          = true;
            output.loop                 = true;
            output.spatialBlend         = 0f;        // 2D
            output.clip                 = AudioClip.Create("EchoStream", TARGET_SR, TARGET_CHANNELS, TARGET_SR, true, OnAudioFilterReadPull);
            output.Play();
        }

        // Start is called before the first frame update
        private void Start()
		{
            webSocket = new WebSocket(Url.ToString(), new ConnectionRecovery(this));
            webSocket.Connect();            
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.A))
            {
                StartToSpeak();
            }
            else if (Input.GetKeyUp(KeyCode.A))
            {
                StopToSpeak();
            }
        }

        public void Open(IConnectHandler connectHandler)
        {
            Debug.Log("WebSocket opened");
        }

        public void Message(IConnectHandler connectHandler, string dataStr)
        {
            if (string.IsNullOrEmpty(dataStr)) return;

            // 嘗試解析為 AudioFrame
            try
            {
                var frame = JsonConvert.DeserializeObject<AudioFrame>(dataStr);

                if (frame != null && frame.t == "audio" && !string.IsNullOrEmpty(frame.data))
                {
                    byte[] bytes    = Convert.FromBase64String(frame.data);
                    float[] f32     = BytesToFloatArray(bytes);

                    // 收到的音訊若不是目標 SR / CH，做一次轉換
                    float[] toPlay = f32;
                    if (frame.ch != TARGET_CHANNELS)
                    {
                        // 只保留第一聲道（簡單處理）
                        toPlay = MicrophoneTools.DownmixToMono(toPlay, frame.ch);
                    }
                    if (frame.sr != TARGET_SR)
                    {
                        toPlay = MicrophoneTools.ResampleLinear(toPlay, frame.sr, TARGET_SR);
                    }

                    // 丟進播放 queue
                    lock (streamLock)
                    {
                        int maxFloats = TARGET_SR * TARGET_CHANNELS * 2; // 最多保留 2 秒，避免延遲無限增加
                        while (streamQueue.Count > maxFloats && streamQueue.Count > 0)
                            streamQueue.Dequeue();

                        for (int i = 0; i < toPlay.Length; i++)
                            streamQueue.Enqueue(toPlay[i]);
                    }
                    return;
                }
            }
            catch (Exception)
            {
                // 不是我們的 JSON 音訊包就當作文字訊息
            }   
        }

        public void Error(IConnectHandler connectHandler, string errorTxt)
        {
            Debug.LogWarning("WebSocket error: " + errorTxt);
        }

        public void Close(IConnectHandler connectHandler, bool bErrorHappen)
        {
            Debug.Log("WebSocket closed with Url Is " + connectHandler.Url);
        }

        private void StartToSpeak()
        {
            if (bIsRecording) return;
            bIsRecording = true;

            MicrophoneTools.StartRecordingStreaming(0, OnMicChunk);
        }

        private void StopToSpeak()
        {
            if (!bIsRecording) return;
            bIsRecording = false;

            MicrophoneTools.EndRecording();
        }

        // 麥克風串流回呼：送出到 echo server
        private float[] sendCache = new float[0];
        private int sendCacheFill = 0;

        // 麥克風串流回呼：把 mic PCM 轉成固定播放格式（mono + TARGET_SR）後塞到 queue
        private void OnMicChunk(float[] micPcm, int micChannels, int micSampleRate)
        {
            // 1) 先 downmix 成 mono（若已 mono 則略過拷貝）
            float[] mono    = (micChannels == 1) ? micPcm : MicrophoneTools.DownmixToMono(micPcm, micChannels);

            // 2) 取樣率不同就做線性重採樣；相同就直接用
            float[] fixedSr = (micSampleRate == TARGET_SR)
                ? mono
                : MicrophoneTools.ResampleLinear(mono, micSampleRate, TARGET_SR);

            // 3) chunking：累積到 PACKET_SAMPLES 就丟一包
            EnsureSendCacheCapacity(ref sendCache, sendCacheFill + fixedSr.Length);
            Array.Copy(fixedSr, 0, sendCache, sendCacheFill, fixedSr.Length);
            sendCacheFill += fixedSr.Length;

            int cursor = 0;
            while (sendCacheFill - cursor >= PACKET_SAMPLES)
            {
                // 取一個 frame
                var frame = new float[PACKET_SAMPLES];
                Array.Copy(sendCache, cursor, frame, 0, PACKET_SAMPLES);
                cursor += PACKET_SAMPLES;

                // 送出（JSON+Base64）
                SendAudioFrame(frame, TARGET_SR, TARGET_CHANNELS);
            }

            // 把未滿一包的殘餘移到開頭
            int leftOver = sendCacheFill - cursor;
            if (leftOver > 0)
                Array.Copy(sendCache, cursor, sendCache, 0, leftOver);
            sendCacheFill = leftOver;
        }

        private void EnsureSendCacheCapacity(ref float[] sendCache, int needed)
        {
            if (sendCache.Length >= needed) return;
            int cap = Math.Max(needed, sendCache.Length * 2 + 1024);
            Array.Resize(ref sendCache, cap);
        }

        /********** 傳輸：送出音訊（JSON / Base64） **********/
        private void SendAudioFrame(float[] samples, int sr, int ch)
        {
            if (webSocket == null)
            {
                Debug.LogWarning("Web Socket is Null !!");
                return;
            }

            try
            {
                byte[] bytes    = FloatArrayToBytes(samples); // float32 LE
                string b64      = Convert.ToBase64String(bytes);

                var payload     = new AudioFrame
                {
                    sr      = sr,
                    ch      = ch,
                    data    = b64
                };

                string json     = JsonConvert.SerializeObject(payload);

                // 1) 文字訊息（跨平台 echo 最保險）
                webSocket.Send(json);

                // 2) 如果你的 WebSocket 類別支援二進位，可以直接送 bytes
                //    但多數「公開 echo」只會原樣回你一個「同型別」的一包資料
                //webSocket.Send(bytes);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Send audio frame 異常: {e.Message}");
            }
        }

        /********** 播放端：AudioClip 的 PCM 拉取 **********/
        // 使用 AudioClip.SetData 動態推是不穩定；用 streaming clip 的 callback 比較穩
        private void OnAudioFilterReadPull(float[] data)
        {
            // 由 Unity 以 audio thread 來要資料
            int need = data.Length;

            lock (streamLock)
            {
                int i = 0;
                while (i < need && streamQueue.Count > 0)
                {
                    data[i++] = streamQueue.Dequeue();
                }
                // 不足的補 0（避免爆音）
                while (i < need) data[i++] = 0f;
            }
        }

        /********** 小工具 **********/
        private static byte[] FloatArrayToBytes(float[] arr)
        {
            byte[] bytes = new byte[arr.Length * 4];
            Buffer.BlockCopy(arr, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static float[] BytesToFloatArray(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return Array.Empty<float>();
            int count   = bytes.Length / 4;
            float[] arr = new float[count];
            Buffer.BlockCopy(bytes, 0, arr, 0, bytes.Length);
            return arr;
        }

        /*********************************
         * 實作IConnectHandler
         * *******************************/
        public Uri Url
		{
			get
			{
				// 公共 Websocket Test Server
				return new Uri(urlStr);
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
