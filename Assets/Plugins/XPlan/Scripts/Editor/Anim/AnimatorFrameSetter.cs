using UnityEditor;
using UnityEngine;

namespace XPlan.Editors
{
    [CustomEditor(typeof(Animator))]
    public class AnimatorFrameSetter : Editor
    {
        private int frame       = 1;
        private float frameRate = 60f; // 根據您的動畫幀率調整此值

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Animator animator   = (Animator)target;

            frame               = EditorGUILayout.IntField("Frame", frame);
            frameRate           = EditorGUILayout.FloatField("Frame Rate", frameRate);

            if (GUILayout.Button("Set Frame"))
            {
                SetAnimatorFrame(animator, frame, frameRate);
            }
        }

        private void SetAnimatorFrame(Animator animator, int frame, float frameRate)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning("Animator 或 Animator Controller 未設定。");
                return;
            }

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime        = frame / (frameRate * stateInfo.length);
            animator.Play(stateInfo.fullPathHash, -1, normalizedTime);
            animator.Update(0);
        }
    }
}