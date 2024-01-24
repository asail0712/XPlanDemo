using System.Collections;
using UnityEngine;

using XPlan.Interface;

namespace XPlan.UI
{
    public class SingleClickFilterStateResetter : MonoBehaviour
    {
        private WaitForEndOfFrame waitForEndOfFrame;

        private IStateValue<bool> isClicked;

        private void Awake()
        {
            waitForEndOfFrame   = new WaitForEndOfFrame();
            isClicked           = GetComponent<IStateValue<bool>>();
        }

        private void Start()
        {
            StartCoroutine(ResetSingleClickFilterState());
        }

        private IEnumerator ResetSingleClickFilterState()
        {
            while (true)
            {
                yield return waitForEndOfFrame;

                isClicked.State = false;
            }
        }
    }
}