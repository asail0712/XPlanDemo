﻿using System;

namespace XPlan.Net
{
    public interface IConnectHandler
    {
        Uri Url { get; }
        void Connect();
        void Interruptconnect();
        void CloseConnect();
    }
}
