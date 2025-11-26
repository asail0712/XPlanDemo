using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace XPlan.Weaver.Abstractions
{
    public static class CecilHelper
    {
        /// <summary>
        /// 把 targetMethod 的 body / 參數 / 泛型 / locals / EH
        /// 通通複製到一個新的方法，並回傳這個「原始邏輯方法」。
        /// （不會自動 Add 到 DeclaringType.Methods，呼叫端自己決定何時加）
        /// </summary>
        public static MethodDefinition CloneAsOriginalMethod(MethodDefinition targetMethod, string suffix = "__Weaved")
        {
            var declaringType   = targetMethod.DeclaringType;
            var userMethodName  = targetMethod.Name;

            // 建立新的方法：Add__Weaved
            var originalMethod  = new MethodDefinition(
                userMethodName + suffix,
                targetMethod.Attributes,
                targetMethod.ReturnType
            );

            // 1) 複製參數
            foreach (var p in targetMethod.Parameters)
            {
                originalMethod.Parameters.Add(
                    new ParameterDefinition(p.Name, p.Attributes, p.ParameterType));
            }

            // 2) 複製 generic 參數
            foreach (var gp in targetMethod.GenericParameters)
            {
                var newGp = new GenericParameter(gp.Name, originalMethod);
                originalMethod.GenericParameters.Add(newGp);
            }

            // 3) 複製 body (IL、locals、EH)
            var oldBody         = targetMethod.Body;
            originalMethod.Body = new MethodBody(originalMethod)
            {
                InitLocals = oldBody.InitLocals
            };

            // locals
            foreach (var v in oldBody.Variables)
                originalMethod.Body.Variables.Add(new VariableDefinition(v.VariableType));

            // IL 指令
            var ilOrig = originalMethod.Body.GetILProcessor();
            foreach (var instr in oldBody.Instructions)
                ilOrig.Append(instr);

            // ExceptionHandlers
            foreach (var eh in oldBody.ExceptionHandlers)
                originalMethod.Body.ExceptionHandlers.Add(eh);

            return originalMethod;
        }

        /// <summary>
        /// 從指定 type 一路往上找，回傳第一個符合 predicate 的 MethodDefinition。
        /// 找不到就回傳 null。
        /// </summary>
        public static MethodDefinition FindMethodInHierarchy(
            TypeDefinition typeDef,
            Func<MethodDefinition, bool> predicate)
        {
            while (typeDef != null)
            {
                var method = typeDef.Methods.FirstOrDefault(predicate);
                if (method != null)
                    return method;

                typeDef = typeDef.BaseType?.Resolve();
            }

            return null;
        }

        /// <summary>
        /// 從指定 type 一路往上找，判斷該type是否為targetFullName的子類
        /// 找不到就回傳 null。
        /// </summary>
        public static bool IsSubclassOf(TypeDefinition type, string targetFullName, int maxDepth = 16)
        {
            if (type == null) return false;
            if (string.IsNullOrEmpty(targetFullName)) return false;

            TypeDefinition cur = type;

            // 防止意外循環，設個上限
            for (int i = 0; i < maxDepth && cur != null; i++)
            {
                // 自己就是指定型別
                if (cur.FullName == targetFullName)
                    return true;

                var baseRef = cur.BaseType;
                if (baseRef == null)
                    break;

                // 先用 FullName 判一次，不用 Resolve 也能抓到直接繼承的情況
                if (baseRef.FullName == targetFullName)
                    return true;

                try
                {
                    // 再往上爬
                    cur = baseRef.Resolve();
                }
                catch
                {
                    break;  // Resolve 失敗就跳出，最後 return false
                }
            }

            return false;
        }

        // 保留原本 API，內部改呼叫通用版
        public static bool IsMonoBehaviourSubclass(TypeDefinition type)
        {
            return IsSubclassOf(type, "UnityEngine.MonoBehaviour");
        }

        /// <summary>
        /// 讓 method 綁定到指定的泛型實例型別上。
        /// 例如：
        ///   ObservableProperty`1::get_Value  +  ObservableProperty`1<System.String>
        /// => ObservableProperty`1<System.String>::get_Value
        /// </summary>
        public static MethodReference MakeHostInstanceGeneric(MethodReference method, TypeReference hostType)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            if (hostType == null)
                throw new ArgumentNullException(nameof(hostType));

            var reference = new MethodReference(method.Name, method.ReturnType, hostType)
            {
                HasThis             = method.HasThis,
                ExplicitThis        = method.ExplicitThis,
                CallingConvention   = method.CallingConvention
            };

            foreach (var param in method.Parameters)
                reference.Parameters.Add(new ParameterDefinition(param.ParameterType));

            foreach (var gp in method.GenericParameters)
                reference.GenericParameters.Add(new GenericParameter(gp.Name, reference));

            return reference;
        }

        public static FieldDefinition FindFieldInHierarchy(TypeDefinition type, string fieldName, int maxDepth = 16)
        {
            var cur = type;

            for (int i = 0; i < maxDepth && cur != null; i++)
            {
                var field = cur.Fields.FirstOrDefault(f => f.Name == fieldName);
                if (field != null)
                    return field;

                var baseRef = cur.BaseType;
                if (baseRef == null)
                    break;

                try
                {
                    cur = baseRef.Resolve();
                }
                catch
                {
                    break;  // 解析不到就停
                }
            }

            return null;
        }

        public static MethodDefinition FindMethodInHierarchy(TypeDefinition type, string methodName, int maxDepth = 16)
        {
            var cur = type;

            for (int i = 0; i < maxDepth && cur != null; i++)
            {
                var method = cur.Methods.FirstOrDefault(m => m.Name == methodName);
                if (method != null)
                    return method;

                var baseRef = cur.BaseType;
                if (baseRef == null)
                    break;

                try
                {
                    cur = baseRef.Resolve();
                }
                catch
                {
                    break;  // 解析不到就停
                }
            }

            return null;
        }

        /// <summary>
        /// 判斷是否為 XPlan.UI.ViewBase`1 泛型定義。
        /// </summary>
        public static bool TryFindViewBaseGeneric(TypeDefinition type, out TypeDefinition viewBaseType, string genericName, int maxDepth = 16)
        {
            viewBaseType    = null;
            var cur         = type;

            // 防止循環 也防止 Unity 特殊父類型亂跳
            for (int i = 0; i < maxDepth && cur != null; i++)
            {
                // 找到 XPlan.UI.ViewBase`1
                if (cur.FullName == genericName)
                {
                    viewBaseType = cur;
                    return true;
                }

                // 沒有父類別就離開
                if (cur.BaseType == null)
                    break;

                try
                {
                    cur = cur.BaseType.Resolve();
                }
                catch
                {
                    break;
                }
            }

            return false;
        }
    }
}