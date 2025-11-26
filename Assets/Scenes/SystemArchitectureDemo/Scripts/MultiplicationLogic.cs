using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Observe;

namespace XPlan.Demo.Architecture
{
    public class MultiplicationMsg : MessageBase
    {
        public int a;
        public int b;
        public Action<int> finishAction;

        public MultiplicationMsg(int a, int b, Action<int> finishAction)
        {
            this.a              = a;
            this.b              = b;
            this.finishAction   = finishAction;
        }
    }
    public class MultiplicationLogic : LogicComponent
    {
        [NotifyHandler]
        private void Multiplication(MultiplicationMsg msg)
        {
            float a = (float)msg.a;
            float b = (float)msg.b;

            msg.finishAction?.Invoke((int)(a * b));
        }
    }
}
