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
            // ��L�\��z�LServiceLocator�եέp����\��
            ServiceLocator.Register<ICalculator>(this);
        }

        public void Calcular(int a, int b, OperatorType type, Action<int> finishAction)
        {
            SendMsg<CalculatorMsg>(a, b, type, finishAction);
		}
    }
}