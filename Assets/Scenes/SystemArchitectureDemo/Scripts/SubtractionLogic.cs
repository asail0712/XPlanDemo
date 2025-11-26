using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Observe;

namespace XPlan.Demo.Architecture
{
    public class SubtractionMsg : MessageBase
    {
        public int a;
        public int b;
        public Action<int> finishAction;

        public SubtractionMsg(int a, int b, Action<int> finishAction)
        {
            this.a              = a;
            this.b              = b;
            this.finishAction   = finishAction;
        }
    }
    public class SubtractionLogic : LogicComponent
    {
        [NotifyHandler]
        private void Subtraction(SubtractionMsg msg)
        {
            int a = msg.a;
            int b = msg.b;

            msg.finishAction?.Invoke(a - b);
        }
    }
}
