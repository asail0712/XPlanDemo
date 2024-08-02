using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Utility;

namespace XPlan.Demo.Architecture
{
    public interface ICalculator
	{
        void Calcular(int a, int b, OperatorType type, Action<int> finishAction);
    }

    public class CalculatorInterface : LogicComponentBase, ICalculator
    {
        // Start is called before the first frame update
        public CalculatorInterface()
        {
            // 其他功能透過ServiceLocator調用計算機功能
            ServiceLocator.Register<ICalculator>(this);
        }

        public void Calcular(int a, int b, OperatorType type, Action<int> finishAction)
        {
            SendMsg<CalculatorMsg>(a, b, type, finishAction);
		}
    }
}