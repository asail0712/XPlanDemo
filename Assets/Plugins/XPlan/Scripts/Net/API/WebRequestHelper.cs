using System;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Net
{
    public static class WebRequestHelper
    {
        /*****************************
         * ����API�^��
         * ***************************/
        static private int numOfWaiting = 0;

        static public int GetWaitingNum()
        {
            return numOfWaiting;
        }

        static internal void IncreaseWaitingNum()
        {
            ++numOfWaiting;
        }

        static internal void DecreaseWaitingNum()
        {
            --numOfWaiting;
        }

        /*****************************
         * API�o�Ϳ��~�������B�z
         * ***************************/
        static private List<Action<string, string>> errorActions = new List<Action<string, string>>();

        static public void AddErrorDelegate(Action<string, string> errorAction)
        {
            errorActions.Add(errorAction);
        }

        static public void RemoveErrorDelegate(Action<string, string> errorAction)
        {
            errorActions.Remove(errorAction);
        }

        static public void ClearAllErrorDelegate()
        {
            errorActions.Clear();
        }

        static internal void TriggerError(string apiName, string errorMsg)
        {
            foreach(Action<string, string> errorAction in errorActions)
            {
                errorAction?.Invoke(apiName, errorMsg);
            }
        }
    }
}
