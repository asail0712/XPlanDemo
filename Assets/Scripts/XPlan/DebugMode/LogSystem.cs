using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace XPlan.DebugMode
{ 
    public static class LogSystem
    {
        public static void Record(string logInfo, LogType logLevel = LogType.Log, Action<string> onFinish = null)
		{
            StackTrace stackTrace   = new StackTrace(true);
            StackFrame frame        = stackTrace.GetFrame(1);
            string className        = frame.GetMethod().DeclaringType.Name;
            string methodName       = frame.GetMethod().Name;
            string lineNumber       = frame.GetFileLineNumber().ToString();

            string fullLogInfo      = $"{logInfo} at [ {className}::{methodName}() ], line {lineNumber} ";

			switch (logLevel)
			{
                case LogType.Log:
                    UnityEngine.Debug.Log(fullLogInfo);
                    break;
                case LogType.Warning:
                    UnityEngine.Debug.LogWarning(fullLogInfo);
                    break;
                case LogType.Error:
                    UnityEngine.Debug.LogError(fullLogInfo);
                    break;
            }

            onFinish?.Invoke(fullLogInfo);
        }
	}
}