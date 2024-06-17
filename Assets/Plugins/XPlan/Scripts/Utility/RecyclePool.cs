using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Utility
{ 
    public class PoolInfo
	{
        public int refCount = 0;

        private List<GameObject> duplicateList;
        private GameObject backUpObj = null;
        private GameObject poolRoot;

        public PoolInfo()
		{
            duplicateList = new List<GameObject>();
        }

        public void InitialInfo(GameObject obj, int maxNum, GameObject poolRoot)
		{
            this.poolRoot   = poolRoot;
            this.backUpObj  = obj;
            this.refCount   = 1;

            for (int i = 0; i < maxNum; ++i)
            {
                GameObject duplicateObj         = GameObject.Instantiate(obj);
                duplicateObj.transform.position = new Vector3(99999f, 0f, 0f);

                if (null != poolRoot)
                {
                    duplicateObj.transform.parent = poolRoot.transform;
                }

                duplicateList.Add(duplicateObj);
            }
        }

        public void ReleaseInfo()
		{
            foreach(GameObject obj in duplicateList)
			{
                GameObject.Destroy(obj);
            }

            duplicateList.Clear();

            refCount    = 0;
            poolRoot    = null;
            backUpObj   = null;
        }

        public void ResetPool()
		{
            foreach (GameObject obj in duplicateList)
            {
                GameObject.Destroy(obj);
            }

            duplicateList.Clear();
        }

        public GameObject SpawnOne(bool bEnabled)
		{
            if (duplicateList.Count == 0 || !bEnabled)
            {
                //Debug.Log($"Pool {type}型別空了 所以生成一個新的 !!");

                return GameObject.Instantiate(backUpObj);
            }

            GameObject go = duplicateList[0];
            duplicateList.RemoveAt(0);

            return go;
        }

        public void DisponeOne(GameObject obj, bool bEnabled)
		{
            if (!bEnabled)
            {
                GameObject.DestroyImmediate(obj);

                return;
            }

            if (null != poolRoot)
            {
                obj.transform.parent = poolRoot.transform;
            }
            else
            {
                obj.transform.parent = null;
            }

            duplicateList.Add(obj);
        }
    }

    public class RecyclePool<T>
    {
        static public bool bEnabled = true;

        static private Dictionary<T, PoolInfo> poolInfoList = new Dictionary<T, PoolInfo>();
        static private GameObject poolRoot;

        static public void SetRoot(GameObject root)
		{
            poolRoot = root;
        }

        /**************************************************
         * 生成流程
         * ************************************************/
        static public GameObject SpawnOne(T type, Action<GameObject, GameObject> afterSpawn = null)
		{
            if (!poolInfoList.ContainsKey(type))
            {
                return null;
            }

            return poolInfoList[type].SpawnOne(bEnabled);
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

        static public void DisposeOne(T type, GameObject go, Action<GameObject> afterDispose = null)
		{
            if (!poolInfoList.ContainsKey(type))
            {
                return;
            }

            poolInfoList[type].DisponeOne(go, bEnabled);

            afterDispose?.Invoke(go);
        }

        static public void DisposeList(T type, List<GameObject> goList, Action<GameObject> afterDispose = null)
        {
            for(int i = 0; i < goList.Count; ++i)
			{
                DisposeOne(type, goList[i], afterDispose);
            }
        }

        /**************************************************
         * 註冊流程
         * ************************************************/
        static public bool RegisterType(T type, GameObject go, int maxNum = 5)
		{
            if(poolInfoList.ContainsKey(type))
			{
                // 重複的話，ref count 加1
                PoolInfo poolInfo = poolInfoList[type];                
                poolInfo.refCount++;
            }
			else 
            {
                PoolInfo poolInfo = new PoolInfo();
                poolInfo.InitialInfo(go, maxNum, poolRoot);

                poolInfoList.Add(type, poolInfo);
            }            

            return true;
		}

        static public void UnregisterType(T type, bool bForce = false)
        {
            if (!poolInfoList.ContainsKey(type))
            {
                return;
            }

            PoolInfo poolInfo = poolInfoList[type];
            --poolInfo.refCount;

            if (poolInfo.refCount == 0 || bForce)
			{
                poolInfo.ReleaseInfo();
                poolInfoList.Remove(type);
            }
            else
			{
                // 清空的目的是
                // 避免更換場景時，pool info裡面有被釋放的物件，導致null
                poolInfo.ResetPool();
            }
        }

        static public void UnregisterAll()
        {
            List<T> keyList = new List<T>(poolInfoList.Keys);

            foreach (T key in keyList)
			{
                UnregisterType(key, true);
            }
        }
    }
}