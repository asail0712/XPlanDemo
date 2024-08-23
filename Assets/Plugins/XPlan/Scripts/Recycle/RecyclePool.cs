using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.DebugMode;

namespace XPlan.Recycle
{ 
    public class PoolInfo<T> where T: IPoolable, new()
    {
        private Queue<T> poolableQueue;
        private GameObject prefab;
        private int totalNum;
             
        public PoolInfo()
		{
            poolableQueue   = new Queue<T>();
            totalNum        = 0;
        }

        public void AddPrefab(GameObject prefab, int num)        
		{
            if (!typeof(PoolableComponent).IsAssignableFrom(typeof(T)))
            {
                return;
            }

            PoolableComponent dummy = null;

            if (!prefab.TryGetComponent<PoolableComponent>(out dummy))
			{
                return;
			}

            // 考慮到是monobehavior，生成方式會不一樣
            this.prefab     = prefab;
            this.totalNum   = num;

            for (int i = 0; i < num; ++i)
            {
                GameObject go   = GameObject.Instantiate(prefab);
                T comp          = go.GetComponent<T>();

                go.SetActive(false);
                poolableQueue.Enqueue(comp);
            }
        }

        public void AddPoolable(List<T> poolList)
        {
            for (int i = 0; i < poolList.Count; ++i)
            {
                poolableQueue.Enqueue(poolList[i]);
            }

            totalNum += poolList.Count;
        }

        public void ResetPool()
		{
			poolableQueue.Clear();
		}

		public T SpawnOne()
		{
            T poolable  = default(T);
            Type type   = typeof(T);

            // 若是type繼承PoolableComponent
            // 則判斷為GameObject做分別處理
            if (typeof(PoolableComponent).IsAssignableFrom(type))
            {
                if (poolableQueue.Count == 0)
                {
                    if (prefab == null)
                    {
                        LogSystem.Record($"backup 物件 為空，無法生成新GameObject !!", LogType.Error);
                    }
                    else
                    {
                        GameObject go   = GameObject.Instantiate(prefab);
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

        public void Recycle(T poolable)
		{
            poolable.OnRecycle();
            
            poolableQueue.Enqueue(poolable);
        }

        public int PoolNum()
        {
            return poolableQueue.Count;
        }

        public int TotalNum()
		{
            return totalNum;
        }
    }

    public static class RecyclePool<T> where T : IPoolable, new()
    {
        static private Dictionary<Type, PoolInfo<T>> poolInfoList = new Dictionary<Type, PoolInfo<T>>();

        /**************************************************
         * 生成流程
         * ************************************************/
        static public T SpawnOne()
		{
            Type type = typeof(T);

            if (!poolInfoList.ContainsKey(type))
            {
                return default(T);
            }

            return poolInfoList[type].SpawnOne();
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

        static public void Recycle(T something)
		{
            if(something == null)
			{
                return;
			}

            Type type = typeof(T);

            if (!poolInfoList.ContainsKey(type))
            {
                return;
            }

            poolInfoList[type].Recycle(something);
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
            Type type = typeof(T);

            if (poolInfoList.ContainsKey(type))
            {
                PoolInfo<T> poolInfo = poolInfoList[type];

                return poolInfo.TotalNum();
            }

            return 0;
        }

        static public int GetPoolNum()
        {
            Type type = typeof(T);

            if (poolInfoList.ContainsKey(type))
            {
                PoolInfo<T> poolInfo = poolInfoList[type];

                return poolInfo.PoolNum();
            }

            return 0;
        }

        /**************************************************
         * 註冊流程        
         * *************************************************/
        static public bool RegisterType(GameObject prefab, int num = 5)
        {
            Type type               = typeof(T);
            PoolInfo<T> poolInfo    = null;

            if (poolInfoList.ContainsKey(type))
            {
                poolInfo = poolInfoList[type];
            }
            else
            {
                poolInfo = new PoolInfo<T>();

                poolInfoList.Add(type, poolInfo);
            }

            poolInfo.AddPrefab(prefab, num);

            return true;
        }

        static public bool RegisterType(List<T> compList)
        {
            Type type = typeof(T);
            PoolInfo<T> poolInfo = null;

            if (poolInfoList.ContainsKey(type))
            {
                poolInfo = poolInfoList[type];
                poolInfo = poolInfoList[type];
            }
            else
            {
                poolInfoList.Add(type, poolInfo);
            }

            poolInfo.AddPoolable(compList);

            return true;
        }

        static public void UnregisterType()
        {
            Type type = typeof(T);

            if (!poolInfoList.ContainsKey(type))
            {
                return;
            }

            PoolInfo<T> poolInfo = poolInfoList[type];

            // 清空的目的是
            // 避免更換場景時，pool info裡面有被釋放的物件，導致null
            poolInfo.ResetPool();
        }

        static public void UnregisterAll()
        {
            List<Type> keyList = new List<Type>(poolInfoList.Keys);

            foreach (Type key in keyList)
			{
                UnregisterType();
            }
        }
    }
}