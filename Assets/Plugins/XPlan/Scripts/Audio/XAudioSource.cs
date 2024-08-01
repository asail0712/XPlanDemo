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
        private MonoBehaviourHelper.MonoBehavourInstance playMBIns;
        private Action finishAction;

        public bool loop 
        { 
            get 
            { 
                return audioSource.loop; 
            }
            set 
            { 
                audioSource.loop = value; 
            }
        }
        public AudioClip clip
        {
            get
            {
                return audioSource.clip;
            }
            set
            {
                audioSource.clip = value;
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

        public void Play(Action finishAction = null)
		{
            if(playMBIns != null)
			{
                playMBIns.StopCoroutine();
                playMBIns = null;

                finishAction?.Invoke();
            }

            this.finishAction   = finishAction;
            this.playMBIns      = MonoBehaviourHelper.StartCoroutine(Play_Internal());

            audioSource.Play();
        }

        private IEnumerator Play_Internal()
		{
            // 音樂停止 呼叫finishAction
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => !audioSource.isPlaying);

            finishAction?.Invoke();
        }

        public void Stop()
        {
            audioSource.Stop();

            playMBIns.StopCoroutine();
            playMBIns = null;

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

        public void UnPause()
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