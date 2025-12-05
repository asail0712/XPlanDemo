using System;

namespace XPlan
{
    /// <summary>
    /// 標記：這個 ViewModel Method 要被當成「按鈕點擊」處理。
    /// 預設用方法名稱推導 Button 欄位名稱，例如：
    ///   OnDemoTriggerClick → demoTriggerBtn
    /// 如果不想用命名規則，也可以在建構子給 ButtonName。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class ViewBindingAttribute : Attribute
    {
        public ViewBindingAttribute()
        {
        }
    }
}