using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.DebugMode;

namespace XPlan.Recycle
{ 
    public static class RecyclePool<T> where T : IPoolable, new()
    {
        static private Queue<T> poolableQueue   = new Queue<T>();
        static private int totalNum             = 0;
        static private GameObject prefabInstance;

        /**************************************************
         * 生成流程
         * ************************************************/
        static public T SpawnOne()
		{
            T poolable  = default(T);
            Type type   = typeof(T);

            // 若是type繼承PoolableComponent
            // 則判斷為GameObject做分別處理
            if (typeof(PoolableComponent).IsAssignableFrom(type))
            {
                if (poolableQueue.Count == 0)
                {
                    if (prefabInstance == null)
                    {
                        LogSystem.Record($"backup 物件 為空，無法生成新GameObject !!", LogType.Error);
                    }
                    else
                    {
                        GameObject go   = GameObject.Instantiate(prefabInstance);
                        poolable        = go.GetComponent<T>();

                        ++totalNum;
                    }
                }
                else
                {
                    poolable = poolableQueue.Dequeue();
                }
            }
            else
            {
                if (poolableQueue.Count == 0)
                {
                    LogSystem.Record($"Pool {type} 型別空了 所以生成一個新的 !!", LogType.Warning);

                    poolable = new T();
                    poolable.InitialPoolable();

                    ++totalNum;
                }
                else
                {
                    poolable = poolableQueue.Dequeue();
                }
            }

            poolable.OnSpawn();

            return poolable;
        }

        static public List<T> SpawnList(int num)
        {
            List<T> result = new List<T>();

            for(int i = 0; i < num; ++i)
			{
                result.Add(SpawnOne());
            }

            return result;
        }

        static public void Recycle(T poolable)
		{
            poolable.OnRecycle();

            poolableQueue.Enqueue(poolable);
        }

        static public void RecycleList(List<T> goList)
        {
            for(int i = 0; i < goList.Count; ++i)
			{
                Recycle(goList[i]);
            }
        }
        /**************************************************
         * 其他
         * *************************************************/
        static public int GetTotalNum()
		{
            return totalNum;
        }

        static public int GetPoolNum()
        {
            return poolableQueue.Count;
        }

        /**************************************************
         * 註冊流程        
         * *************************************************/
        static public bool RegisterType(GameObject prefab, int num = 5, GameObject poolRoot = null)
        {
            if (!typeof(PoolableComponent).IsAssignableFrom(typeof(T)))
            {
                return false;
            }

            PoolableComponent dummy = null;

            if (!prefab.TryGetComponent<PoolableComponent>(out dummy))
            {
                return false;
            }

            // 考慮到是monobehavior，生成方式會不一樣
            prefabInstance  = prefab;
            totalNum        = num;

            for (int i = 0; i < num; ++i)
            {
                GameObject go       = GameObject.Instantiate(prefab);
                T comp              = go.GetComponent<T>();
                go.transform.parent = poolRoot.transform;

                go.SetActive(false);                
                poolableQueue.Enqueue(comp);
            }

            return true;
        }

        static public bool RegisterType(List<T> poolList)
        {
            for (int i = 0; i < poolList.Count; ++i)
            {
                poolableQueue.Enqueue(poolList[i]);
            }

            totalNum += poolList.Count;

            return true;
        }

        static public void UnregisterType()
        {
            while (poolableQueue.Count > 0)
            {
                T poolable = poolableQueue.Dequeue();

                if(poolable is PoolableComponent)
				{
                    PoolableComponent poolableComp = poolable as PoolableComponent;

                    poolableComp.ReleasePoolable();                    
                }
            }

            poolableQueue.Clear();
        }
    }
}