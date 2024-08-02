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

		protected override void OnInitialHandler()
		{
			// ��L�\��z�LServiceLocator�եέp����\��
			RegisterLogic(new CalculatorInterface());
			// ��U�PUI��interface�����q
			RegisterLogic(new CalculatorLogic());
			// ���[�k�B��
			RegisterLogic(new AdditionLogic());
			// ����k�B��
			RegisterLogic(new SubtractionLogic());
			// �����k�B��
			RegisterLogic(new MultiplicationLogic());
			// �����k�B��
			RegisterLogic(new DivisionLogic());
		}
    }
}