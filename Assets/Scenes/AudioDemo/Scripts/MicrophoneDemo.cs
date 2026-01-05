using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Audio;
using XPlan.Utility;

namespace XPlan.Demo.Audio
{ 
    public class MicrophoneDemo : MonoBehaviour
    {
        [SerializeField] AudioSource playerSource;

        // streaming相關
        private System.Object streamLock    = new System.Object();
        private AudioClip streamingClip     = null;
        private Queue<float> streamQueue    = new Queue<float>(1 << 16);

        private int TARGET_SR               = 1;
        private const int TARGET_CHANNELS   = 1; // 強制單聲道播放

        private void Awake()
        {
            GameViewSizeForce.EnsureAndUseFixed("XPlan.Demo", 2160, 1440);
        
            TARGET_SR = AudioSettings.outputSampleRate; // 也可改成 48000 或你要的固定值
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.A))
            {
                TeardownStreamingClip();

                MicrophoneTools.StartRecording(0, (clip) =>
                {
                    playerSource.Stop();
                    playerSource.loop = false;
                    playerSource.clip = clip;

                    playerSource.Play();
                });
            }
            else if (Input.GetKeyUp(KeyCode.S))
            {
                MicrophoneTools.EndRecording();
            }
            else if (Input.GetKeyUp(KeyCode.D))
            {
                SetupStreamingClipIfNeeded();

                MicrophoneTools.StartRecordingStreaming(0, OnMicChunk);
            }
        }

        // 建立拉取式的串流 AudioClip 並掛到 AudioSource
        private void SetupStreamingClipIfNeeded()
        {
            if (streamingClip != null && playerSource.clip == streamingClip) return;

            TeardownStreamingClip();

            int lengthSamples   = TARGET_SR * TARGET_CHANNELS * 5; // 5 秒佔位即可
            streamingClip       = AudioClip.Create(
                                                    name: "LiveMicStream",
                                                    lengthSamples: lengthSamples,
                                                    channels: TARGET_CHANNELS,
                                                    frequency: TARGET_SR,
                                                    stream: true,
                                                    pcmreadercallback: OnPCMRead,
                                                    pcmsetpositioncallback: null
                                                );

            playerSource.Stop();
            playerSource.loop = true; // 串流拉取式通常 loop=true
            playerSource.clip = streamingClip;
            playerSource.Play();

            lock (streamLock) streamQueue.Clear();
        }

        private void TeardownStreamingClip()
        {
            if (streamingClip != null)
            {
                if (playerSource.clip == streamingClip)
                {
                    playerSource.Stop();
                    playerSource.clip = null;
                }

                Destroy(streamingClip);
                streamingClip = null;
            }
            lock (streamLock) streamQueue.Clear();
        }

        // 麥克風串流回呼：把 mic PCM 轉成固定播放格式（mono + TARGET_SR）後塞到 queue
        private void OnMicChunk(float[] micPcm, int micChannels, int micSampleRate)
        {
            // 1) 先 downmix 成 mono（若已 mono 則略過拷貝）
            float[] mono = (micChannels == 1) ? micPcm : MicrophoneTools.DownmixToMono(micPcm, micChannels);

            // 2) 取樣率不同就做線性重採樣；相同就直接用
            float[] toEnqueue = (micSampleRate == TARGET_SR)
                ? mono
                : MicrophoneTools.ResampleLinear(mono, micSampleRate, TARGET_SR);

            // 3) 塞到播放佇列；限制上限避免延遲累積
            lock (streamLock)
            {
                // 當 queue 裡的資料超過「兩秒份」時，就把最舊的資料丟掉（Dequeue），讓延遲永遠不會超過 2 秒
                int maxFloats = TARGET_SR * TARGET_CHANNELS * 2; // 2 秒上限，可依需求調整
                while (streamQueue.Count > maxFloats && streamQueue.Count > 0)
                    streamQueue.Dequeue();

                for (int i = 0; i < toEnqueue.Length; i++)
                    streamQueue.Enqueue(toEnqueue[i]);
            }
        }

        // AudioThread：AudioSource 需要多少資料，就來這裡跟我們要
        private void OnPCMRead(float[] data)
        {
            int needed = data.Length;
            int filled = 0;

            lock (streamLock)
            {
                while (filled < needed && streamQueue.Count > 0)
                    data[filled++] = streamQueue.Dequeue();
            }

            // 不足的部分補 0，避免雜訊
            for (int i = filled; i < needed; i++)
                data[i] = 0f;
        }
    }
}
