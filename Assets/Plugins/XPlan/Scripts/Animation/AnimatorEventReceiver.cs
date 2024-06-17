using System;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Anim
{ 
    public class AnimatorEventReceiver : MonoBehaviour
    {
		private static readonly string FuncName_AnimStart	= "OnAnimStart";
		private static readonly string FuncName_AnimEnd		= "OnAnimEnd";

		public Action<string, float> onStart;
		public Action<string> onFinish;
		public Action<string, float, string> onPlaying;

		[HideInInspector]
		public Animator animator;

		private void Awake()
		{
			animator = GetComponent<Animator>();

			if(animator == null)
			{
				return;
			}

			AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
			foreach (AnimationClip clip in animationClips)
			{
				bool bNeedToAddStart	= true;
				bool bNeedToAddFinish	= true;

				foreach (AnimationEvent animEvent in clip.events)
				{
					// 由於AnimationEvent無法刪除，因此只能避免重複添加
					if (animEvent.functionName == FuncName_AnimStart)
					{
						bNeedToAddStart = false;
						continue;
					}

					if (animEvent.functionName == FuncName_AnimEnd)
					{
						bNeedToAddFinish = false;
						continue;
					}
				}

				if (bNeedToAddStart)
				{
					AnimationEvent animStartEvent	= new AnimationEvent();
					animStartEvent.functionName		= FuncName_AnimStart;   // 替換成您的回調函數名稱				
					animStartEvent.time				= 0f;                   // 在動畫結束時觸發回調函數
					clip.AddEvent(animStartEvent);
				}

				if (bNeedToAddFinish)
				{
					AnimationEvent animEndEvent		= new AnimationEvent();
					animEndEvent.functionName		= FuncName_AnimEnd;     // 替換成您的回調函數名稱				
					animEndEvent.time				= clip.length;          // 在動畫結束時觸發回調函數
					clip.AddEvent(animEndEvent);
				}				
			}
		}

		public void OnAnimStart(AnimationEvent animationEvent)
		{
			onStart?.Invoke(animationEvent.animatorClipInfo.clip.name, animationEvent.animatorClipInfo.clip.length);
		}

		public void OnAnimEnd(AnimationEvent animationEvent)
	    {
			onFinish?.Invoke(animationEvent.animatorClipInfo.clip.name);
		}

		public void OnAnimPlaying(AnimationEvent animationEvent)
		{
			onPlaying?.Invoke(animationEvent.animatorClipInfo.clip.name, animationEvent.time, animationEvent.stringParameter);
		}

		//private void OnDestroy()
		//{
		//	Animator animator = GetComponent<Animator>();

		//	if (animator == null)
		//	{
		//		return;
		//	}

		//	AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
		//	foreach (AnimationClip clip in animationClips)
		//	{
		//		Array.

		//		Debug.Log(clip.events);
		//	}
		//}
	}
}
