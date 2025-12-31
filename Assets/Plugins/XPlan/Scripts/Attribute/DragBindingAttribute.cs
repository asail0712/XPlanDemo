using System;

namespace XPlan
{
    /// <summary>
    /// 標記：這個 ViewModel Method 要被當成「按鈕點擊」處理。
    /// 預設用方法名稱推導 Button 欄位名稱，例如：
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class DragBindingAttribute : Attribute
    {
        public DragPhase Phase { get; }
        public DragBindingAttribute(DragPhase phase)
        {
            Phase = phase;
        }
    }
}