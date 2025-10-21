using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Utility;

// 參考資料
// https://blog.csdn.net/Mediary/article/details/118333666

namespace XPlan.Audio
{
    // 使用develop build 有機會在手機會有延遲
    // 生成的AudioClip要手動釋放

    public static class MicrophoneTools
    {
        static private bool bIsFinished;
        static private MonoBehaviourHelper.MonoBehavourInstance coroutine;
        static private AudioClip recordedClip;

        static public void StartRecording(int idx = 0, Action<AudioClip> finishAction = null, int bufferSec = 10)
        {
            string[] devices = Microphone.devices;
            if (devices.Length == 0 || idx >= devices.Length || idx < 0 || idx >= devices.Length)
            {
                Debug.LogWarning("未檢測到麥克風設備或 index 超出範圍。");
                return;
            }

            string selectedDevice = devices[idx]; // 選擇第一個可用的設備

            if(coroutine != null)
            {
                coroutine.StopCoroutine();
                coroutine = null;
            }

            // 選擇合適取樣率：若裝置不回報（0,0），就用當前輸出取樣率
            Microphone.GetDeviceCaps(selectedDevice, out int minFreq, out int maxFreq);
            
            int sampleRate = (minFreq == 0 && maxFreq == 0)
                                ? AudioSettings.outputSampleRate
                                : Mathf.Clamp(44100, minFreq == 0 ? 8000 : minFreq, maxFreq == 0 ? 48000 : maxFreq);


            coroutine = MonoBehaviourHelper.StartCoroutine(StartRecord_Internal(selectedDevice, finishAction, bufferSec, sampleRate));
        }

        static private IEnumerator StartRecord_Internal(string selectedDevice, Action<AudioClip> finishAction, int bufferSec, int sampleRate)
        {
            AudioClip micClip           = Microphone.Start(selectedDevice, true, bufferSec, sampleRate);

            // 等待裝置就緒（位置 > 0）
            const double startTimeout   = 2.0;
            double startTime            = AudioSettings.dspTime;

            // 等待裝置就緒（位置 > 0）
            while (Microphone.GetPosition(selectedDevice) <= 0)
            {
                if (AudioSettings.dspTime - startTime > startTimeout)
                {
                    Debug.LogError("麥克風啟動逾時。");
                    Microphone.End(selectedDevice);

                    yield break;
                }

                yield return null;
            }


            int channels            = micClip.channels;
            int clipSamples         = micClip.samples;              // 單聲道樣本數
            int interleavedLength   = clipSamples * channels;       // 交錯後總長度

            var buffer              = new List<float>(interleavedLength * 2);
            int prevPos             = 0; // 單位：樣本（非 float 長度），且是單聲道樣本數
            bIsFinished             = false;

            while (!bIsFinished)
            {
                int curPos = Microphone.GetPosition(selectedDevice); // 單位：樣本（單聲道）
                if (curPos < 0) curPos = 0;

                if (curPos != prevPos)
                {
                    if (curPos > prevPos)
                    {
                        AppendRange(micClip, prevPos, curPos - prevPos, channels, buffer);
                    }
                    else
                    {
                        // wrap：先拿 prevPos -> clipEnd
                        AppendRange(micClip, prevPos, clipSamples - prevPos, channels, buffer);

                        // 再拿 0 -> curPos
                        if (curPos > 0)
                        {
                            AppendRange(micClip, 0, curPos, channels, buffer);
                        }
                    }

                    prevPos = curPos;
                }

                yield return null;
            }

            // 停止前再把尾段補齊
            {
                int curPos = Microphone.GetPosition(selectedDevice);
                if (curPos < 0) curPos = 0;

                if (curPos != prevPos)
                {
                    if (curPos > prevPos)
                    {
                        AppendRange(micClip, prevPos, curPos - prevPos, channels, buffer);
                    }
                    else
                    {
                        AppendRange(micClip, prevPos, clipSamples - prevPos, channels, buffer);

                        if (curPos > 0)
                        {
                            AppendRange(micClip, 0, curPos, channels, buffer);
                        }
                    }
                }
            }

            Microphone.End(selectedDevice);

            // 建立成品（保留原通道數與取樣率，不強制單聲道）
            if (recordedClip != null)
            {
                GameObject.Destroy(recordedClip);
                recordedClip = null;
            }

            recordedClip = AudioClip.Create("MicrophoneRecord", buffer.Count / channels, channels, sampleRate, false);
            recordedClip.SetData(buffer.ToArray(), 0);

            finishAction?.Invoke(recordedClip);
        }

        /// <summary>
        /// 將 micClip 中 [start, count)（單位：樣本/每聲道）複製到 buffer（輸出為交錯格式）
        /// </summary>
        static private void AppendRange(AudioClip micClip, int startSample, int sampleCount, int channels, List<float> outBuffer)
        {
            if (sampleCount <= 0) return;

            int floatsNeeded = sampleCount * channels;
            var temp = new float[floatsNeeded];
            micClip.GetData(temp, startSample);
            outBuffer.AddRange(temp);
        }

        static public void EndRecording()
        {
            bIsFinished = true;
        }
    }
}
