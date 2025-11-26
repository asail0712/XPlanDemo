using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

using UnityEngine;

using XPlan.Weaver.Abstractions;

/*************************************************
* LogAspect 的實作：原本 Patch_LogAspect 搬進來
*************************************************/

namespace XPlan.Editors.Weaver
{
    internal sealed class LogAspectWeaver : IMethodAspectWeaver
    {
        public string AttributeFullName => "XPlan.LogAspectAttribute";

        public void Apply(ModuleDefinition module, MethodDefinition targetMethod, CustomAttribute attr)
        {
            if (!targetMethod.HasBody)
                return;

            // 取得 Attribute 的 LoggerType
            if (attr.ConstructorArguments.Count != 1)
                throw new InvalidOperationException("[LogAspect] 構造參數錯誤");

            var loggerTypeRef   = module.ImportReference((TypeReference)attr.ConstructorArguments[0].Value);
            var loggerTypeDef   = loggerTypeRef.Resolve();

            // 找 Logger 的無參數建構子
            var loggerCtorDef   = loggerTypeDef.Methods.First(m => m.IsConstructor && !m.HasParameters);

            var loggerCtor      = module.ImportReference(loggerCtorDef);

            // 找 Before / After
            var beforeDef       = loggerTypeDef.Methods.First(m =>
                m.Name == "Before" &&
                m.Parameters.Count == 1 &&
                m.Parameters[0].ParameterType.FullName == "System.String");

            var afterDef        = loggerTypeDef.Methods.First(m =>
                m.Name == "After" &&
                m.Parameters.Count == 2 &&
                m.Parameters[0].ParameterType.FullName == "System.String" &&
                m.Parameters[1].ParameterType.FullName == "System.Int64");

            var beforeRef   = module.ImportReference(beforeDef);
            var afterRef    = module.ImportReference(afterDef);

            // Stopwatch
            var swType      = typeof(System.Diagnostics.Stopwatch);
            var swStartNew  = module.ImportReference(swType.GetMethod("StartNew", Type.EmptyTypes));
            var swStop      = module.ImportReference(swType.GetMethod("Stop"));
            var swElapsed   = module.ImportReference(swType.GetProperty("ElapsedMilliseconds").GetMethod);
            var swTypeRef   = module.ImportReference(swType);

            var declaringType   = targetMethod.DeclaringType;
            var userMethodName  = targetMethod.Name;

            // ==== 1. 用 CecilHelper 建立原始邏輯方法，把舊 body 搬過去 ====
            MethodDefinition originalMethod = CecilHelper.CloneAsOriginalMethod(targetMethod);
            declaringType.Methods.Add(originalMethod);

            // ==== 2. 清掉原方法 body，重建成 wrapper ====
            targetMethod.Body = new MethodBody(targetMethod)
            {
                InitLocals = true
            };
            var il = targetMethod.Body.GetILProcessor();

            // locals
            var loggerVar = new VariableDefinition(loggerTypeRef);
            targetMethod.Body.Variables.Add(loggerVar);

            var swVar = new VariableDefinition(swTypeRef);
            targetMethod.Body.Variables.Add(swVar);

            bool hasReturn = targetMethod.ReturnType.FullName != "System.Void";
            VariableDefinition returnVar = null;
            if (hasReturn)
            {
                returnVar = new VariableDefinition(targetMethod.ReturnType);
                targetMethod.Body.Variables.Add(returnVar);
            }

            var displayName = declaringType.FullName + "." + userMethodName;

            // logger = new LoggerType();
            il.Emit(OpCodes.Newobj, loggerCtor);
            il.Emit(OpCodes.Stloc, loggerVar);

            // logger.Before(name)
            il.Emit(OpCodes.Ldloc, loggerVar);
            il.Emit(OpCodes.Ldstr, displayName);
            il.Emit(OpCodes.Callvirt, beforeRef);

            // sw = Stopwatch.StartNew();
            il.Emit(OpCodes.Call, swStartNew);
            il.Emit(OpCodes.Stloc, swVar);

            // 呼叫原本邏輯的 Add__Weaved
            if (!originalMethod.IsStatic)
                il.Emit(OpCodes.Ldarg_0);

            for (int i = 0; i < targetMethod.Parameters.Count; i++)
            {
                var index = originalMethod.IsStatic ? i : i + 1;
                il.Emit(OpCodes.Ldarg, index);
            }

            var weavedRef = module.ImportReference(originalMethod);
            if (!originalMethod.IsStatic && originalMethod.IsVirtual)
                il.Emit(OpCodes.Callvirt, weavedRef);
            else
                il.Emit(OpCodes.Call, weavedRef);

            if (hasReturn)
                il.Emit(OpCodes.Stloc, returnVar);

            // sw.Stop();
            il.Emit(OpCodes.Ldloc, swVar);
            il.Emit(OpCodes.Callvirt, swStop);

            // logger.After(name, elapsed)
            il.Emit(OpCodes.Ldloc, loggerVar);
            il.Emit(OpCodes.Ldstr, displayName);
            il.Emit(OpCodes.Ldloc, swVar);
            il.Emit(OpCodes.Callvirt, swElapsed);
            il.Emit(OpCodes.Callvirt, afterRef);

            if (hasReturn)
                il.Emit(OpCodes.Ldloc, returnVar);

            il.Emit(OpCodes.Ret);

            Debug.Log($"[LogAspectWeaver] LogAspect 注入完成：{displayName}");
        }
    }
}