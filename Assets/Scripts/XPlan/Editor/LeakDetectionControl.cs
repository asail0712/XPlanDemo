using Unity.Collections;
using UnityEditor;

// 參考
// https://blog.csdn.net/weixin_42565127/article/details/125990221

namespace XPlan.Editors
{
    public static class LeakDetectionControl
    {
        [MenuItem("Jobs/Leak Detection/Enable")]
        private static void LeakDetection()
		{
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Enabled;
		}

        [MenuItem("Jobs/Leak Detection/With Stack Trace")]
        private static void LeakDetectionWithStackTrace()
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
        }

        [MenuItem("Jobs/Leak Detection/Disable")]
        private static void NoLeakDetection()
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Disabled;
        }
    }   
}