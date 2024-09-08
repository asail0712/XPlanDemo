using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using XPlan.DebugMode;
using XPlan.Extensions;

namespace XPlan.Anim
{
    public class AnimInfo
	{
		public int idx;
        public AnimationClip animclip;
        public Animator animator;        
        public GameObject animGO;
        public float duration;

        private AnimatorEventReceiver receiver;

        public AnimInfo(int idx, Animator animator, Action<int> finishAction)
	    {
            /***********************************
             * 初始化
             * *********************************/
            this.idx            = idx;
            this.animclip       = animator.GetClip();
            this.animator       = animator;
            this.animGO         = animator.transform.parent.gameObject;
            this.duration       = animclip == null? 0f : animclip.length;
            this.receiver       = animator.gameObject.AddComponent<AnimatorEventReceiver>();

            receiver.onFinish += (dummy) =>
            {
                animGO.SetActive(false);

                finishAction?.Invoke(idx);
            };
        }

        public void PlayAnim(float ratio)
		{
            animGO.SetActive(true);
            animator.Play(animclip.name, 0, ratio);
            animator.speed = 1f;
        }

        public void StopAnim()
        {
            animGO.SetActive(false);
            animator.speed = 1f;
        }

        public void PauseAnim()
        {
            animGO.SetActive(true);
            animator.speed = 0f;
        }

        public void ResumeAnim()
        {
            animGO.SetActive(true);
            animator.speed = 1f;
        }
    }

    [Serializable]
    public class AnimAlternate
    {
        [SerializeField] public int from;
        [SerializeField] public Animator animator;
        [SerializeField] public string triggerID;
    }

    public class AnimSequeneController : MonoBehaviour
    {
        [SerializeField] private Animator[] animatorArr;
        [SerializeField] private AnimAlternate[] animAlternateList;

        public Action<float> onProgressAction;
        public Action<int> onEndAction;

        private List<AnimInfo> animInfoList;
        private List<AnimInfo> infoAlternateList;
        private Coroutine progressCoroutine;

        private Dictionary<int, string> alternateDict;

        /***********************************
         * 初始化
         * *********************************/
        void Awake()
        {
            // 主線功能初始化
            animInfoList        = new List<AnimInfo>();
            infoAlternateList   = new List<AnimInfo>();
            progressCoroutine   = null;

            // 候補功能初始化
            alternateDict         = new Dictionary<int, string>();

            InitialAnimInfo();
        }

        private void InitialAnimInfo()
		{
            float totalTime = 0;

            for (int i = 0; i < animatorArr.Length; ++i)
            {
                // 先開啟gameobject讓後續流程能夠讀取animator
                Animator animator = animatorArr[i];
                animator.transform.parent.gameObject.SetActive(true);
            }

            for (int i = 0; i < animAlternateList.Length; ++i)
            {
                // 先開啟gameobject讓後續流程能夠讀取animator
                AnimAlternate animAlternate = animAlternateList[i];
                animAlternate.animator.transform.parent.gameObject.SetActive(true);
            }

            // 設定default的 animInfo            
            for (int i = 0; i < animatorArr.Length; ++i)
            {
                AnimInfo animInfo   = new AnimInfo(i, animatorArr[i], OnAnimEnd);
                totalTime           += animInfo.duration;
                animInfo.StopAnim();

                animInfoList.Add(animInfo);
            }

            // 設定分支的 animInfo
            for (int i = 0; i < animAlternateList.Length; ++i)
            {
                int animIdx         = animAlternateList[i].from;
                AnimInfo animInfo   = new AnimInfo(animAlternateList[i].from, animAlternateList[i].animator, OnAnimEnd);

                infoAlternateList.Add(animInfo);
                animInfo.StopAnim();
            }
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

            AnimInfo animInfo = FindAnimInfo(animIdx);

            if (animInfo == null)
            {
                return;
            }

            StopAnim();
            progressCoroutine = StartCoroutine(ProgressBoardcast());

            animInfo.PlayAnim(0f);
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

            animInfo.PlayAnim(playTime / animInfo.duration);
        }

        public void PlayAnimByTime(float playTime)
        {
            float playRatio = playTime / GetTotalTime();
            PlayAnim(playRatio);
        }

        public void StopAnim()
        {
            // coroutine停止
            if(progressCoroutine != null)
			{
                StopCoroutine(progressCoroutine);

                progressCoroutine = null;
            }

            // 主線停止
            for (int i = 0; i < animInfoList.Count; ++i)
            {
                animInfoList[i].StopAnim();
            }

            // 分支停止
            for (int i = 0; i < infoAlternateList.Count; ++i)
            {
                infoAlternateList[i].StopAnim();
            }
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

            animInfo.PauseAnim();
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

            // 從Pause到Resume時 有機會會有不同的Trigger
            // 因此撥放的 anim也會不一樣， 所以用StopAnim昨重製
            StopAnim();

            animInfo.ResumeAnim();
        }

        /***********************************
        * Trigger
        * *********************************/
        public void AddTrigger(string triggerID)
		{
            int alternateFrom = FindAlternateFrom(triggerID);

            if (alternateFrom == -1)
            {
                LogSystem.Record($"沒有 {triggerID} 這個 Trigger ID", LogType.Warning);

                return;
            }

            // 更換Dictionary的資料
            if(alternateDict.ContainsKey(alternateFrom))
			{
                alternateDict[alternateFrom] = triggerID;
            }
            else
            { 
                alternateDict.Add(alternateFrom, triggerID);
            }
        }

