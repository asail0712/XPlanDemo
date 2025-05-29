using System;
using System.Collections.Generic;
using System.Linq;


namespace XPlan.Utility
{
	public static class ContainerExtension
	{
		public static bool IsValidIndex<T>(this List<T> list, int idx)
		{
			if (list == null)
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

		public static void AddUnique<T>(this List<T> list, T t)
		{
			if(!list.Contains(t))
            {
				list.Add(t);
			}
		}

		public static T First<T>(this List<T> list)
		{
			if (list == null || list.Count == 0)
			{
				return default(T);
			}

			return list[0];
		}

		public static T Last<T>(this List<T> list)
		{
			if(list == null || list.Count == 0)
            {
				return default(T);
            }

			return list[list.Count - 1];
		}

		public static List<T> Shuffled<T>(this List<T> list)
        {
			List<T> copy	= new List<T>(list);
			Random rng		= new Random(); // 只 new 一次
			int n			= copy.Count;

			while (n > 1)
			{
				n--;
				int k				= rng.Next(n + 1);
				(copy[n], copy[k])	= (copy[k], copy[n]); // swap
			}

			return copy;
		}
	}
}
