using System;
using System.Collections.Generic;

namespace XPlan.Utility
{
    public static class ServiceLocator
    {
        private static Dictionary<Type, object> services;
        
        static ServiceLocator()
        {
            services = new Dictionary<Type, object>();
        }

        public static void Register<T>(T service)
            where T : class
        {
            if (services.ContainsKey(typeof(T)))
            {
                throw new ArgumentException($"Service [{typeof(T)}] already existed.");
            }

            services.Add(typeof(T), service);
        }

        public static void Register<T, U>()
            where T : class
            where U : class, new()
        {
            if (services.ContainsKey(typeof(T)))
            {
                throw new ArgumentException($"Service [{typeof(T)}] already existed.");
            }

            services.Add(typeof(T), new U());
        }

        public static void RegisterSingleton<T>(Func<T> serviceFactory)
            where T : class
        {
            Type type = typeof(T);

            if (!services.ContainsKey(type))
            {
                services.Add(type, serviceFactory.Invoke());
            }
        }

        public static void Deregister<T>()
            where T : class
        {
            services.Remove(typeof(T));
        }

        public static void DeregisterAll()
        {
            services.Clear();
        }

        public static T GetService<T>()
            where T : class
        {
            if (!services.ContainsKey(typeof(T)))
            {
                throw new KeyNotFoundException($"Service type [{typeof(T)}] not found.");
            }

            return (T)services[typeof(T)];
        }

		public static bool HasService<T>()
	        where T : class
		{
            return services.ContainsKey(typeof(T));
		}
	}
}