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
            var flagFieldName = $"<XPlan>k__NotifyRegistered_{targetMethod.Name}_{msgTypeRef.Name}";

            // 檢查是否已存在，避免編織器重複執行時多次新增欄位
            var registrationFlag = declaringType.Fields.FirstOrDefault(f => f.Name == flagFieldName);

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
            var actionOpenType = module.ImportReference(typeof(Action<>)); // Action<T> 的開放泛型型別引用
            var actionGeneric = new GenericInstanceType(actionOpenType);    // Action<TMsg> 的泛型實例
            actionGeneric.GenericArguments.Add(msgTypeRef);

            var actionCtorDef = actionOpenType.Resolve()
                .Methods.First(m => m.IsConstructor && m.Parameters.Count == 2);

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

            // ★ 注入到所有 instance constructors
            foreach (var ctor in declaringType.Methods.Where(m => m.IsConstructor && !m.IsStatic))
            {
                if (!ctor.HasBody)
                    continue;

                var il = ctor.Body.GetILProcessor();
                // 尋找 Ret 指令作為插入點
                var ret = ctor.Body.Instructions.LastOrDefault();

                if (ret == null || ret.OpCode != OpCodes.Ret)
                {
                    Debug.LogWarning($"[NotifyHandlerWeaver] 建構子 {ctor.FullName} 的 IL 結束結構異常，無法安全注入。略過。");
                    continue;
                }

                // 建立註冊區塊結束的標記
                var endOfRegistrationBlock = ret;

                // --- 注入程式碼到 ret 之前 (即建構子邏輯的最後) ---

                // IL: Ldsfld registrationFlag (載入靜態旗標的值)
                il.InsertBefore(endOfRegistrationBlock, il.Create(OpCodes.Ldsfld, registrationFlag));

                // IL: Brtrue_S endOfRegistrationBlock (如果旗標為 True (已註冊)，跳到結尾 Ret)
                // 這裡需要先建立一個空的 Brtrue_S，後面再用目標指令更新它
                var jumpIfRegistered = il.Create(OpCodes.Brtrue_S, endOfRegistrationBlock);
                il.InsertBefore(endOfRegistrationBlock, jumpIfRegistered);

                // 註冊邏輯開始：

                // 1. Load `this` for RegisterNotify (method call target)
                il.InsertBefore(endOfRegistrationBlock, il.Create(OpCodes.Ldarg_0));

                // 2. Load `this` for delegate target (new Action<TMsg> target)
                il.InsertBefore(endOfRegistrationBlock, il.Create(OpCodes.Ldarg_0));

                // 3. Load function pointer of targetMethod
                il.InsertBefore(endOfRegistrationBlock, il.Create(OpCodes.Ldftn, targetMethod));

                // 4. Call Action<TMsg> ctor (creates the delegate instance)
                il.InsertBefore(endOfRegistrationBlock, il.Create(OpCodes.Newobj, actionCtorRef));

                // 5. Call RegisterNotify<TMsg>(Action<TMsg>)
                il.InsertBefore(endOfRegistrationBlock, il.Create(OpCodes.Call, registerNotifyGeneric));

                // 6. 設定旗標為 true
                // IL: Ldc_I4_1
                il.InsertBefore(endOfRegistrationBlock, il.Create(OpCodes.Ldc_I4_1));
                // IL: Stsfld registrationFlag 
                il.InsertBefore(endOfRegistrationBlock, il.Create(OpCodes.Stsfld, registrationFlag));
            }

            Debug.Log($"[NotifyHandlerWeaver] 注入成功：{declaringType.FullName}.{targetMethod.Name}，已加入冪等性檢查。");
        }
    }
}
