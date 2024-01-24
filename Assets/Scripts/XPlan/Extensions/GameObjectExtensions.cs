using UnityEngine;

namespace XPlan.Utility.Extensions
{
    public static class GameObjectExtensions
    {
        public static void ClearAllChildren(this GameObject gameObject, float delayTime = 0f)
		{
            // 取得父物件的Transform
            Transform parentTransform = gameObject.transform;

            // 逐個刪除子物件
            for (int i = parentTransform.childCount - 1; i >= 0; i--)
            {
                Transform childTransform = parentTransform.GetChild(i);

                if(delayTime == 0f)
				{
                    GameObject.DestroyImmediate(childTransform.gameObject);
                }
                else
				{
                    GameObject.Destroy(childTransform.gameObject, delayTime);
                }
            }
        }
    }
}

