// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;

namespace XNAControls
{
    internal class Singleton<T> : Singleton where T : class, new()
    {
        public static T Instance
        {
            get
            {
                var typeKey = typeof(T);

                if(!_typeMap.ContainsKey(typeKey) || _typeMap[typeKey] == null)
                    _typeMap.Add(typeKey, new T());

                return (T)_typeMap[typeKey];
            }
        }
    }

    internal class Singleton
    {
        protected static readonly Dictionary<Type, object> _typeMap = new Dictionary<Type, object>();
    }
}
