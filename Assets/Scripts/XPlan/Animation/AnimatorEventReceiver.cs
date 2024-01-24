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

		private void Awake()
		{
			Animator animator = GetComponent<Animator>();

			if(animator == null)
			{
				return;
			}

			AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
			foreach (AnimationClip clip in animationClips)
			{
				bool bNeedToAdd = true;

				foreach(AnimationEvent animEvent in clip.events)
				{
					// 由於AnimationEvent無法刪除，因此只能避免重複添加
					if(animEvent.functionName == FuncName_AnimEnd)
					{
						bNeedToAdd = false;
						break;
					}
				}

				if(bNeedToAdd)
				{
					AnimationEvent animStartEvent	= new AnimationEvent();
					animStartEvent.functionName		= FuncName_AnimStart;   // 替換成您的回調函數名稱				
					animStartEvent.time				= 0f;					// 在動畫結束時觸發回調函數
					clip.AddEvent(animStartEvent);

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
