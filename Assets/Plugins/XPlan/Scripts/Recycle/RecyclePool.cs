﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XPlan.DebugMode;

namespace XPlan.Recycle
{ 
    public class PoolInfo<T> where T: IPoolable, new()
    {
        private Queue<T> poolableQueue;
        private PoolableComponent backupComp;
             
        public PoolInfo()
		{
            poolableQueue   = new Queue<T>();
            backupComp      = null;
        }

        public void AddPoolable(List<T> poolList)
		{
            // 考慮到是monobehavior，生成方式會不一樣，所以要backup
            if(poolList.Count > 0 && typeof(T) == typeof(PoolableComponent))
			{
                backupComp = poolList[0] as PoolableComponent;
            }

            for (int i = 0; i < poolList.Count; ++i)
            {
                poolableQueue.Enqueue(poolList[i]);
            }
        }

		public void ResetPool()
		{
			poolableQueue.Clear();
		}

		public T SpawnOne()
		{
            T poolable  = default(T);
            Type type   = typeof(T);

            // 判斷是否為GameObject做分別處理
            if (type == typeof(PoolableComponent))
            {
                if (poolableQueue.Count == 0)
                {
                    if (backupComp.gameObject == null)
                    {
                        LogSystem.Record($"backup 物件 為空，無法生成新GameObject !!", LogType.Error);
                    }
                    else
                    {
                        GameObject go   = GameObject.Instantiate(backupComp.gameObject);
                        poolable        = go.GetComponent<T>();
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
         * 註冊流程        
         * *************************************************/      

        static public bool RegisterType(List<T> compList)
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