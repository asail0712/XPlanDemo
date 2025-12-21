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

        private const string HookMethodName = "__LogicComponent_WeaverHook";

        public void Apply(ModuleDefinition module, MethodDefinition targetMethod, CustomAttribute attr)
        {
            var declaringType = targetMethod.DeclaringType;

            // 方法必須僅有一個參數（訊息類型）
            if (!targetMethod.HasParameters || targetMethod.Parameters.Count != 1)
            {
                throw new InvalidOperationException(
                    $"[NotifyHandler] {targetMethod.FullName} 必須僅有一個參數（訊息類型）");
            }

            // 方法參數型別 = TMsg
            var msgTypeRef = module.ImportReference(targetMethod.Parameters[0].ParameterType);

            // 找 RegisterNotify<TMsg>(Action<TMsg>)
            var registerNotifyDef = CecilHelper.FindMethodInHierarchy(
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

            // ★ Step 1: 注入「instance bool」欄位（每個 instance 自己記錄是否已註冊）
            // 欄位名稱應獨特，以避免多個 NotifyHandler 產生衝突
            var flagFieldName = $"<XPlan>k__NotifyRegistered_{targetMethod.Name}_{msgTypeRef.Name}";

            // 避免編織器重複執行時多次新增欄位
            var registrationFlag = declaringType.Fields.FirstOrDefault(f => f.Name == flagFieldName);
            if (registrationFlag == null)
            {
                registrationFlag = new FieldDefinition(
                    flagFieldName,
                    FieldAttributes.Private,          // ✅ 不要 Static
                    module.TypeSystem.Boolean);

                declaringType.Fields.Add(registrationFlag);
            }

            // ★ Construct Action<TMsg>
            var actionOpenType = module.ImportReference(typeof(Action<>));     // Action<T> 開放泛型
            var actionGeneric = new GenericInstanceType(actionOpenType);       // Action<TMsg>
            actionGeneric.GenericArguments.Add(msgTypeRef);

            var actionCtorDef = actionOpenType.Resolve()
                .Methods.First(m => m.IsConstructor && m.Parameters.Count == 2); // .ctor(object, IntPtr)

            var actionCtorRef = new MethodReference(".ctor", module.TypeSystem.Void, actionGeneric)
            {
                HasThis = true,
                ExplicitThis = false,
                CallingConvention = actionCtorDef.CallingConvention
            };

            foreach (var p in actionCtorDef.Parameters)
                actionCtorRef.Parameters.Add(new ParameterDefinition(p.ParameterType));

            // ★ Construct RegisterNotify<TMsg>
            var registerNotifyRef = module.ImportReference(registerNotifyDef);
            var registerNotifyGeneric = new GenericInstanceMethod(registerNotifyRef);
            registerNotifyGeneric.GenericArguments.Add(msgTypeRef);

            // ★ 找 / 建立 __LogicComponent_WeaverHook()
            var hookMethod = declaringType.Methods.FirstOrDefault(m =>
                !m.IsStatic &&
                m.Name == HookMethodName &&
                m.Parameters.Count == 0 &&
                m.ReturnType.FullName == module.TypeSystem.Void.FullName);

            if (hookMethod == null)
            {
                hookMethod = new MethodDefinition(
                    HookMethodName,
                    MethodAttributes.Private | MethodAttributes.HideBySig,
                    module.TypeSystem.Void);

                hookMethod.Body = new MethodBody(hookMethod);
                hookMethod.Body.GetILProcessor().Append(Instruction.Create(OpCodes.Ret));
                declaringType.Methods.Add(hookMethod);
            }

            if (!hookMethod.HasBody)
            {
                hookMethod.Body = new MethodBody(hookMethod);
                hookMethod.Body.GetILProcessor().Append(Instruction.Create(OpCodes.Ret));
            }

            // 找最後一個 ret，沒有就補一個
            var ilHook = hookMethod.Body.GetILProcessor();
            var retInstr = hookMethod.Body.Instructions.LastOrDefault(i => i.OpCode == OpCodes.Ret);
            if (retInstr == null)
            {
                retInstr = ilHook.Create(OpCodes.Ret);
                ilHook.Append(retInstr);
            }

            // ★ 防止重複注入：如果 hook 內已經操作過這個 registrationFlag，就略過
            bool alreadyInjected = hookMethod.Body.Instructions.Any(i =>
                (i.OpCode == OpCodes.Ldfld || i.OpCode == OpCodes.Stfld) &&
                i.Operand is FieldReference fr &&
                fr.Name == registrationFlag.Name);

            if (alreadyInjected)
            {
                Debug.Log($"[NotifyHandlerWeaver] Skip：{declaringType.FullName}.{HookMethodName} 已注入 {registrationFlag.Name}");
                return;
            }

            // end label 就用 retInstr
            var end = retInstr;

            // ---------------------------------------------------------
            // 注入 IL（instance 欄位版本，堆疊必須正確）
            //
            // if (this.flag) goto end;
            // this.RegisterNotify<TMsg>(new Action<TMsg>(this, &targetMethod));
            // this.flag = true;
            // ---------------------------------------------------------

            // if (this.flag) goto end;
            ilHook.InsertBefore(end, ilHook.Create(OpCodes.Ldarg_0));
            ilHook.InsertBefore(end, ilHook.Create(OpCodes.Ldfld, registrationFlag));
            ilHook.InsertBefore(end, ilHook.Create(OpCodes.Brtrue_S, end));

            // this.RegisterNotify<TMsg>(new Action<TMsg>(this, &targetMethod));
            ilHook.InsertBefore(end, ilHook.Create(OpCodes.Ldarg_0));          // this for instance call
            ilHook.InsertBefore(end, ilHook.Create(OpCodes.Ldarg_0));          // this for delegate target
            ilHook.InsertBefore(end, ilHook.Create(OpCodes.Ldftn, targetMethod));
            ilHook.InsertBefore(end, ilHook.Create(OpCodes.Newobj, actionCtorRef));
            ilHook.InsertBefore(end, ilHook.Create(OpCodes.Call, registerNotifyGeneric));

            // this.flag = true;
            ilHook.InsertBefore(end, ilHook.Create(OpCodes.Ldarg_0));
            ilHook.InsertBefore(end, ilHook.Create(OpCodes.Ldc_I4_1));
            ilHook.InsertBefore(end, ilHook.Create(OpCodes.Stfld, registrationFlag));

            Debug.Log($"[NotifyHandlerWeaver] 注入成功：{declaringType.FullName}.{targetMethod.Name} -> {HookMethodName}（instance 冪等）");
        }
    }
}
