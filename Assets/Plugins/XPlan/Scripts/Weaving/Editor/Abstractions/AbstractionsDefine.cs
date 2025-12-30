using Mono.Cecil;
using UnityEngine;

namespace XPlan.Weaver.Abstractions
{
    /****************************************
    * 切面介面：所有級別的 Aspect 都實作它們
    ****************************************/
    // 方法只會掃有Attribute的方法
    public interface IMethodAspectWeaver
    {
        string AttributeFullName { get; }

        void Apply(ModuleDefinition module, MethodDefinition targetMethod, CustomAttribute attr);
    }

    // 類型會掃所有類型
    public interface ITypeAspectWeaver
    {
        string AttributeFullName { get; }
        void Apply(ModuleDefinition module, TypeDefinition targetType, CustomAttribute attr);
    }

    // Field只會掃有Attribute的Field
    public interface IFieldAspectWeaver
    {
        string AttributeFullName { get; }
        void Apply(ModuleDefinition module, FieldDefinition targetField, CustomAttribute attr);
    }
}
