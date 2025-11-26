using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

using UnityEngine;

using XPlan.Weaver.Abstractions;

namespace XPlan.Editors.Weaver
{
    internal sealed class NotifyHandlerWeaver : IMethodAspectWeaver
    {
        public string AttributeFullName => "XPlan.NotifyHandlerAttribute";

        public void Apply(ModuleDefinition module, MethodDefinition targetMethod, CustomAttribute attr)
        {
            var declaringType = targetMethod.DeclaringType;

            // ★★ 1) 不再從 Attribute 拿 Type，而是從方法參數自動推斷 ★★
            if (!targetMethod.HasParameters || targetMethod.Parameters.Count != 1)
            {
                throw new InvalidOperationException(
                    $"[NotifyHandler] {targetMethod.FullName} 必須僅有一個參數（訊息類型）");
            }

            // 方法參數型別 = TMsg
            var msgTypeRef          = module.ImportReference(targetMethod.Parameters[0].ParameterType);

            // ★ 找 RegisterNotify<TMsg>(Action<TMsg>)
            var registerNotifyDef   = CecilHelper.FindMethodInHierarchy(
                declaringType,
                m =>
                    m.Name == "RegisterNotify" &&
                    m.HasGenericParameters &&
                    m.GenericParameters.Count == 1 &&
                    m.Parameters.Count == 1 &&
                    m.Parameters[0].ParameterType.Namespace == "System" &&
                    m.Parameters[0].ParameterType.Name.StartsWith("Action`1")
            );

            if (registerNotifyDef == null)
            {
                Debug.LogWarning($"[NotifyHandlerWeaver] 無法在 {declaringType.FullName} 找到 RegisterNotify<T>，略過 weaving");
                return;
            }

            // ★ Construct Action<TMsg>
            var actionOpenType  = module.ImportReference(typeof(Action<>));
            var actionGeneric   = new GenericInstanceType(actionOpenType);
            actionGeneric.GenericArguments.Add(msgTypeRef);

            var actionCtorDef   = actionOpenType.Resolve()
                .Methods.First(m => m.IsConstructor && m.Parameters.Count == 2);

            var actionCtorRef   = new MethodReference(".ctor", module.TypeSystem.Void, actionGeneric)
            {
                HasThis             = true,
                ExplicitThis        = false,
                CallingConvention   = actionCtorDef.CallingConvention
            };
            foreach (var p in actionCtorDef.Parameters)
                actionCtorRef.Parameters.Add(new ParameterDefinition(p.ParameterType));

            // ★ Construct RegisterNotify<TMsg>
            var registerNotifyRef       = module.ImportReference(registerNotifyDef);
            var registerNotifyGeneric   = new GenericInstanceMethod(registerNotifyRef);
            registerNotifyGeneric.GenericArguments.Add(msgTypeRef);

            // ★ 注入到所有 instance constructors
            foreach (var ctor in declaringType.Methods.Where(m => m.IsConstructor && !m.IsStatic))
            {
                if (!ctor.HasBody)
                    continue;

                var il  = ctor.Body.GetILProcessor();
                var ret = ctor.Body.Instructions.Last(i => i.OpCode == OpCodes.Ret);

                il.InsertBefore(ret, il.Create(OpCodes.Ldarg_0));           // this
                il.InsertBefore(ret, il.Create(OpCodes.Ldarg_0));           // this for delegate target
                il.InsertBefore(ret, il.Create(OpCodes.Ldftn, targetMethod)); // &method
                il.InsertBefore(ret, il.Create(OpCodes.Newobj, actionCtorRef));
                il.InsertBefore(ret, il.Create(OpCodes.Call, registerNotifyGeneric));
            }

            Debug.Log($"[NotifyHandlerWeaver] 注入成功：{declaringType.FullName}.{targetMethod.Name}");
        }
    }
}
