using UnityEngine;

namespace XPlan
{
    public class ConsoleLogger
    {
        public void Before(string methodName)
        {
            Debug.Log($"[Begin foo] {methodName}");
        }

        public void After(string methodName, long elapsedMs)
        {
            Debug.Log($"[Finish foo]  {methodName} 花費 {elapsedMs} ms");
        }
    }
}
