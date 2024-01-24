using UnityEngine;

namespace XPlan.Gesture
{
	public enum GesterType
	{
		None,
		Up,
		Down,
		Left,
		Right
	}

	public class GestureAction : MonoBehaviour
    {
		[SerializeField]
		private float triggerDis = 200f;

		[SerializeField]
		private GesterType triggerType = GesterType.None;

		public bool CanTrigger(Vector2 offset)
		{
            if(Vector2.Distance(offset, Vector2.zero) < triggerDis)
			{
                return false;
			}

            bool bIsTrigger = false;

            switch (triggerType)
            {
                case GesterType.Up:
                    if (offset.y > 0)
                    {
                        bIsTrigger = true;
                    }
                    break;
                case GesterType.Down:
                    if (offset.y < 0)
                    {
                        bIsTrigger = true;
                    }
                    break;
                case GesterType.Left:
                    if (offset.x > 0)
                    {
                        bIsTrigger = true;
                    }
                    break;
                case GesterType.Right:
                    if (offset.x < 0)
                    {
                        bIsTrigger = true;
                    }
                    break;
                case GesterType.None:
                    break;
            }

            return bIsTrigger;
        }

		public void TriggerAction()
		{
			OnTriggerAction();
		}

		protected virtual void OnTriggerAction()
		{
			// for override
		}
	}
}
