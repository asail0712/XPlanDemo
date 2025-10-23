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

        // =========================
        // 完整錄音 -> 回傳 AudioClip
        // =========================
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
            
            int sampleRate  = (minFreq == 0 && maxFreq == 0)
                                ? AudioSettings.outputSampleRate
                                : Mathf.Clamp(44100, minFreq == 0 ? 8000 : minFreq, maxFreq == 0 ? 48000 : maxFreq);

            bIsFinished     = false;
            coroutine       = MonoBehaviourHelper.StartCoroutine(RecordFullClipCoroutine(selectedDevice, bufferSec, sampleRate, finishAction));
        }

        static private IEnumerator RecordFullClipCoroutine(string selectedDevice, int bufferSec, int sampleRate, Action<AudioClip> finishAction)
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
        // =========================
        // 二、僅串流 -> 回呼固定大小的 PCM 區塊
        // =========================
        public static void StartRecordingStreaming(
            int idx = 0,
            Action<float[], int, int> onStreamChunk = null,
            int bufferSec = 10,
            int streamMinSamplesPerChannel = 2048
        )
        {
            string[] devices = Microphone.devices;
            if (devices == null || devices.Length == 0 || idx < 0 || idx >= devices.Length)
            {
                Debug.LogWarning("未檢測到麥克風設備或 index 超出範圍。");
                return;
            }

            if (onStreamChunk == null)
            {
                Debug.LogWarning("StartRecordingStreaming：onStreamChunk 不能為 null。");
                return;
            }

            string selectedDevice = devices[idx];

            if (coroutine != null)
            {
                coroutine.StopCoroutine();
                coroutine = null;
            }

            // 選擇合適取樣率：若裝置不回報（0,0），就用當前輸出取樣率
            Microphone.GetDeviceCaps(selectedDevice, out int minFreq, out int maxFreq);
            int sampleRate = (minFreq == 0 && maxFreq == 0)
                                ? AudioSettings.outputSampleRate
                                : Mathf.Clamp(44100, minFreq == 0 ? 8000 : minFreq, maxFreq == 0 ? 48000 : maxFreq);

            bIsFinished = false;
            coroutine   = MonoBehaviourHelper.StartCoroutine(
                StreamChunksCoroutine(selectedDevice, bufferSec, sampleRate, onStreamChunk, streamMinSamplesPerChannel)
            );
        }

        static private IEnumerator StreamChunksCoroutine(
            string selectedDevice,
            int bufferSec,
            int sampleRate,
            Action<float[], int, int> onStreamChunk,
            int streamMinSamplesPerChannel
        )
        {
            AudioClip micClip = Microphone.Start(selectedDevice, true, bufferSec, sampleRate);

            // 等待裝置就緒（位置 > 0）
            const double startTimeout   = 2.0;
            double startTime            = AudioSettings.dspTime;

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

            int channels        = micClip.channels;
            int clipSamples     = micClip.samples; // 單聲道樣本數

            var streamAccum     = new List<float>(streamMinSamplesPerChannel * channels * 2);
            int prevPos         = 0;

            while (!bIsFinished)
            {
                int curPos = Microphone.GetPosition(selectedDevice);
                if (curPos < 0) curPos = 0;

                if (curPos != prevPos)
                {
                    if (curPos > prevPos)
                    {
                        AppendStream(micClip, prevPos, curPos - prevPos, channels, streamAccum,
                                     onStreamChunk, channels, sampleRate, streamMinSamplesPerChannel);
                    }
                    else
                    {
                        // wrap：先尾巴，再頭
                        AppendStream(micClip, prevPos, clipSamples - prevPos, channels, streamAccum,
                                     onStreamChunk, channels, sampleRate, streamMinSamplesPerChannel);
                        if (curPos > 0)
                        {
                            AppendStream(micClip, 0, curPos, channels, streamAccum,
                                         onStreamChunk, channels, sampleRate, streamMinSamplesPerChannel);
                        }
                    }
                    prevPos = curPos;
                }

                yield return null;
            }

            // 停止前把殘留也吐一次（避免遺漏）
            if (streamAccum.Count > 0)
            {
                var tail = streamAccum.ToArray();
                streamAccum.Clear();
                onStreamChunk?.Invoke(tail, channels, sampleRate);
            }

            Microphone.End(selectedDevice);
        }

        static private void AppendStream(
            AudioClip micClip,
            int startSamplePerCh,
            int sampleCountPerCh,
            int channels,
            List<float> streamAccum,
            Action<float[], int, int> onStreamChunk,
            int outChannels,
            int outSampleRate,
            int streamMinSamplesPerChannel
        )
        {
            if (sampleCountPerCh <= 0) return;

            int floatsNeeded    = sampleCountPerCh * channels;
            var temp            = new float[floatsNeeded];
            micClip.GetData(temp, startSamplePerCh);

            streamAccum.AddRange(temp);

            int thresholdFloats = streamMinSamplesPerChannel * channels;

            // 可能一次就超過多個門檻，逐段吐出
            while (streamAccum.Count >= thresholdFloats)
            {
                var chunk = new float[thresholdFloats];
                streamAccum.CopyTo(0, chunk, 0, thresholdFloats);
                streamAccum.RemoveRange(0, thresholdFloats);
                onStreamChunk?.Invoke(chunk, outChannels, outSampleRate);
            }
        }

        /// <summary>
        /// 將 micClip 中 [start, count)（單位：樣本/每聲道）複製到 buffer（輸出為交錯格式）
        /// </summary>
        static private void AppendRange(AudioClip micClip, int startSample, int sampleCount, int channels, List<float> outBuffer)
        {
            if (sampleCount <= 0) return;

            int floatsNeeded    = sampleCount * channels;
            var temp            = new float[floatsNeeded];
            micClip.GetData(temp, startSample);
            outBuffer.AddRange(temp);
        }

        static public void EndRecording()
        {
            bIsFinished = true;
        }

        // ========== 工具函式 ==========
        /*************************************************************************
         * 把多聲道（例如立體聲 stereo：2 channels）的音訊資料，轉成單聲道（mono）
         * ***********************************************************************/
        static public float[] DownmixToMono(float[] interleaved, int channels)
        {
            int frames  = interleaved.Length / channels;
            var mono    = new float[frames];
            for (int f = 0; f < frames; f++)
            {
                float s     = 0f;
                int baseIdx = f * channels;
                for (int c = 0; c < channels; c++)
                    s += interleaved[baseIdx + c];
                mono[f] = s / channels; // 平均
            }
            return mono;
        }

        /*************************************************************************
         * 把音訊資料從原取樣率（inRate）轉換成新的取樣率（outRate），
         * 例如從 48000Hz 轉到 16000Hz。
         * ***********************************************************************/
        static public float[] ResampleLinear(float[] src, int inRate, int outRate)
        {
            if (inRate <= 0 || outRate <= 0 || src.Length == 0) return Array.Empty<float>();
            if (inRate == outRate) return (float[])src.Clone();

            double ratio    = (double)outRate / inRate;
            int outLen      = Mathf.Max(1, Mathf.RoundToInt((float)(src.Length * ratio)));
            var dst         = new float[outLen];

            double pos = 0.0;
            for (int i = 0; i < outLen; i++)
            {
                double idx  = pos;
                int i0      = (int)idx;
                int i1      = Mathf.Min(i0 + 1, src.Length - 1);
                float t     = (float)(idx - i0);
                dst[i]      = Mathf.Lerp(src[i0], src[i1], t);
                pos         += 1.0 / ratio; // 反比前進（可等效為 pos += inRate / (double)outRate）
            }
            return dst;
        }
    }
}
