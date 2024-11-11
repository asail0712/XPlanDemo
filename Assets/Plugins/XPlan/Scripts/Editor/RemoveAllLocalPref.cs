using UnityEditor;
using UnityEngine;

using XPlan.Utility;

namespace XPlan.Editors
{
    public class RemoveAllLocalPrefs : MonoBehaviour
    {
        [MenuItem("XPlanTools/Remove All LocalPrefs")]
        private static void RemoveLocalPref()
        {
            LocalPref.ClearAll();
        }
    }
}