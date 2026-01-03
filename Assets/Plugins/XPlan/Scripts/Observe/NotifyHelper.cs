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
using System.Reflection;
using UnityEngine;

namespace XPlan.Observe
{
    public static class NotifyHelper
    {
        public static void RegisterNotify<T>(INotifyReceiver notifyReceiver, Action<T> notifyAction) where T : MessageBase
        {
            if (notifyReceiver == null)
            {
                LogSystem.Record($"NotifyReceiver is not implement INotifyReceiver", LogType.Error);
                return;
            }

            NotifySystem.Instance.RegisterNotify<T>(notifyReceiver, (msgReceiver) =>
            {
                T msg = msgReceiver.GetMessage<T>();

                notifyAction?.Invoke(msg);
            });
        }

        public static void RegisterNotify<T>(INotifyReceiver notifyReceiver, ReceiveOption option, Action<T> notifyAction) where T : MessageBase
        {
            if (notifyReceiver == null)
            {
                LogSystem.Record($"NotifyReceiver is not implement INotifyReceiver", LogType.Error);
                return;
            }

            NotifySystem.Instance.RegisterNotify<T>(notifyReceiver, option, (msgReceiver) =>
            {
                T msg = msgReceiver.GetMessage<T>();

                notifyAction?.Invoke(msg);
            });
        }

        public static void SendMsg<T>(bool bAsync, params object[] args) where T : MessageBase
        {
            // 获取类型
            Type type = typeof(T);

            // 查找匹配的构造函数
            ConstructorInfo ctor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                CallingConventions.HasThis,
                Array.ConvertAll(args, item => item.GetType()),
                null
            );

            if (ctor == null)
            {
                throw new Exception($"No matching constructor found for {type.Name}");
            }

            // 生成msg並寄出
            T msg = (T)ctor.Invoke(args);
            msg.Send(bAsync);
        }
    }
}
