using System.Collections.Concurrent;
using System.Collections.Generic;

namespace XPlan.Recycle
{
    public static class CacheManager
    {
        static ConcurrentDictionary<string, object> dataDict = new ConcurrentDictionary<string, object>();

        static public bool SaveToCache<T>(string key, T data, bool bForce = false)
        {
            if(dataDict.ContainsKey(key) && !bForce)
            {
                return false;
            }

            dataDict[key] = data;

            return true;
        }

        static public bool LoadFromCache<T>(string key, out T data)
        {
            if (!dataDict.ContainsKey(key))
            {
                data = default(T);
                return false;
            }

            if (dataDict[key] is T)
            {
                data = (T)dataDict[key];
                return true;
            }
            else
            {
                data = default(T);
                return false;
            }
        }

        static public void ClearCache()
        {
            dataDict.Clear();
        }

        static public void Remove(string key)
        {
            dataDict.Remove(key, out object dummy);
        }
    }
}
