using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Demo.Architecture
{ 
    public class CalculatorSystem : SystemBase
    {
		protected override void OnPreInitial()
		{
			Application.targetFrameRate = 60;
		}

		protected override void OnInitialLogic()
		{
			// 其他功能透過ServiceLocator調用計算機功能
			RegisterLogic(new CalculatorInterface());
			// 協助與UI或interface做溝通
			RegisterLogic(new CalculatorLogic());
			// 做加法運算
			RegisterLogic(new AdditionLogic());
			// 做減法運算
			RegisterLogic(new SubtractionLogic());
			// 做乘法運算
			RegisterLogic(new MultiplicationLogic());
			// 做除法運算
			RegisterLogic(new DivisionLogic());
		}
    }
}