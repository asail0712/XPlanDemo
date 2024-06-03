using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XPlan.Extensions
{
    public static class ContainerExtension
    {
        public static bool IsValidIndex<T>(this List<T> list, int idx)
		{
			if(list == null)
			{
				return false;
			}

            return idx >= 0 && idx < list.Count;
		}

        public static U FindOrAdd<T, U>(this Dictionary<T, U> dict, T key) where U : new()
		{
			if (!dict.ContainsKey(key))
			{
				dict[key] = new U();
			}

			U u = dict[key];

			return u;
		}
    }
}
