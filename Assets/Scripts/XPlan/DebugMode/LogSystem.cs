using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace XPlan.DebugMode
{ 
    public enum LogLevel
	{
        Normal  = 0,
        Warning,
        Error,
	}

    public static class LogSystem
    {
        public static void Record(string logInfo, LogLevel logLevel = LogLevel.Normal, Action<string> onFinish = null)
		{
            StackTrace stackTrace   = new StackTrace(true);
            StackFrame frame        = stackTrace.GetFrame(1);
            string className        = frame.GetMethod().DeclaringType.Name;
            string methodName       = frame.GetMethod().Name;
            string lineNumber       = frame.GetFileLineNumber().ToString();

            string fullLogInfo      = $"{logInfo} at [ {className}::{methodName}() ], line {lineNumber} ";

			switch (logLevel)
			{
                case LogLevel.Normal:
                    UnityEngine.Debug.Log(fullLogInfo);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(fullLogInfo);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(fullLogInfo);
                    break;
            }

            onFinish?.Invoke(fullLogInfo);
        }
	}
}