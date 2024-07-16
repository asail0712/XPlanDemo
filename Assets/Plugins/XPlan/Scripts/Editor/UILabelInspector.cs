using UnityEditor;

using XPlan.UI.Component;

namespace XPlan.Editors
{
    [CustomEditor(typeof(UILabel), false)]
    public class UILabelInspector : Editor
    {
        private SerializedProperty typeNameValue;

		private void OnEnable()
		{
			typeNameValue = serializedObject.FindProperty("labelIdx");
		}
	}
}

