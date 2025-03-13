using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.UI
{
    public class CanvasInfo : MonoBehaviour
    {
        [HideInInspector]
        public int defaultDisplayIdx;

        private void Awake()
        {
            Canvas canvas       = GetComponent<Canvas>();
            defaultDisplayIdx   = canvas.targetDisplay;
        }
    }
}
