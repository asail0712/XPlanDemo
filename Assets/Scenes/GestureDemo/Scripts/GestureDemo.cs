using UnityEngine;
using UnityEngine.UI;

using XPlan.DebugMode;
using XPlan.Gesture;

namespace XPlan.Demo.Gesture
{
    public class GestureDemo : MonoBehaviour
    {
        [SerializeField] private Button switchBtn;
        [SerializeField] private Text switchDescTxt;

        [SerializeField] private Text moveTxt;
        [SerializeField] private Text rotateTxt;
        [SerializeField] private Text scaleTxt;

        [SerializeField] private DragToRotate dragToRotate;
        [SerializeField] private DragToMove dragToMove;
        [SerializeField] private TapToPoint tapToPoint;

        private bool bSwitchToMove = false;

		private void Awake()
		{
            switchBtn.onClick.AddListener(() =>
            {
                dragToRotate.enabled    = !dragToRotate.enabled;
                dragToMove.enabled      = !dragToMove.enabled;
                bSwitchToMove           = !bSwitchToMove;

                if (bSwitchToMove)
                {
                    switchDescTxt.text = "Switch To Rotate";
                }
                else
                {
                    switchDescTxt.text = "Switch To Move";
                }
            });

            tapToPoint.finishAction += (go, hitPoint, hitNormal) =>
            {
                if(go == gameObject)
				{
                    DebugDraw.DrawSphere(Color.red, hitPoint, 0.1f, 12, 3f);
                }
            };
        }

		// Update is called once per frame
		void Update()
        {
            moveTxt.text    = $"Pos : {transform.position}";
            rotateTxt.text  = $"Rotate : {transform.rotation.eulerAngles}";
            scaleTxt.text   = $"Scale : {transform.localScale}";
        }
    }
}
