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

            // ★★ 1) 不再從 Attribute 拿 Type，而是從方法參數自動推斷 ★★
            if (!targetMethod.HasParameters || targetMethod.Parameters.Count != 1)
            {
                throw new InvalidOperationException(
                    $"[NotifyHandler] {targetMethod.FullName} 必須僅有一個參數（訊息類型）");
            }

            // 方法參數型別 = TMsg
            var msgTypeRef = module.ImportReference(targetMethod.Parameters[0].ParameterType);

            // ★ 找 RegisterNotify<TMsg>(Action<TMsg>)
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

            // ★ Step 1: 注入靜態布林欄位，用於追蹤是否已註冊 (實現冪等性)
            // 欄位名稱應獨特，以避免多個 NotifyHandler 產生衝突
            var flagFieldName       = $"<XPlan>k__NotifyRegistered_{targetMethod.Name}_{msgTypeRef.Name}";

            // 檢查是否已存在，避免編織器重複執行時多次新增欄位
            var registrationFlag    = declaringType.Fields.FirstOrDefault(f => f.Name == flagFieldName);

            if (registrationFlag == null)
            {
                registrationFlag = new FieldDefinition(
                    flagFieldName,
                    // 必須是 Static 和 Private，用於跨實例共享註冊狀態
                    FieldAttributes.Private | FieldAttributes.Static,
                    module.TypeSystem.Boolean);
                declaringType.Fields.Add(registrationFlag);
            }

            // ★ Construct Action<TMsg>
            var actionOpenType  = module.ImportReference(typeof(Action<>)); // Action<T> 的開放泛型型別引用
            var actionGeneric   = new GenericInstanceType(actionOpenType);    // Action<TMsg> 的泛型實例
            actionGeneric.GenericArguments.Add(msgTypeRef);

            var actionCtorDef = actionOpenType.Resolve()
                .Methods.First(m => m.IsConstructor && m.Parameters.Count == 2);

            var actionCtorRef = new MethodReference(".ctor", module.TypeSystem.Void, actionGeneric)
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

            // ★ 找 / 建立 __LogicComponent_WeaverHook()
            var hookMethod = declaringType.Methods.FirstOrDefault(m =>
                    !m.IsStatic &&
                    m.Name == HookMethodName &&
                    m.Parameters.Count == 0 &&
                    m.ReturnType.FullName == module.TypeSystem.Void.FullName);

            if (hookMethod == null)
            {
                // 建一個 private void __LogicComponent_WeaverHook() { }
                hookMethod = new MethodDefinition(
                    HookMethodName,
                    MethodAttributes.Private | MethodAttributes.HideBySig,
                    module.TypeSystem.Void);

                var body        = new MethodBody(hookMethod);
                hookMethod.Body = body;

                var ilInit      = body.GetILProcessor();
                ilInit.Append(ilInit.Create(OpCodes.Ret));

                declaringType.Methods.Add(hookMethod);
            }

            // ★ 把註冊 IL 塞到 hook 的最後一個 ret 前
            if (!hookMethod.HasBody)
            {
                hookMethod.Body = new MethodBody(hookMethod);
                var ilTmp       = hookMethod.Body.GetILProcessor();
                ilTmp.Append(ilTmp.Create(OpCodes.Ret));
            }

            var ilHook      = hookMethod.Body.GetILProcessor();
            var retInstr    = hookMethod.Body.Instructions.LastOrDefault(i => i.OpCode == OpCodes.Ret);

            if (retInstr == null)
            {
                retInstr = ilHook.Create(OpCodes.Ret);
                ilHook.Append(retInstr);
            }

            // end label 就用 retInstr
            var endOfRegistrationBlock = retInstr;

            // IL: Ldsfld registrationFlag
            ilHook.InsertBefore(endOfRegistrationBlock, ilHook.Create(OpCodes.Ldsfld, registrationFlag));

            // IL: Brtrue_S endOfRegistrationBlock
            var jumpIfRegistered = ilHook.Create(OpCodes.Brtrue_S, endOfRegistrationBlock);
            ilHook.InsertBefore(endOfRegistrationBlock, jumpIfRegistered);

            // 註冊邏輯：
            // 1. this (作為 RegisterNotify 的 this)
            ilHook.InsertBefore(endOfRegistrationBlock, ilHook.Create(OpCodes.Ldarg_0));

            // 2. this (作為 Action<TMsg> 的 target)
            ilHook.InsertBefore(endOfRegistrationBlock, ilHook.Create(OpCodes.Ldarg_0));

            // 3. 函式指標 ldftn targetMethod
            ilHook.InsertBefore(endOfRegistrationBlock, ilHook.Create(OpCodes.Ldftn, targetMethod));

            // 4. newobj Action<TMsg>(object, IntPtr)
            ilHook.InsertBefore(endOfRegistrationBlock, ilHook.Create(OpCodes.Newobj, actionCtorRef));

            // 5. call RegisterNotify<TMsg>(Action<TMsg>)
            ilHook.InsertBefore(endOfRegistrationBlock, ilHook.Create(OpCodes.Call, registerNotifyGeneric));

            // 6. 設定 flag = true
            ilHook.InsertBefore(endOfRegistrationBlock, ilHook.Create(OpCodes.Ldc_I4_1));
            ilHook.InsertBefore(endOfRegistrationBlock, ilHook.Create(OpCodes.Stsfld, registrationFlag));

            Debug.Log($"[NotifyHandlerWeaver] 注入成功：{declaringType.FullName}.{targetMethod.Name} -> {HookMethodName}，已加入冪等性檢查。");
        }
    }
}
