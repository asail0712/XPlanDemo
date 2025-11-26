using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

using UnityEngine;

using XPlan.Weaver.Abstractions;

namespace XPlan.Editors.Weaver
{
    /// <summary>
    /// 處理貼在欄位上的 [XPlan.I18N.I18NViewAttribute]：
    /// 1. 檢查欄位宣告類別是否為 MonoBehaviour 子類
    /// 2. 檢查欄位型別：Text / TMP or Image
    /// 3. 在該類別的 Awake() 開頭插入註冊呼叫
    /// </summary>
    internal sealed class I18NViewWeaver : IFieldAspectWeaver
    {
        // 欄位上的 Attribute
        public string AttributeFullName => "XPlan.I18NViewAttribute";

        public void Apply(ModuleDefinition module, FieldDefinition targetField, CustomAttribute attr)
        {
            var declaringType = targetField.DeclaringType;

            // 1) 檢查是否為 MonoBehaviour 子類
            if (!CecilHelper.IsMonoBehaviourSubclass(declaringType))
            {
                throw new InvalidOperationException(
                    $"[I18NViewWeaver] {declaringType.FullName} 不是 MonoBehaviour 子類，" +
                    $"欄位 {targetField.FullName} 不允許使用 [I18NViewAttribute]");
            }

            // 2) 檢查欄位型別，判斷是 Text / TMP 還是 Image
            var fieldTypeFullName   = targetField.FieldType.FullName;
            bool isTextLike         =
                                        fieldTypeFullName == "UnityEngine.UI.Text" ||
                                        fieldTypeFullName == "TMPro.TextMeshProUGUI";
            bool isImageLike        =
                                        fieldTypeFullName == "UnityEngine.UI.Image";

            if (!isTextLike && !isImageLike)
            {
                throw new InvalidOperationException(
                    $"[I18NViewWeaver] 欄位 {targetField.FullName} 被標記為 I18NView，" +
                    $"但型別 {fieldTypeFullName} 不是 Text/TMP/Image 之一");
            }

            // 3) 從 Attribute 讀取 key（假設 ctor 第一個參數就是 key）
            if (attr.ConstructorArguments.Count != 1)
            {
                throw new InvalidOperationException(
                    $"[I18NViewWeaver] {targetField.FullName} 的 I18NViewAttribute 構造參數數量錯誤（預期 1）");
            }

            var rawKey  = (string)attr.ConstructorArguments[0].Value;
            var key     = isTextLike ? "Key_" + rawKey : rawKey; // 視需求加前綴

            // 4) Import runtime 類別與方法
            // ⚠ I18NWeaverRuntime 要放在 Runtime 組件（不能是 Editor-only）
            var runtimeTypeRef = module.ImportReference(typeof(XPlan.Weaver.Runtime.I18NWeaverRuntime));
            var runtimeTypeDef = runtimeTypeRef.Resolve();

            MethodReference registerRef;

            if (isTextLike)
            {
                var registerTextDef = runtimeTypeDef.Methods.First(m =>
                    m.Name == "RegisterText" &&
                    m.Parameters.Count == 3);

                registerRef = module.ImportReference(registerTextDef);
            }
            else // isImageLike
            {
                var registerImageDef = runtimeTypeDef.Methods.First(m =>
                    m.Name == "RegisterImage" &&
                    m.Parameters.Count == 3);

                registerRef = module.ImportReference(registerImageDef);
            }

            // 5) 取得或建立 Start() 方法（非 static、無參數、void）
            // ViewBase有使用到Awake 因此改用Start避開
            var start = declaringType.Methods.FirstOrDefault(m =>
                m.Name == "Start" &&
                !m.IsStatic &&
                !m.HasParameters &&
                m.ReturnType.FullName == "System.Void");

            if (start == null)
            {
                start = new MethodDefinition(
                    "Start",
                    MethodAttributes.Public | MethodAttributes.HideBySig,
                    module.TypeSystem.Void);

                start.Body  = new MethodBody(start);
                var ilAwake = start.Body.GetILProcessor();
                ilAwake.Append(ilAwake.Create(OpCodes.Ret));

                declaringType.Methods.Add(start);
            }

            var il          = start.Body.GetILProcessor();
            var firstInstr  = start.Body.Instructions.FirstOrDefault();

            if (firstInstr == null)
            {
                // 理論上不會，因為上面有保底加 Ret
                firstInstr = il.Create(OpCodes.Ret);
                il.Append(firstInstr);
            }

            // 6) 在 Start 開頭插入註冊呼叫
            // C# 概念：
            // I18NWeaverRuntime.RegisterX(this, this.<field>, "Key_xxx");

            var ldThis1 = il.Create(OpCodes.Ldarg_0);      // this (view)
            var ldThis2 = il.Create(OpCodes.Ldarg_0);      // this (for ldfld)
            var ldfld   = il.Create(OpCodes.Ldfld, targetField);
            var ldKey   = il.Create(OpCodes.Ldstr, key);
            var call    = il.Create(OpCodes.Call, registerRef);

            il.InsertBefore(firstInstr, call);
            il.InsertBefore(call, ldKey);
            il.InsertBefore(ldKey, ldfld);
            il.InsertBefore(ldfld, ldThis2);
            il.InsertBefore(ldThis2, ldThis1);

            Debug.Log(
                $"[I18NViewWeaver] 織入完成：{declaringType.FullName}.{start.Name} → " +
                $"{(isTextLike ? "RegisterText" : "RegisterImage")} for {targetField.Name} (key={key})");
        }
    }
}
