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
#endif //WEAVING_ENABLE

namespace XPlan.Weaver.Abstractions
{
    /****************************************
    * 切面介面：所有級別的 Aspect 都實作它們
    ****************************************/
    // 方法只會掃有Attribute的方法
    public interface IMethodAspectWeaver
    {
        string AttributeFullName { get; }
#if WEAVING_ENABLE
        void Apply(ModuleDefinition module, MethodDefinition targetMethod, CustomAttribute attr);
#endif // WEAVING_ENABLE
    }

    // 類型會掃所有類型
    public interface ITypeAspectWeaver
    {
        string AttributeFullName { get; }
#if WEAVING_ENABLE
        void Apply(ModuleDefinition module, TypeDefinition targetType, CustomAttribute attr);
#endif // WEAVING_ENABLE
    }

    // Field只會掃有Attribute的Field
    public interface IFieldAspectWeaver
    {
        string AttributeFullName { get; }
#if WEAVING_ENABLE
        void Apply(ModuleDefinition module, FieldDefinition targetField, CustomAttribute attr);
#endif // WEAVING_ENABLE
    }
}
