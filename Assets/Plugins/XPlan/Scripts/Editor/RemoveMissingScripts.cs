using UnityEditor;
using UnityEngine;

namespace XPlan.Editors
{
    public class RemoveMissingScripts : MonoBehaviour
    {
        [MenuItem("Tools/Remove Missing Scripts")]
        private static void RemoveMissingScriptsFromSelected()
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            }
        }
    }
}