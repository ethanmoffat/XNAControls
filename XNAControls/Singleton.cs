using System;
using System.Collections.Generic;

namespace XNAControls
{
    internal class Singleton<T> : Singleton where T : class
    {
        public static T Instance => (T)_typeMap[typeof(T)];

        public static void Map(T instance)
        {
            var typeKey = typeof(T);
            if (_typeMap.ContainsKey(typeKey))
                _typeMap.Remove(typeKey);
            _typeMap.Add(typeKey, instance);
        }

        public static void MapIfMissing(T instance)
        {
            if (!_typeMap.ContainsKey(typeof(T)))
                Map(instance);
        }
    }

    internal class Singleton<T, U> : Singleton where U : class, T, new()
    {
        public static void Map()
        {
            Map(new U());
        }

        internal static void Map(U instance)
        {
            var typeKey = typeof(T);
            if (_typeMap.ContainsKey(typeKey))
                _typeMap.Remove(typeKey);
            _typeMap.Add(typeKey, instance);
        }
    }

    internal class Singleton
    {
        protected static readonly Dictionary<Type, object> _typeMap = new Dictionary<Type, object>();
    }
}
