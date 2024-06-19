using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace XPlan.DebugMode
{ 
    public class StackInfo
	{
        private StackTrace stackTrace;
        private StackFrame stackFrame;

        public StackInfo(int stackNum = 1)
		{
            stackTrace = new StackTrace(true);
            stackFrame = stackTrace.GetFrame(stackNum);
        }

        public string GetClassName()
		{
            return stackFrame.GetMethod().DeclaringType.Name;
        }

        public string GetMethodName()
        {
            return stackFrame.GetMethod().Name;
        }

        public string GetLineNumber()
        {
            return stackFrame.GetFileLineNumber().ToString();
        }
    }

    public static class LogSystem
    {
        public static void Record(string logInfo, LogType logLevel = LogType.Log)
		{
#if DEBUG
            StackInfo stackTrace    = new StackInfo();
            string className        = stackTrace.GetClassName();
            string methodName       = stackTrace.GetMethodName();
            string lineNumber       = stackTrace.GetLineNumber();
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
#endif //DEBUG
        }
    }
}