using System;
using UnityEngine;

namespace XPlan
{
    // 標記此欄位可與ViewModel成員繫結（預設由欄位名推導）
    // 名稱為 BindName
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class BindNameAttribute : Attribute
    {
        public string Name { get; }
        public BindNameAttribute(string name) => Name = name;
    }
}
