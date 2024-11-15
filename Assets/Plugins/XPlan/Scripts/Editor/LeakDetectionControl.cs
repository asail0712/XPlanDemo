using Unity.Collections;
using UnityEditor;

// 參考
// https://blog.csdn.net/weixin_42565127/article/details/125990221

namespace XPlan.Editors
{
    public static class LeakDetectionControl
    {
        [MenuItem("XPlanTools/Leak Detection/Enable")]
        private static void LeakDetection()
		{
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Enabled;
		}

        [MenuItem("XPlanTools/Leak Detection/With Stack Trace")]
        private static void LeakDetectionWithStackTrace()
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
        }

        [MenuItem("XPlanTools/Leak Detection/Disable")]
        private static void NoLeakDetection()
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Disabled;
        }
    }   
}