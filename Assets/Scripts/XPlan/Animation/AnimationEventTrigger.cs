using UnityEngine;
using UnityEngine.Events;

namespace XPlan.Anim
{
	public class AnimationEventTrigger : MonoBehaviour
	{
		public UnityEvent OnAnimationEnd;

		public void AnimationEnd()
		{
			OnAnimationEnd?.Invoke();
		}
	}
}