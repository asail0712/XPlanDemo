using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Anim
{
    public static class AnimUtility
    {
        static public AnimationClip GetClip(this Animator animator, int clipIdx = 0, int layerIdx = 0)
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(layerIdx);

            if (clipInfo.Length > 0)
            {
                return clipInfo[clipIdx].clip;
            }
            else
            {
                Debug.LogWarning("沒有可以撥放得Clip");
                return null;
            }
        }

        static public bool IsPlay(this Animator animator, int layerIdx = 0)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(layerIdx);

            return info.fullPathHash != 0 && info.length > 0f && info.speed > 0f;
        }

        static public float GetPlayProgress(this Animator animator, int layerIdx = 0)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(layerIdx);

            // % 1是因為如果動畫是循環的，則這個值可能大於 1.0
            // 使用 % 1 來忽略超過一次循環的部分
            return info.normalizedTime % 1; 
        }
    }
}
