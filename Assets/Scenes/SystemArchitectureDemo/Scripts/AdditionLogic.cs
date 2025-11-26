using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Observe;

namespace XPlan.Demo.Architecture
{
    public class AdditionMsg : MessageBase
	{
        public int a;
        public int b;
        public Action<int> finishAction;

        public AdditionMsg(int a, int b, Action<int> finishAction)
		{
            this.a              = a;
            this.b              = b;
            this.finishAction   = finishAction;
        }
    }

    public class AdditionLogic : LogicComponent
    {
        [NotifyHandler]
        private void Addition(AdditionMsg msg)
        {
            int a = msg.a;
            int b = msg.b;

            msg.finishAction?.Invoke(a + b);
        }
    }
}
