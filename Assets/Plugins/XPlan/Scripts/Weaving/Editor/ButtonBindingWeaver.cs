#if UNITY_EDITOR
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;
using UnityEngine;

using XPlan.Weaver.Abstractions;
using XPlan.Weaver.Runtime;

namespace XPlan.Editors.Weaver
{
    /// <summary>
    /// 針對 XPlan.UI.ViewBase`1：
    /// 在 OnEnable 末尾插入 VmButtonBindingRuntime.BindButtons(this);
    /// 在 OnDisable 末尾插入 VmButtonBindingRuntime.UnbindButtons(this);
    /// </summary>
    internal sealed class ButtonBindingWeaver : ITypeAspectWeaver
    {
        public string AttributeFullName => "XPlan.ViewBindingAttribute";

        public void Apply(ModuleDefinition module, TypeDefinition targetType, CustomAttribute attr)
        {
            if (targetType == null)
                return;

            // 只處理 ViewBase<TViewModel>
            if (!CecilHelper.TryFindViewBaseGeneric(targetType, out var dummy, "XPlan.UI.ViewBase`1"))
                return;

            // 取得 Runtime 的靜態方法參考
            var bindMethodInfo = typeof(VmButtonBindingRuntime).GetMethod(
                nameof(VmButtonBindingRuntime.BindButtons),
                new[] { typeof(object) });

            var unbindMethodInfo = typeof(VmButtonBindingRuntime).GetMethod(
                nameof(VmButtonBindingRuntime.UnbindButtons),
                new[] { typeof(object) });

            if (bindMethodInfo == null || unbindMethodInfo == null)
                throw new InvalidOperationException("[VmButtonBindingViewBaseWeaver] 找不到 VmButtonBindingRuntime.BindButtons / UnbindButtons");

            var bindMethodRef   = module.ImportReference(bindMethodInfo);
            var unbindMethodRef = module.ImportReference(unbindMethodInfo);

            // 在 OnEnable / OnDisable 中插入呼叫
            InjectCallInto(targetType, "OnEnable", bindMethodRef);
            InjectCallInto(targetType, "OnDisable", unbindMethodRef);

            Debug.Log($"[VmButtonBindingViewBaseWeaver] 成功注入 {targetType.FullName} 的 OnEnable/OnDisable");
        }

        /// <summary>
        /// 在指定方法（OnEnable / OnDisable）中插入：
        ///   VmButtonBindingRuntime.Xxx(this);
        /// 若該方法不存在就建立一個空的，然後插入呼叫。
        /// 會避免重複插入。
        /// </summary>
        private static void InjectCallInto(TypeDefinition type, string methodName, MethodReference staticMethod)
        {
            var module = type.Module;

            // 找到現有的方法：void OnEnable() / void OnDisable()
            var method = type.Methods.FirstOrDefault(m =>
                m.Name == methodName &&
                !m.HasParameters &&
                m.ReturnType.FullName == module.TypeSystem.Void.FullName);

            // 如果不存在就建立一個 protected void OnXxx() { }
            if (method == null)
            {
                method = new MethodDefinition(
                    methodName,
                    // protected、HideBySig，跟一般 Unity Message 類似
                    MethodAttributes.Family |
                    MethodAttributes.HideBySig,
                    module.TypeSystem.Void);

                type.Methods.Add(method);

                var il = method.Body.GetILProcessor();
                il.Append(il.Create(OpCodes.Ret));
            }

            var ilProcessor = method.Body.GetILProcessor();

            // 避免重複插入：如果 method 內已經有呼叫這個靜態方法就略過
            if (method.Body.Instructions.Any(i =>
                    i.OpCode == OpCodes.Call &&
                    i.Operand is MethodReference mr &&
                    mr.FullName == staticMethod.FullName))
            {
                return;
            }

            // 找到最後一個 ret 指令，準備插在它前面
            var ret = method.Body.Instructions.LastOrDefault(i => i.OpCode == OpCodes.Ret);
            if (ret == null)
            {
                // 理論上 Unity 的 OnXxx 應該一定會有 ret，這裡防呆一下
                ret = ilProcessor.Create(OpCodes.Ret);
                ilProcessor.Append(ret);
            }

            // 插入：
            //   ldarg.0        // this
            //   call Xxx(this)
            ilProcessor.InsertBefore(ret, ilProcessor.Create(OpCodes.Ldarg_0));
            ilProcessor.InsertBefore(ret, ilProcessor.Create(OpCodes.Call, staticMethod));
        }
    }
}
#endif
