using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Observe;

namespace XPlan.Demo.Architecture
{
    public class DivisionMsg : MessageBase
    {
        public int a;
        public int b;
        public Action<int> finishAction;

        public DivisionMsg(int a, int b, Action<int> finishAction)
        {
            this.a              = a;
            this.b              = b;
            this.finishAction   = finishAction;
        }
    }

    public class DivisionLogic : LogicComponentBase
    {
        public DivisionLogic()
        {
            RegisterNotify<DivisionMsg>((msg) =>
            {
                float a = (float)msg.a;
                float b = (float)msg.b;

                if(b.Equals(0f))
				{
                    Debug.LogError("除數不可為0");
                    return;
				}

                msg.finishAction?.Invoke((int)(a / b));
            });
        }
    }
}
