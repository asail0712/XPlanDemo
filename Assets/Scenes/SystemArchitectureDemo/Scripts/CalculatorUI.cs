using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XPlan.UI;

namespace XPlan.Demo.Architecture
{
    public class CalculatorUI : UIBase
    {
        [SerializeField] InputField inputTextA;
        [SerializeField] InputField inputTextB;
        [SerializeField] Dropdown operatorDropdown;
        [SerializeField] Text answerText;
        [SerializeField] Button calculatorBtn;

        // Start is called before the first frame update
        void Awake()
        {
            OperatorType operatorType = OperatorType.Addition;

            RegisterDropdown("", operatorDropdown, (operatorStr) => 
            {
                switch(operatorStr)
				{
                    case "+":
                        operatorType = OperatorType.Addition;
                        break;
                    case "-":
                        operatorType = OperatorType.Subtraction;
                        break;
                    case "*":
                        operatorType = OperatorType.Multiplication;
                        break;
                    case "/":
                        operatorType = OperatorType.Division;
                        break;
                }
            });

            RegisterButton<CalculatorInfo>(UIRequest.ToCalcular, calculatorBtn, () => new CalculatorInfo()
            {
                a               = int.Parse(inputTextA.text),                
                b               = int.Parse(inputTextB.text),
                operatorType    = operatorType,
            });

            ListenCall<int>(UICommand.CalcularResult, (result) => 
            {
                answerText.text = result.ToString();
            });
        }
    }
}