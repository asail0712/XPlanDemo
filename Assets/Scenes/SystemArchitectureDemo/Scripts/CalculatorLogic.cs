using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Observe;

namespace XPlan.Demo.Architecture
{
    public class CalculatorMsg : MessageBase
    {
        public int a;
        public int b;
        public OperatorType operatorType;
        public Action<int> finishAction;

        public CalculatorMsg(int a, int b, OperatorType operatorType, Action<int> finishAction)
		{
            this.a              = a;
            this.b              = b;
            this.operatorType   = operatorType;
            this.finishAction   = finishAction;
        }
    }

    public class CalculatorInfo
	{
        public int a;
        public int b;
        public OperatorType operatorType;
    }

    public class CalculatorLogic : LogicComponentBase, INotifyReceiver
    {
        // Start is called before the first frame update
        public CalculatorLogic()
        {
            AddUIListener<CalculatorInfo>(UIRequest.ToCalcular, (info) => 
            {
                CalcularResult(info.a, info.b, info.operatorType, (result) =>
                {
                    DirectCallUI<int>(UICommand.CalcularResult, result);
                });                
            });

            RegisterNotify<CalculatorMsg>((msg)=> 
            {
                CalcularResult(msg.a, msg.b, msg.operatorType, msg.finishAction);
            });
        }

        private void CalcularResult(int a, int b, OperatorType operatorType, Action<int> finishAction)
        {
            switch (operatorType)
            {
                case OperatorType.Addition:
                    SendMsg<AdditionMsg>(a, b, finishAction);
                    break;
                case OperatorType.Subtraction:
                    SendMsg<SubtractionMsg>(a, b, finishAction);
                    break;
                case OperatorType.Multiplication:
                    SendMsg<MultiplicationMsg>(a, b, finishAction);
                    break;
                case OperatorType.Division:
                    SendMsg<DivisionMsg>(a, b, finishAction);
                    break;
            }
        }
    }
}