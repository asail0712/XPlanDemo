using System;
using System.Reflection;

namespace XPlan.Utility
{
	public static class Tools
	{
        public static Type GetTypeInDomain(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);

                if (type != null)
                { 
                    return type;
                }
            }

            return null;
        }
    }
}