        public void RemoveTrigger(string triggerID)
        {
            int branchFrom = FindAlternateFrom(triggerID);

            if (branchFrom == -1)
            {
                LogSystem.Record($"沒有 {triggerID} 這個 Trigger ID", LogType.Warning);

                return;
            }

            alternateDict.Remove(branchFrom);
        }

        public void ClearTrigger()
        {
            alternateDict.Clear();
        }

        private int FindAlternateFrom(string triggerID)
		{
            int alternateFrom = -1;

            for (int i = 0; i < animAlternateList.Length; ++i)
            {
                AnimAlternate animAlternate = animAlternateList[i];
                if (animAlternate.triggerID == triggerID)
                {
                    alternateFrom = animAlternate.from;
                    break;
                }
            }

            return alternateFrom;
        }

        /***********************************
        * 其他 public
        * *********************************/
        public bool IsPlaying()
        {
            // animInfoList 判斷是否有在Play
            for (int i = 0; i < animInfoList.Count; ++i)
            {
                if(FindAnimInfo(i).animator.IsPlay())
				{
                    return true;
				}
            }
            
            return false;
        }

        public float GetPlayRatio()
		{
            AnimInfo animInfo = null;

            for (int i = 0; i < animInfoList.Count; ++i)
            {    
                if (animInfoList[i].animGO.activeSelf)
                {
                    animInfo = animInfoList[i];
                }
            }

            if(animInfo == null)
			{
                for (int i = 0; i < infoAlternateList.Count; ++i)
                {
                    if (infoAlternateList[i].animGO.activeSelf)
                    {
                        animInfo = infoAlternateList[i];
                    }
                }
            }

            // 沒有一個GameObject有顯示，表示播完了
            if (animInfo == null)
            {
                return 0f;
            }

            float animStartTime     = GetAnimStartTime(animInfo.idx);
            float animCurrPlayTime  = animInfo.animator.GetPlayProgress() * animInfo.duration;
            float currRatio         = (animStartTime + animCurrPlayTime) / GetTotalTime();

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

        public float GetTotalTime()
		{
            float totalTime = 0f;

            for(int i = 0; i < animInfoList.Count; ++i)
			{
                AnimInfo animInfo   = FindAnimInfo(i);
                totalTime           += animInfo.duration;
            }

            return totalTime;
		}

        /***********************************
        * 其他 private
        * *********************************/
        private AnimInfo FindAnimInfo(float playRatio, ref float animTime)
		{
            if(playRatio < 0f || playRatio > GetTotalTime())
			{
                return null;
			}

            float currTime  = GetTotalTime() * playRatio;
            int i           = 0;

            for(i = 0; i < animInfoList.Count - 1; ++i)
			{
                float nextAnimStartTime = GetAnimStartTime(i + 1);

                if (nextAnimStartTime > currTime)
				{
                    break;
				}
			}

            AnimInfo animInfo   = FindAnimInfo(i);
            animTime            = currTime - GetAnimStartTime(i);

            return animInfo;
        }

        private void OnAnimEnd(int animIdx)
		{
            // 撥放分支
            AnimInfo nextAnimInfo = FindAnimInfo(animIdx + 1);

            if(nextAnimInfo == null)
			{
                //Debug.Log($"Anim{animIdx} is Over， All Anim Over");

                // 表示撥放結束
                return;
            }

            //Debug.Log($"Anim{animIdx} is Over， Now Playing {nextIdx}");

            nextAnimInfo.PlayAnim(0f);

            onEndAction?.Invoke(animIdx);
        }

        /***********************************
        * 分支處理
        * *********************************/
        public float GetAnimStartTime(int idx)
        {
            float accumulationTime = 0f;

            if (idx == 0)
			{
                return accumulationTime;
			}

            for (int i = 0; i < idx; ++i)
            {
                AnimInfo info = FindAnimInfo(i);

                accumulationTime += info.duration;
            }

            return accumulationTime;
        }

        private AnimInfo FindAnimInfo(int infoIdx)
		{           
            if (alternateDict.ContainsKey(infoIdx))
            {
                // 透過trigger ID尋找候補的Anim
                string triggerID    = alternateDict[infoIdx];
                int alternateIdx    = Array.FindIndex<AnimAlternate>(animAlternateList, (E04) =>
                {
                    return E04.triggerID == triggerID;
                });

                if (alternateIdx == -1)
                {
                    // 分支設定有異常，所以傳回原先的Anim                    
                    return FindAnimFromDefault(infoIdx);
                }

                AnimAlternate animAlternate = animAlternateList[alternateIdx];

                // 透過Anim Branch 尋找對應的 AnimInfo
                int branchInfoIdx = infoAlternateList.FindIndex((E04) =>
                {
                    return E04.animator == animAlternate.animator;
                });

                if (!infoAlternateList.IsValidIndex<AnimInfo>(branchInfoIdx))
                {
                    // 分支設定有異常，所以傳回原先的Anim                    
                    return FindAnimFromDefault(infoIdx);
                }

                return infoAlternateList[branchInfoIdx];
            }
            else
            {
                // 沒有對應的候補，所以傳回原先的Anim
                return FindAnimFromDefault(infoIdx);
            }
        }

        private AnimInfo FindAnimFromDefault(int infoIdx)
		{
            if (!animInfoList.IsValidIndex<AnimInfo>(infoIdx))
            {
                return null;
            }

            return animInfoList[infoIdx];
        }
    }
}
