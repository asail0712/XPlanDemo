using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Utility;

// 參考資料
// https://blog.csdn.net/Mediary/article/details/118333666

namespace XPlan.audio
{
    // 使用develop build 有機會在手機會有延遲
    // 生成的AudioClip要手動釋放

    public static class MicrophoneTools
    {
        static private bool bIsFinished;
        static private MonoBehaviourHelper.MonoBehavourInstance coroutine;

        static public void StartRecording(int idx = 0, Action<AudioClip> finishAction = null)
        {
            string[] devices = Microphone.devices;
            if (devices.Length == 0 || idx >= devices.Length)
            {
                Debug.LogWarning("未檢測到麥克風設備。");
                return;
            }

            string selectedDevice = devices[idx]; // 選擇第一個可用的設備

            if(coroutine != null)
            {
                coroutine.StopCoroutine();
                coroutine = null;
            }

            coroutine = MonoBehaviourHelper.StartCoroutine(StartRecord_Internal(selectedDevice, finishAction));
        }

        static private IEnumerator StartRecord_Internal(string selectedDevice, Action<AudioClip> finishAction)
        {
            List<float> micDataList = new List<float>();
            AudioClip micClip       = Microphone.Start(selectedDevice, true, 1, AudioSettings.outputSampleRate);

            bIsFinished             = false;
            int length              = micClip.channels * micClip.samples;
            bool bSaveFirstHalf     = true;            
            float[] micDataTemp     = new float[length / 2];
            int micPos;

            while (!bIsFinished)
            {
                if(bSaveFirstHalf)
                {
                    // 保存前半
                    micPos = Microphone.GetPosition(selectedDevice);

                    while (micPos >= 0 && micPos < length * 0.5f)                        
                    {
                        yield return new WaitForSeconds(0.5f);

                        micPos = Microphone.GetPosition(selectedDevice);
                    }

                    micClip.GetData(micDataTemp, 0);
                    micDataList.AddRange(micDataTemp);
                    bSaveFirstHalf = !bSaveFirstHalf;
                }
                else
                {
                    // 保存後半
                    micPos = Microphone.GetPosition(selectedDevice);

                    while (micPos >= length * 0.5f && micPos < length)
                    {
                        yield return new WaitForSeconds(0.5f);

                        micPos = Microphone.GetPosition(selectedDevice);
                    }

                    micClip.GetData(micDataTemp, length / 2);
                    micDataList.AddRange(micDataTemp);
                    bSaveFirstHalf = !bSaveFirstHalf;
                }
            }

            micPos = Microphone.GetPosition(selectedDevice);

            if(micPos <= length / 2)
            {
                micDataTemp = new float[micPos];
                micClip.GetData(micDataTemp, 0);
            }
            else
            {
                micDataTemp = new float[micPos - length / 2];
                micClip.GetData(micDataTemp, length / 2);
            }

            micDataList.AddRange(micDataTemp);
            Microphone.End(selectedDevice);

            AudioClip newAudioClip = AudioClip.Create("Record", micDataList.Count, 1, AudioSettings.outputSampleRate, false);
            newAudioClip.SetData(micDataList.ToArray(), 0);

            finishAction?.Invoke(newAudioClip);
        }

        static public void EndRecording()
        {
            bIsFinished = true;
        }
    }
}
