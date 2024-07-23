using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Demo.Architecture
{
    public class CalculatorInfo
	{
        public int a;
        public int b;
        public OperatorType operatorType;
    }

    public class CalculatorLogic : LogicComponentBase
    {
        // Start is called before the first frame update
        public CalculatorLogic()
        {
            Action<int> calculatorAction = (result) =>
            {
                DirectCallUI<int>(UICommand.CalcularResult, result);
            };

            AddUIListener<CalculatorInfo>(UIRequest.InputToCalcular, (info) => 
            {
                switch(info.operatorType)
				{
                    case OperatorType.Addition:
                        SendMsg<AdditionMsg>(info.a, info.b, calculatorAction);
                        break;
                    case OperatorType.Subtraction:
                        SendMsg<SubtractionMsg>(info.a, info.b, calculatorAction);
                        break;
                    case OperatorType.Multiplication:
                        SendMsg<MultiplicationMsg>(info.a, info.b, calculatorAction);
                        break;
                    case OperatorType.Division:
                        SendMsg<DivisionMsg>(info.a, info.b, calculatorAction);
                        break;
                }
            });            
        }
    }
}