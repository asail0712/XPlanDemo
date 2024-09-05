using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Extensions;

namespace XPlan.Anim
{
    public class AnimInfo
	{
		public int idx;
        public float startTime;
        public AnimationClip animclip;
        public Animator animator;        
        public GameObject animGO;
        public float duration;
        public Action<int> finishAction;

        private AnimatorEventReceiver receiver;

        public AnimInfo(int idx, Animator animator, float startTime)
	    {
            /***********************************
             * 初始化
             * *********************************/
            this.idx        = idx;
            this.startTime  = startTime;
            this.animclip   = animator.GetClip();
            this.animator   = animator;
            this.animGO     = animator.transform.parent.gameObject;
            this.duration   = animclip == null? 0f : animclip.length;
            this.receiver   = animator.gameObject.AddComponent<AnimatorEventReceiver>();

            receiver.onFinish += (dummy) =>
            {
                animGO.SetActive(false);

                finishAction?.Invoke(idx);
            };
        }
    }

    public class AnimSequeneController : MonoBehaviour
    {
        [SerializeField] private Animator[] animatorArr;

        public Action<float> onProgressAction;

        private List<AnimInfo> animInfoList;
        private float totalTime;
        private Coroutine progressCoroutine;

        /***********************************
         * 初始化
         * *********************************/
        void Awake()
        {
            animInfoList        = new List<AnimInfo>();
            totalTime           = 0f;
            progressCoroutine   = null;

            InitialAnimInfo();
        }

        private void InitialAnimInfo()
		{
            for (int i = 0; i < animatorArr.Length; ++i)
            {
                // 先開啟gameobject讓後續流程能夠讀取animator
                Animator animator = animatorArr[i];
                animator.transform.parent.gameObject.SetActive(true);
            }

            // 計算總時間長度
            for (int i = 0; i < animatorArr.Length; ++i)
            {
                AnimInfo animInfo       = new AnimInfo(i, animatorArr[i], totalTime);
                animInfo.finishAction   = OnAnimEnd;
                totalTime               += animInfo.duration;
                animInfoList.Add(animInfo);

                animatorArr[i].transform.parent.gameObject.SetActive(false);
            }

            //Debug.Log($"Total Time : {totalTime}");
        }

        private IEnumerator ProgressBoardcast()
		{
            while(true)
			{
                yield return new WaitForSeconds(0.1f);

                if (!IsPlaying())
				{
                    continue;
				}

                float currRatio = GetPlayRatio();

                onProgressAction?.Invoke(currRatio);
			}
		}

        /***********************************
         * 撥放
         * *********************************/
        public void PlayAnim(int animIdx = 0)
		{
            if(!animInfoList.IsValidIndex<AnimInfo>(animIdx))
			{
                return;
			}

            StopAnim();

            progressCoroutine = StartCoroutine(ProgressBoardcast());
            AnimInfo animInfo = animInfoList[animIdx];

            animInfo.animGO.SetActive(true);
            animInfo.animator.Play(animInfo.animclip.name, 0, 0f);
            animInfo.animator.speed = 1f;
        }

        public void PlayAnim(float playRatio)
        {
            float playTime      = 0f;
            AnimInfo animInfo   = FindAnimInfo(playRatio, ref playTime);

            if (animInfo == null)
            {
                return;
            }

            StopAnim();

            progressCoroutine   = StartCoroutine(ProgressBoardcast());
            float ratio         = playTime / animInfo.duration;

            animInfo.animGO.SetActive(true);
            animInfo.animator.Play(animInfo.animclip.name, 0, ratio);
            animInfo.animator.speed = 1f;
        }

        public void StopAnim()
        {
            float dummy         = 0f;
            float currRatio     = GetPlayRatio();
            AnimInfo animInfo   = FindAnimInfo(currRatio, ref dummy);

            if (animInfo == null)
            {
                return;
            }

            if(progressCoroutine != null)
			{
                StopCoroutine(progressCoroutine);

                progressCoroutine = null;
            }

            animInfo.animGO.SetActive(false);
        }

        public void PauseAnim()
		{
            float dummy         = 0f;
            float currRatio     = GetPlayRatio();
            AnimInfo animInfo   = FindAnimInfo(currRatio, ref dummy);

            if (animInfo == null)
            {
                return;
            }

            animInfo.animator.speed = 0f;
        }

        public void ResumeAnim()
        {
            float dummy         = 0f;
            float currRatio     = GetPlayRatio();
            AnimInfo animInfo   = FindAnimInfo(currRatio, ref dummy);

            if (animInfo == null)
            {
                return;
            }

            animInfo.animator.speed = 1f;
        }

        /***********************************
        * 其他 public
        * *********************************/
        public bool IsPlaying()
        {
            // 計算總時間長度
            for (int i = 0; i < animInfoList.Count; ++i)
            {
                if(animInfoList[i].animator.IsPlay())
				{
                    return true;
				}
            }

            return false;
        }

        public float GetPlayRatio()
		{
            int i = -1;

            for (i = 0; i < animInfoList.Count; ++i)
            {
                if (animInfoList[i].animGO.activeSelf)
                {
                    break;
                }
            }

            // 沒有一個GameObject有顯示，表示播完了
            if(!animInfoList.IsValidIndex<AnimInfo>(i))
			{
                return 0f;
			}

            AnimInfo animInfo       = animInfoList[i];
            float animStartTime     = animInfo.startTime;
            float animCurrPlayTime  = animInfo.animator.GetPlayProgress() * animInfo.duration;
            float currRatio         = (animStartTime + animCurrPlayTime) / totalTime;

            return currRatio;
        }

        public void SetPlayRatio(float playRatio)
		{
            float playTime      = 0f;
            AnimInfo animInfo   = FindAnimInfo(playRatio, ref playTime);

            if (animInfo == null)
            {
                return;
            }

            StopAnim();

            float ratio = playTime / animInfo.duration;

            animInfo.animGO.SetActive(true);
            animInfo.animator.Play(animInfo.animclip.name, 0, ratio);
            animInfo.animator.speed = 0f;
        }

        /***********************************
        * 其他 private
        * *********************************/
        private AnimInfo FindAnimInfo(float playRatio, ref float animTime)
		{
            if(playRatio < 0f || playRatio > totalTime)
			{
                return null;
			}

            float currTime  = totalTime * playRatio;
            int i           = 0;

            for(i = 0; i < animInfoList.Count - 1; ++i)
			{
                if(animInfoList[i + 1].startTime > currTime)
				{
                    break;
				}
			}

            animTime = currTime - animInfoList[i].startTime;

            return animInfoList[i];
        }

        private void OnAnimEnd(int animIdx)
		{
            if(animatorArr.Length - 1 == animIdx)
			{
                //Debug.Log($"Anim{animIdx} is Over， All Anim Over");

                return;
			}

            //Debug.Log($"Anim{animIdx} is Over， Now Playing {animIdx + 1}");

            PlayAnim(animIdx + 1);
        }
    }
}
