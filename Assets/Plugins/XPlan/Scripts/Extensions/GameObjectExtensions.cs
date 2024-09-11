using System;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Extensions
{
    public static class GameObjectExtensions
    {
        public static List<GameObject> GetAllChildren(this GameObject gameObject, Func<GameObject, bool> filter = null)
		{
            List<GameObject> allChildren = new List<GameObject>();

            // 遞迴函數，遍歷子物件
            void GetChildrenRecursive(Transform parent)
            {
                foreach (Transform child in parent)
                {
                    // 只有符合過濾條件的子物件才會被加入列表
                    if (filter == null || filter(child.gameObject))
                    {
                        allChildren.Add(child.gameObject);
                    }

                    // 遞迴調用，檢查這個子物件的子物件
                    GetChildrenRecursive(child);
                }
            }

            // 從傳入的 GameObject 開始遞迴
            GetChildrenRecursive(gameObject.transform);

            return allChildren;
        }

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

        public static void AddChild(this GameObject gameObject, GameObject childGO)
        {
            if (childGO == null)
            {
                return;
            }

            childGO.transform.SetParent(gameObject.transform);
        }

        public static void AddChild(this GameObject gameObject, GameObject childGO
            , Vector3 locPos        = default(Vector3)
            , Vector3 eulerAngles   = default(Vector3)
            , float ratio           = 1.0f)
		{
            if(childGO == null)
			{
                return;
			}

            childGO.transform.SetParent(gameObject.transform);
            childGO.transform.localPosition     = locPos;
			childGO.transform.localEulerAngles  = eulerAngles;
			childGO.transform.localScale        = new Vector3(ratio, ratio, ratio);
        }

        public static void SetLayer(this GameObject gameObject, int layer, bool bContainChild = true)
        {
            gameObject.layer = layer;

            if(bContainChild)
			{
                int count = gameObject.transform.childCount;

                for(int i = 0; i < count; ++i)
				{
                    SetLayer(gameObject.transform.GetChild(i).gameObject, layer, bContainChild);
                }
            }
        }
    }
}

