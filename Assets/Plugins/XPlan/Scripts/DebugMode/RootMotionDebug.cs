using UnityEngine;

namespace XPlan.DebugMode
{
    public class RootMotionDebug : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        void Start()
        {
            if (animator == null)
            {
                Debug.LogError("Animator component is missing.");
            }
        }

        void OnAnimatorMove()
        {
            if (animator)
            {
                Debug.Log("OnAnimatorMove called");
                Debug.Log("deltaPosition: " + animator.deltaPosition);
                Debug.Log("deltaRotation: " + animator.deltaRotation);

                // 应用Root Motion到Transform上
                transform.position += animator.deltaPosition;
                transform.rotation *= animator.deltaRotation;
            }
        }
    }
}