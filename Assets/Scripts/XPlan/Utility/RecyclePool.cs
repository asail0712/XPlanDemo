using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Utility;

namespace Granden.kmrt
{ 
    public class RecyclePool<T>
    {
        static public bool bEnabled = true;

        static private Dictionary<T, GameObject> backupList            = new Dictionary<T, GameObject>();
        static private Dictionary<T, List<GameObject>> gameObjectPool  = new Dictionary<T, List<GameObject>>();
        static private GameObject poolRoot;

        static public void SetRoot(GameObject root)
		{
            poolRoot = root;
        }

        /**************************************************
         * 生成流程
         * ************************************************/
        static public GameObject SpawnOne(T type)
		{
            if (!gameObjectPool.ContainsKey(type))
            {
                return null;
            }

            List<GameObject> goList = gameObjectPool[type];

            if(goList.Count == 0 || !bEnabled)
			{
                Debug.Log($"Pool {type}型別空了 所以生成一個新的 !!");

                return GameObject.Instantiate(backupList[type]);
            }

            GameObject go = goList[0];
            goList.RemoveAt(0);

            return go;
        }

        static public List<GameObject> SpawnList(T type, int num)
        {
            List<GameObject> result = new List<GameObject>();

            for(int i = 0; i < num; ++i)
			{
                result.Add(SpawnOne(type));
            }

            return result;
        }

        static public void DisposeOne(T type, GameObject go)
		{
            if (!gameObjectPool.ContainsKey(type))
            {
                return;
            }

            if(!bEnabled)
			{
                GameObject.DestroyImmediate(go);

                return;
			}

            if (null != poolRoot)
            {
                go.transform.parent = poolRoot.transform;
            }
            else
			{
                go.transform.parent = null;

            }

            List<GameObject> goList = gameObjectPool[type];

            goList.Add(go);
        }

        static public void DisposeList(T type, List<GameObject> goList)
        {
            for(int i = 0; i < goList.Count; ++i)
			{
                DisposeOne(type, goList[i]);
            }
        }

        /**************************************************
         * 註冊流程
         * ************************************************/
        static public bool RegisterType(T type, GameObject go, int maxNum = 5)
		{
            if(gameObjectPool.ContainsKey(type))
			{
                return false;
			}

            List<GameObject> goList = new List<GameObject>();

            for (int i = 0; i < maxNum; ++i)
			{
                GameObject duplicateGO          = GameObject.Instantiate(go);
                duplicateGO.transform.position  = new Vector3(99999f, 0f, 0f);

                if (null != poolRoot)
				{
                    duplicateGO.transform.parent = poolRoot.transform;
                }
                
                goList.Add(duplicateGO);
			}

            backupList.Add(type, go);
            gameObjectPool.Add(type, goList);

            return true;
		}

        static public void UnregisterType(T type)
        {
            if (!gameObjectPool.ContainsKey(type))
            {
                return;
            }

            List<GameObject> goList = gameObjectPool[type];

            for (int i = 0; i < goList.Count; ++i)
            {
                GameObject.Destroy(goList[i]);
            }

            backupList.Remove(type);
            gameObjectPool.Remove(type);
        }

        static public void UnregisterAll()
        {
            foreach(var kvp in gameObjectPool)
			{
                List<GameObject> goList = gameObjectPool[kvp.Key];

                for (int i = 0; i < goList.Count; ++i)
                {
                    GameObject.Destroy(goList[i]);
                }
            }

            backupList.Clear();
            gameObjectPool.Clear();
        }
    }
}