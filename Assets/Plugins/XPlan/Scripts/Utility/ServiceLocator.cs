using System;
using System.Collections.Generic;

namespace XPlan.Utility
{
    /// <summary>
    /// 提供簡易服務定位器功能，支持實例註冊和延遲單例註冊。
    /// </summary>
    public static class ServiceLocator
    {
        // 儲存已註冊且已創建的服務實例（即時單例或手動實例）
        private static readonly Dictionary<Type, object> _registeredInstances       = new Dictionary<Type, object>();

        // 儲存用於延遲創建服務的工廠函數 (Lazy Singleton Factories)
        private static readonly Dictionary<Type, Func<object>> _registeredFactories = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// 註冊一個已存在的實例作為服務。
        /// </summary>
        /// <typeparam name="T">服務的類型/介面。</typeparam>
        /// <param name="service">要註冊的服務實例。</param>
        public static void Register<T>(T service)
            where T : class
        {
            Type type = typeof(T);

            if (_registeredInstances.ContainsKey(type) || _registeredFactories.ContainsKey(type))
            {
                throw new ArgumentException($"Service [{type}] already registered.");
            }

            _registeredInstances.Add(type, service);
        }

        /// <summary>
        /// 註冊一個單例工廠函數。實例將在第一次 GetService 呼叫時創建 (延遲初始化)。
        /// </summary>
        /// <typeparam name="T">服務的類型/介面。</typeparam>
        /// <param name="serviceFactory">創建服務實例的工廠函數。</param>
        public static void RegisterSingleton<T>(Func<T> serviceFactory)
            where T : class
        {
            Type type = typeof(T);

            if (_registeredInstances.ContainsKey(type) || _registeredFactories.ContainsKey(type))
            {
                throw new ArgumentException($"Service [{type}] already registered.");
            }

            // 將工廠函數儲存起來，等待第一次 GetService 時執行
            _registeredFactories.Add(type, () => serviceFactory.Invoke());
        }

        /// <summary>
        /// 解析並獲取服務實例。如果是延遲單例，將在此處創建實例。
        /// </summary>
        /// <typeparam name="T">服務的類型/介面。</typeparam>
        /// <returns>服務的實例。</returns>
        public static T GetService<T>()
            where T : class
        {
            Type type = typeof(T);

            // 1. 嘗試從已解析的實例中獲取 (已創建的實例或已解析的單例)
            if (_registeredInstances.ContainsKey(type))
            {
                return (T)_registeredInstances[type];
            }

            // 2. 嘗試從工廠中獲取 (延遲單例 - 第一次解析)
            if (_registeredFactories.ContainsKey(type))
            {
                // 執行工廠函數創建實例
                Func<object> factory    = _registeredFactories[type];
                T instance              = (T)factory.Invoke();

                // 將實例從工廠字典移動到實例字典，確保後續呼叫直接返回該實例
                _registeredInstances.Add(type, instance);
                _registeredFactories.Remove(type);

                return instance;
            }

            throw new KeyNotFoundException($"Service type [{type}] not found.");
        }

        /// <summary>
        /// 註銷指定的服務類型。
        /// </summary>
        /// <typeparam name="T">要註銷的服務類型。</typeparam>
        public static void Deregister<T>()
            where T : class
        {
            Type type = typeof(T);
            _registeredInstances.Remove(type);
            _registeredFactories.Remove(type);
        }

        /// <summary>
        /// 註銷所有已註冊的服務和工廠。
        /// </summary>
        public static void DeregisterAll()
        {
            _registeredInstances.Clear();
            _registeredFactories.Clear();
        }

        /// <summary>
        /// 檢查服務是否已註冊 (無論是實例還是工廠)。
        /// </summary>
        /// <typeparam name="T">服務的類型/介面。</typeparam>
        /// <returns>如果服務已註冊，則為 true；否則為 false。</returns>
        public static bool HasService<T>()
            where T : class
        {
            Type type = typeof(T);
            return _registeredInstances.ContainsKey(type) || _registeredFactories.ContainsKey(type);
        }
    }
}