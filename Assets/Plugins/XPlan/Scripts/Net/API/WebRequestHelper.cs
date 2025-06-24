using System;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Net
{
    public static class WebRequestHelper
    {
        /*****************************
         * 等待API回應
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
         * API發生錯誤的相關處理
         * ***************************/
        static private List<Action<string, string, string>> errorActions = new List<Action<string, string, string>>();

        static public void AddErrorDelegate(Action<string, string, string> errorAction)
        {
            errorActions.Add(errorAction);
        }

        static public void RemoveErrorDelegate(Action<string, string, string> errorAction)
        {
            errorActions.Remove(errorAction);
        }

        static public void ClearAllErrorDelegate()
        {
            errorActions.Clear();
        }

        static internal void TriggerError(string apiName, string errorType, string errorMsg)
        {
            foreach(Action<string, string, string> errorAction in errorActions)
            {
                errorAction?.Invoke(apiName, errorType, errorMsg);
            }
        }
    }
}
