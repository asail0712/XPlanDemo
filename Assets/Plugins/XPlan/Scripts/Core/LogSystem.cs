// ==============================================================================
// XPlan Framework
//
// Copyright (c) 2026 Asail
// All rights reserved.
//
// Author  : Asail0712
// Project : XPlan
// Description:
//     A modular framework for Unity projects, focusing on MVVM architecture,
//     runtime tooling, event-driven design, and extensibility.
//
// Contact : asail0712@gmail.com
// GitHub  : https://github.com/asail0712/XPlanDemo
//
// Unauthorized copying, modification, or distribution of this file,
// via any medium, is strictly prohibited without prior permission.
// ==============================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace XPlan
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
        public static void Record(string logInfo, LogType logLevel = LogType.Log, bool bShowLocal = false)
		{
            if(bShowLocal)
			{
                StackInfo stackTrace    = new StackInfo(2);
                string className        = stackTrace.GetClassName();
                string methodName       = stackTrace.GetMethodName();
                string lineNumber       = stackTrace.GetLineNumber();
                logInfo                 += $" at[ { className}::{ methodName} () ], line { lineNumber} ";
            }

			switch (logLevel)
			{
                case LogType.Log:
                    UnityEngine.Debug.Log(logInfo);
                    break;
                case LogType.Warning:
                    UnityEngine.Debug.LogWarning(logInfo);
                    break;
                case LogType.Error:
                    UnityEngine.Debug.LogError(logInfo);
                    break;
            }
        }
    }
}