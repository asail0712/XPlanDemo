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
#if WEAVING_ENABLE
using Mono.Cecil;
using Mono.Cecil.Cil;
#endif // WEAVING_ENABLE

using System;
using System.Linq;
using UnityEngine;

using XPlan.Activity;
using XPlan.Weaver.Abstractions;

/*************************************************
* Tracker 的實作
*************************************************/

namespace XPlan.Editors.Weaver
{
    internal sealed class TrackerWeaver : IMethodAspectWeaver
    {
        public string AttributeFullName => "XPlan.TrackerAttribute";

#if WEAVING_ENABLE        
        private const string kTraceTypeFullName = "XPlan.Activity.ActivityTrace";
        private const string kTraceMethodName   = "Touch";

        public void Apply(ModuleDefinition module, MethodDefinition targetMethod, CustomAttribute attr)
        {
            if (module == null || targetMethod == null || attr == null)
                return;

            // 抽 feature 名稱：TrackerAttribute("xxx")；沒填就用 Type.Method
            string feature  = ExtractFeatureName(targetMethod, attr);

            // 取得要呼叫的 ActivityTrace.Touch(string)
            var touchMethodInfo = typeof(ActivityTrace).GetMethod(nameof(ActivityTrace.Touch), new[] { typeof(string) });
            var touchRef        = module.ImportReference(touchMethodInfo);
            
            if (touchRef == null)
            {
                Debug.LogWarning($"[TrackerWeaver] Cannot resolve {kTraceTypeFullName}::{kTraceMethodName}(string). Skip weaving.");
                return;
            }

            // 1) 一般方法：直接注入 method entry
            // 2) async/iterator：優先注入到 state machine 的 MoveNext()（更符合「真正執行時」）
            if (TryWeaveStateMachineMoveNext(module, targetMethod, feature, touchRef))
                return;

            // fallback：織在原方法進入點
            TryWeaveMethodEntry(targetMethod, feature, touchRef);
        }

        private static string ExtractFeatureName(MethodDefinition method, CustomAttribute attr)
        {
            // ① ctor arg: TrackerAttribute(string feature = null)
            string feature = null;
            try
            {
                if (attr.HasConstructorArguments && attr.ConstructorArguments.Count >= 1)
                {
                    feature = attr.ConstructorArguments[0].Value as string;
                }
            }
            catch { /* ignore */ }

            // ② named arg 例如：[Tracker(Feature="xxx")]
            if (string.IsNullOrEmpty(feature) && attr.HasProperties)
            {
                var p = attr.Properties.FirstOrDefault(x => x.Name.Equals("Feature", StringComparison.Ordinal));
                if (p.Name != null) feature = p.Argument.Value as string;
            }

            // ③ default：Type.Method
            if (string.IsNullOrEmpty(feature))
                feature = $"{method.DeclaringType.FullName}.{method.Name}";

            return feature;
        }

        private static bool TryWeaveMethodEntry(MethodDefinition method, string feature, MethodReference touchRef)
        {
            if (method == null || !method.HasBody || method.Body == null)
                return false;

            if (method.IsAbstract)
                return false;

            // 避免重複注入：檢查開頭幾個指令是否已經有 call ActivityTrace.Touch
            if (AlreadyWoven(method, touchRef))
                return true;

            var body    = method.Body;
            var il      = body.GetILProcessor();
            var first   = body.Instructions.FirstOrDefault();

            if (first == null)
                return false;

            // 插入：ldstr feature ; call ActivityTrace.Touch
            il.InsertBefore(first, il.Create(OpCodes.Ldstr, feature));
            il.InsertBefore(first, il.Create(OpCodes.Call, touchRef));

            return true;
        }

        private static bool AlreadyWoven(MethodDefinition method, MethodReference touchRef)
        {
            if (!method.HasBody || method.Body?.Instructions == null) return false;

            // 粗略判斷：前 8 個指令內若出現 call 到同名/同簽名，就視為已注入
            var ins = method.Body.Instructions;
            int n   = Math.Min(8, ins.Count);

            for (int i = 0; i < n; i++)
            {
                if (ins[i].OpCode == OpCodes.Call || ins[i].OpCode == OpCodes.Callvirt)
                {
                    if (ins[i].Operand is MethodReference mr)
                    {
                        if (mr.Name == touchRef.Name &&
                            mr.DeclaringType.FullName == touchRef.DeclaringType.FullName &&
                            mr.Parameters.Count == 1 &&
                            mr.Parameters[0].ParameterType.MetadataType == MetadataType.String)
                            return true;
                    }
                }
            }

            return false;
        }

        private static bool TryWeaveStateMachineMoveNext(ModuleDefinition module, MethodDefinition originalMethod, string feature, MethodReference touchRef)
        {
            // Async / Iterator 都會掛 StateMachineAttribute 派生類：
            // - System.Runtime.CompilerServices.AsyncStateMachineAttribute
            // - System.Runtime.CompilerServices.IteratorStateMachineAttribute
            var smAttr = originalMethod.CustomAttributes.FirstOrDefault(a =>
                a.AttributeType.FullName == "System.Runtime.CompilerServices.AsyncStateMachineAttribute" ||
                a.AttributeType.FullName == "System.Runtime.CompilerServices.IteratorStateMachineAttribute"
            );

            if (smAttr == null || !smAttr.HasConstructorArguments)
                return false;

            // ctor arg[0] = StateMachineType
            if (smAttr.ConstructorArguments[0].Value is not TypeReference smTypeRef)
                return false;

            // 解析 state machine type
            var smTypeDef = smTypeRef.Resolve();
            if (smTypeDef == null)
                return false;

            // 找 MoveNext
            var moveNext = smTypeDef.Methods.FirstOrDefault(m =>
                m.Name == "MoveNext" &&
                !m.IsStatic &&
                m.Parameters.Count == 0
            );

            if (moveNext == null || !moveNext.HasBody)
                return false;

            // 避免重複織入
            if (AlreadyWoven(moveNext, touchRef))
                return true;

            // 在 MoveNext 入口注入
            return TryWeaveMethodEntry(moveNext, feature, touchRef);
        }
#endif // WEAVING_ENABLE
    }
}