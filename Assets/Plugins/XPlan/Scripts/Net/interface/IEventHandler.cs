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

namespace XPlan.Net
{
    public interface IEventHandler
    {
        void Open(IConnectHandler eventHandler);
        void Close(IConnectHandler eventHandler, bool bErrorHappen);
        void Error(IConnectHandler eventHandler, string errorTxt);
        void Message(IConnectHandler eventHandler, string msgTxt);
        void Binary(IConnectHandler eventHandler, byte[] byteArr);
    }
}
