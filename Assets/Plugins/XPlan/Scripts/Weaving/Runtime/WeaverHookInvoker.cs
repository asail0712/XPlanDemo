using System.Reflection;

namespace XPlan.Weaver.Runtime
{
    /// <summary>
    /// 給 IL Weaving 的 Hook 入口：
    /// 在衍生類別中產生一個 Hook 方法
    /// 就會被這裡自動呼叫
    /// </summary>
    public static class WeaverHookInvoker
    {
        public static void Invoke(object target, string hookName)
        {
            if (target == null) return;

            var type    = target.GetType();
            var method  = type.GetMethod(
                            hookName,
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (method == null)
                return;

            if (method.ReturnType != typeof(void))
                return;

            if (method.GetParameters().Length != 0)
                return;

            method.Invoke(target, null);
        }
    }
}