using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.DebugMode;
using XPlan.Recycle;
using XPlan.Utility;

namespace XPlan.Audio
{
    public class XAudioSource : IPoolable
    {
        private AudioSource audioSource;
        private MonoBehaviourHelper.MonoBehavourInstance playCoroutine;
        private MonoBehaviourHelper.MonoBehavourInstance fadeInCoroutine;
        private MonoBehaviourHelper.MonoBehavourInstance stopCoroutine;
        private Action finishAction;
        private SoundInfo _soundInfo;

        public SoundInfo soundInfo
        {
            get
            {
                return _soundInfo;
            }
            set
            {
                _soundInfo = value;
            }
        }
        public float volume
        {
			get
			{
				return audioSource.volume;
			}
			set
			{
				audioSource.volume = value;
			}
		}

        public void InitialSource()
		{
            if (audioSource != null)
            {
                return;
            }

            GameObject audioRoot    = AudioSystem.GetGameObject();
            audioSource             = audioRoot.AddComponent<AudioSource>();
        }
        public void ReleaseSource()
        {
            GameObject.DestroyImmediate(audioSource);
        }

        public void Play(float fadeInTime, Action<string> finishAction = null)
		{
            // 有在撥放聲音的話得先停下來
            if (playCoroutine != null)
            {
                MonoBehaviourHelper.StartCoroutine(FadeInOut(fadeInTime, finishAction));
            }
            else
			{
                playCoroutine = MonoBehaviourHelper.StartCoroutine(Play_Internal(fadeInTime, finishAction));
            }
        }

        private IEnumerator FadeInOut(float fadeInTime, Action<string> finishAction = null)
		{
            if (stopCoroutine == null)
			{
                stopCoroutine = MonoBehaviourHelper.StartCoroutine(Stop_Internal(fadeInTime / 2f));
            }

            yield return new WaitUntil(() => playCoroutine == null);

            playCoroutine = MonoBehaviourHelper.StartCoroutine(Play_Internal(fadeInTime / 2f, finishAction));
        }

        private IEnumerator Play_Internal(float fadeInTime, Action<string> finishAction = null)
		{
            if(soundInfo.clipList == null || soundInfo.clipList.Count == 0)
			{
                yield break;
			}

            yield return new WaitForEndOfFrame();

            int playIndex   = 0;

            // 設定fade in
            fadeInCoroutine = MonoBehaviourHelper.StartCoroutine(FadeIn(fadeInTime / 2f));

            while (playIndex < soundInfo.clipList.Count)
            {
                // 設定SoundInfo資料
                audioSource.clip = soundInfo.clipList[playIndex];
                audioSource.loop = false;
                audioSource.Play();

                // 音樂停止 呼叫finishAction
                yield return new WaitUntil(() => !audioSource.isPlaying);

                ++playIndex;

                if (soundInfo.bLoop)
				{
                    playIndex %= soundInfo.clipList.Count;
                }
            }

            finishAction?.Invoke(soundInfo.clipName);
        }

        private IEnumerator FadeIn(float fadeInTime)
		{
            // fade in 設定
            if (fadeInTime == 0f)
            {
                audioSource.volume = soundInfo.volume;
            }
            else
            {
                float targetVolume  = soundInfo.volume;
                float startTime     = Time.time;

                while (Time.time < startTime + fadeInTime)
                {
                    yield return null;
                    audioSource.volume = Mathf.Lerp(0f, targetVolume, (Time.time - startTime) / fadeInTime);
                }

                audioSource.volume = targetVolume;
            }
        }

        public void Stop(float fadeOutTime)
        {
            // 確定是否再撥放
            if (!IsPlaying())
            {
                return;
            }

            // 確認是否在停止中
            if (stopCoroutine != null)
			{
                return;
			}

            if (fadeOutTime > 0f)
            {
                stopCoroutine = MonoBehaviourHelper.StartCoroutine(Stop_Internal(fadeOutTime));
            }
            else
			{
                StopImmediately();
            }
        }

        private IEnumerator Stop_Internal(float fadeOutTime)
		{
            float startVolume   = audioSource.volume;
            float startTime     = Time.time;

            while (Time.time < startTime + fadeOutTime)
            {
                audioSource.volume = Mathf.Lerp(startVolume, 0f, (Time.time - startTime) / fadeOutTime);
                yield return null;
            }

            audioSource.volume = 0f;
            
            StopImmediately();

            stopCoroutine = null;
        }

        private void StopImmediately()
        {
            audioSource.Stop();

            if(playCoroutine != null)
            {
                playCoroutine.StopCoroutine();
                playCoroutine = null;
            }

            if (fadeInCoroutine != null)
            {
                fadeInCoroutine.StopCoroutine();
                fadeInCoroutine = null;
            }

            finishAction?.Invoke();
        }

        public bool IsPlaying()
		{
            return audioSource.isPlaying;
		}

        public void Pause()
        {
            audioSource.Pause();
        }

        public void Resume()
        {
            audioSource.UnPause();
        }

        /***************************************
         * 實作IPoolable
         * ************************************/

        public void InitialPoolable()
        {
            InitialSource();
        }

        public void ReleasePoolable()
        {
            ReleaseSource();
        }

        public void OnSpawn()
        {
        }

        public void OnRecycle()
        {
        }
    }
}