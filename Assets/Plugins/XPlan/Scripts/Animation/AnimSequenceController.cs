using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using XPlan.DebugMode;
using XPlan.Observe;
using XPlan.Extensions;

namespace XPlan.Anim
{
    public class XAnimProgressMsg : MessageBase
	{
        public int animIdx;
        public float progress;
        public bool bIsPause;

        public XAnimProgressMsg(int animIdx, float progress, bool bIsPause)
		{
            this.animIdx    = animIdx;
            this.progress   = progress;
            this.bIsPause   = bIsPause;
        }
    }

    public class XAnimEndMsg : MessageBase
    {
        public int animIdx;

        public XAnimEndMsg(int animIdx)
        {
            this.animIdx = animIdx;
        }
    }

    public class AnimInfo
	{
        public string triggerID;

        public AnimationClip animclip;
        public Animator animator;
        public GameObject animGO;
        public float duration;

        public AnimatorEventReceiver receiver;

        public AnimInfo(Animator animator, Action finishAction)
		{
            this.triggerID  = "";
            this.animclip   = animator.GetClip();
            this.animator   = animator;
            this.animGO     = animator.transform.parent.gameObject;
            this.duration   = animclip == null ? 0f : animclip.length;
            this.receiver   = animator.gameObject.AddComponent<AnimatorEventReceiver>();

            receiver.onFinish += (dummy) =>
            {
                finishAction?.Invoke();
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

        public void PauseAnim(float ratio)
        {
            animGO.SetActive(true);
            animator.Play(animclip.name, 0, ratio);
            animator.speed = 0f;
        }

        public bool IsPlaying()
        {
            return animGO.activeSelf;// && animator.speed != 0f;
        }

        // play與pause的差別為:
        // 進度條有在動且看的到東西為play
        // 進度條沒在動且看的到東西為pause
        public bool IsPause()
        {
            return animGO.activeSelf && animator.speed == 0f;
        }
    }

    public class AnimUnit
	{
		public int idx;
        public Dictionary<string, AnimInfo> animInfoDict;

        private List<string> triggerList;
        private Action<int> finishAction;

        public AnimUnit(int idx, Animator animator, Action<int> finishAction)
	    {
            /***********************************
             * 初始化
             * *********************************/
            this.idx            = idx;
            this.animInfoDict   = new Dictionary<string, AnimInfo>();
            this.triggerList    = new List<string>();
            this.finishAction   = finishAction;

            AnimInfo animInfo   = new AnimInfo(animator, ()=> 
            {
                finishAction?.Invoke(idx);
            });

            animInfo.StopAnim();
            animInfoDict.Add("", animInfo);
        }

        public void PlayAnim(float ratio)
		{
            string triggerID = GetTriggerID();

            foreach(KeyValuePair<string, AnimInfo> kvp in animInfoDict)
			{
                if(kvp.Key == triggerID)
				{
                    animInfoDict[triggerID].PlayAnim(ratio);
                }
                else
				{
                    animInfoDict[kvp.Key].StopAnim();
                }
            }
        }

        public void StopAnim()
        {
            foreach (KeyValuePair<string, AnimInfo> kvp in animInfoDict)
            {
                animInfoDict[kvp.Key].StopAnim();
            }
        }

        public void PauseAnim()
        {
            string triggerID = GetTriggerID();

            foreach (KeyValuePair<string, AnimInfo> kvp in animInfoDict)
            {
                if (kvp.Key == triggerID)
                {
                    animInfoDict[triggerID].PauseAnim();
                }
                else
                {
                    animInfoDict[kvp.Key].StopAnim();
                }
            }
        }

        public void PauseAnim(float ratio)
        {
            string triggerID = GetTriggerID();

            foreach (KeyValuePair<string, AnimInfo> kvp in animInfoDict)
            {
                if (kvp.Key == triggerID)
                {
                    animInfoDict[triggerID].PauseAnim(ratio);
                }
                else
                {
                    animInfoDict[kvp.Key].StopAnim();
                }
            }
        }

        public bool IsPlaying()
        {
            string triggerID = GetTriggerID();

            return animInfoDict[triggerID].IsPlaying();
        }

        public bool IsPause()
        {
            string triggerID = GetTriggerID();

            return animInfoDict[triggerID].IsPause();
        }        

        public float Duration()
		{
            string triggerID = GetTriggerID();

            return animInfoDict[triggerID].duration;
        }

        public float GetPlayProgress()
		{
            string triggerID = GetTriggerID();

            return animInfoDict[triggerID].animator.GetPlayProgress();
        }

        public void AddAlternate(string triggerID, Animator animator)
		{           
            AnimInfo animInfo = new AnimInfo(animator, () =>
            {
                finishAction?.Invoke(idx);
            });

            animInfo.StopAnim();
            animInfoDict[triggerID] = animInfo;
        }

        public void AddTriggerID(string triggerID)
		{
            triggerList.Add(triggerID);
        }

        public void RemoveTriggerID(string triggerID)
        {
            triggerList.Remove(triggerID);
        }

        public void ClearTriggerID()
        {
            triggerList.Clear();
        }

        public void RefreshTrigger()
		{
            // 強制依照當前的trigger來改變anim
            float progress = GetPlayProgress();

            // 刷新只需要刷新有在撥放的Anim
            if(!IsPlaying())
			{
                return;
			}

            if(IsPause())
			{
                PauseAnim(progress);
			}
            else
			{
                PlayAnim(progress);
            }
		}

        private string GetTriggerID()
		{
            for(int i = triggerList.Count - 1; i >= 0; --i)
			{
                string triggerID = triggerList[i];

                if (triggerID != null && animInfoDict.ContainsKey(triggerID))
				{
                    return triggerID;
                }
			}

            return "";
        }
    }

    [Serializable]
    public class AnimAlternate
    {
        [SerializeField] public int from;
        [SerializeField] public Animator animator;
        [SerializeField] public string triggerID;
    }

    public class AnimSequenceController : MonoBehaviour
    {
        [SerializeField] public Animator[] animatorArr;
        [SerializeField] public AnimAlternate[] animAlternateList;
        [SerializeField] private bool bIsLoop = false;

        public Action<int, float, bool> onProgressAction;
        public Action<int> onEndAction;

        private List<AnimUnit> animUnitList;
        private Coroutine progressCoroutine;

        /***********************************
         * 初始化
         * *********************************/
        void Awake()
        {
            // 主線功能初始化
            animUnitList        = new List<AnimUnit>();
            progressCoroutine   = null;

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
                AnimUnit animUnit   = new AnimUnit(i, animatorArr[i], OnAnimEnd);
                totalTime           += animUnit.Duration();
                
                animUnitList.Add(animUnit);
            }

            // 設定分支的 animInfo
            for (int i = 0; i < animAlternateList.Length; ++i)
			{
				AnimUnit animUnit = animUnitList[animAlternateList[i].from];
                animUnit.AddAlternate(animAlternateList[i].triggerID, animAlternateList[i].animator);
            }
        }

        private IEnumerator ProgressBoardcast()
		{
            while(true)
			{
                yield return new WaitForSeconds(0.1f);

                int currIdx     = GetCurrPlayIndex();
                float currRatio = GetPlayRatio();
                bool bIsPause = IsPause();

                onProgressAction?.Invoke(currIdx, currRatio, bIsPause);

                XAnimProgressMsg msg = new XAnimProgressMsg(currIdx, currRatio, bIsPause);
                msg.Send();
            }
		}

        /***********************************
         * 撥放
         * *********************************/
        public void PlayAnim(int animIdx = 0)
		{
            if(!animUnitList.IsValidIndex<AnimUnit>(animIdx))
			{
                return;
			}

            AnimUnit animUnit = animUnitList[animIdx];

            if (animUnit == null)
            {
                return;
            }

            StopAnim();
            progressCoroutine = StartCoroutine(ProgressBoardcast());

            animUnit.PlayAnim(0f);
        }

        public void PlayAnim(float playRatio)
        {
            float playTime      = 0f;
            AnimUnit animUnit   = FindAnimUnit(playRatio, ref playTime);

            if (animUnit == null)
            {
                return;
            }

            StopAnim();
            progressCoroutine   = StartCoroutine(ProgressBoardcast());
            float duration      = animUnit.Duration();
            animUnit.PlayAnim(playTime / duration);
        }

        public void PlayAnimByTime(float playTime)
        {
            float totalTime = GetTotalTime();
            float playRatio = playTime / totalTime;
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
            for (int i = 0; i < animUnitList.Count; ++i)
            {
                animUnitList[i].StopAnim();
            }
        }

        public void PauseAnim(float playRatio)
        {
            float dummy             = 0f;
            float oldPlayRatio      = GetPlayRatio();
            AnimUnit oldAnimUnit    = FindAnimUnit(oldPlayRatio, ref dummy);

            float playTime          = 0f;
            AnimUnit newAnimUnit    = FindAnimUnit(playRatio, ref playTime);

            if (newAnimUnit == null)
            {
                return;
            }

            if(oldAnimUnit.idx != newAnimUnit.idx)
			{
                oldAnimUnit.StopAnim();
            }

            float ratio = playTime / newAnimUnit.Duration();

            newAnimUnit.PauseAnim(ratio);
        }

        public void PauseAnim()
		{
            float dummy         = 0f;
            float currRatio     = GetPlayRatio();
            AnimUnit animUnit   = FindAnimUnit(currRatio, ref dummy);

            if (animUnit == null)
            {
                return;
            }

            animUnit.PauseAnim();
        }

        public void ResumeAnim()
        {
            float playRatio = GetPlayRatio();

            PlayAnim(playRatio);
        }

        /***********************************
        * Trigger
        * *********************************/
        public void AddTrigger(string triggerID)
		{
            foreach(AnimUnit animUnit in animUnitList)
			{
                animUnit.AddTriggerID(triggerID);
            }
        }

        public void RemoveTrigger(string triggerID)
        {
            foreach (AnimUnit animUnit in animUnitList)
            {
                animUnit.RemoveTriggerID(triggerID);
            }
        }

        public void ClearTrigger()
        {
            foreach (AnimUnit animUnit in animUnitList)
            {
                animUnit.ClearTriggerID();
            }            
        }

        public void RefreshTrigger()
		{
            foreach (AnimUnit animUnit in animUnitList)
            {
                animUnit.RefreshTrigger();
            }
        }

        /***********************************
        * 其他 public
        * *********************************/
        public bool IsPlaying()
        {
            // animInfoList 判斷是否有在Play
            for (int i = 0; i < animUnitList.Count; ++i)
            {
                if(animUnitList[i].IsPlaying())
				{
                    return true;
				}
            }
            
            return false;
        }

        public bool IsPause()
        {
            // animInfoList 判斷是否有在Play
            for (int i = 0; i < animUnitList.Count; ++i)
            {
                if (animUnitList[i].IsPause())
                {
                    return true;
                }
            }

            return false;
        }

        public float GetPlayRatio()
		{
            AnimUnit animUnit = null;

            for (int i = 0; i < animUnitList.Count; ++i)
            {    
                if (animUnitList[i].IsPlaying())
                {
                    animUnit = animUnitList[i];
                }
            }

            // 沒有一個GameObject有顯示，表示播完了
            if (animUnit == null)
            {
                return 0f;
            }

            float animStartTime     = GetAnimStartTime(animUnit.idx);
            float animCurrPlayTime  = animUnit.GetPlayProgress() * animUnit.Duration();
            float currRatio         = (animStartTime + animCurrPlayTime) / GetTotalTime();

            return currRatio;
        }

        public void PauseAnimByTime(float playTime)
		{
            PauseAnim(playTime / GetTotalTime());
        }

        public float GetTotalTime()
		{
            float totalTime = 0f;

            for(int i = 0; i < animUnitList.Count; ++i)
			{
                AnimUnit animUnit   = animUnitList[i];
                totalTime           += animUnit.Duration();
            }

            return totalTime;
		}

        /***********************************
        * 其他 private
        * *********************************/
        private int GetCurrPlayIndex()
		{
            for (int i = 0; i < animUnitList.Count; ++i)
            {
                if (animUnitList[i].IsPlaying())
                {
                    return i;
                }
            }

            return -1;
        }

        private AnimUnit FindAnimUnit(float playRatio, ref float animTime)
		{
            if(playRatio < 0f || playRatio > 1.0f)
			{
                return null;
			}

            float currTime  = GetTotalTime() * playRatio;
            int i           = 0;

            for(i = 0; i < animUnitList.Count - 1; ++i)
			{
                float nextAnimStartTime = GetAnimStartTime(i + 1);

                if (nextAnimStartTime > currTime)
				{
                    break;
				}
			}

            AnimUnit animUnit   = animUnitList[i];
            animTime            = currTime - GetAnimStartTime(i);

            return animUnit;
        }

        private void OnAnimEnd(int animIdx)
		{
            AnimUnit prevAnimUnit = animUnitList[animIdx];
            AnimUnit nextAnimUnit = null;

            // 撥放分支
            if(animUnitList.IsValidIndex<AnimUnit>(animIdx + 1))
			{
                nextAnimUnit = animUnitList[animIdx + 1];
            }

            if(nextAnimUnit != null)
			{
                //Debug.Log($"Anim{animIdx} is Over， Now Playing {nextIdx}");
                prevAnimUnit.StopAnim();
                nextAnimUnit.PlayAnim(0f);
            }
            else
			{
                // 表示撥放結束
                //Debug.Log($"Anim{animIdx} is Over， All Anim Over");

                if(bIsLoop)
				{
                    // 停止當前動畫
                    prevAnimUnit.StopAnim();

                    // 從頭開始撥放
                    nextAnimUnit = animUnitList[0];
                    nextAnimUnit.PlayAnim(0f);
                }
                else
				{
                    prevAnimUnit.PauseAnim(0.99f); // 直接設定duration會把影片重置，所以要故意小一點                    
                }
            }

            onEndAction?.Invoke(animIdx);

            XAnimEndMsg msg = new XAnimEndMsg(animIdx);
            msg.Send();
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
                AnimUnit animUnit = animUnitList[i];

                accumulationTime += animUnit.Duration();
            }

            return accumulationTime;
        }
    }
}
