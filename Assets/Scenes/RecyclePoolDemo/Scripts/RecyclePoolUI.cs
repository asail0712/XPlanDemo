using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.Recycle;
using XPlan.Utility;

namespace XPlan.Demo.Recycle
{
	public class RecyclePoolUI : MonoBehaviour
	{
		[SerializeField] BallEmitter emitter;
		[SerializeField] Text displayTxt;

        // Start is called before the first frame update
        private void Awake()
        {
            GameViewSizeForce.EnsureAndUseFixed("XPlan.Demo", 1920, 1080);
        }

        // Update is called once per frame
        void Update()
		{
			string str = "Press 'A' to Spawn a Ball \nPress 'D' to Destroy a Ball";
			str += $"\nNum Of Ball In Pool is {RecyclePool<Ball>.GetTotalNum()}";
			str += $"\nNum Of Ball In Scene is {emitter.GetNumInCamera()}";
			
			displayTxt.text = str;
		}
	}
}
