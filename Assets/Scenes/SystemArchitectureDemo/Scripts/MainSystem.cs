using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Demo.Architecture
{ 
    public class MainSystem : SystemBase
    {
		protected override void OnPreInitial()
		{
			Application.targetFrameRate = 60;
		}

		protected override void OnInitialHandler()
		{
			RegisterLogic(new CalculatorLogic());
			RegisterLogic(new AdditionLogic());
			RegisterLogic(new SubtractionLogic());
			RegisterLogic(new MultiplicationLogic());
			RegisterLogic(new DivisionLogic());
		}
    }
}