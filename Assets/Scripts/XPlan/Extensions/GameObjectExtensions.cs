using UnityEngine;

namespace XPlan.Extensions
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

        public static void AttachChild(this GameObject gameObject, GameObject childGO
            , Vector3 locPos        = default(Vector3)
            , Vector3 eulerAngles   = default(Vector3))
		{
            if(childGO == null)
			{
                return;
			}

            childGO.transform.SetParent(gameObject.transform);
            childGO.transform.localPosition     = locPos;
            childGO.transform.localEulerAngles  = eulerAngles;
            childGO.transform.localScale        = Vector3.one;
        }
    }
}

