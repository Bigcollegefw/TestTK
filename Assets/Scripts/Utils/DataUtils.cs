using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataUtils : SingletonData<DataUtils>
{
    Dictionary<string, Type> _types;
    protected override void OnInit()
    {
        _types = new Dictionary<string, Type>();
    }

    public Dictionary<string, object> popDict()
    {
        return new Dictionary<string, object>();
    }
    Type getTypeByName(string name)
    {
        Type o;
        if (_types.TryGetValue(name, out o))
        {
            return o;
        }
        Type type = Type.GetType(name);
        if (type == null)
        {
            return null;
        }
        _types[name] = type;
        return type;
    }
    public T getActivator<T>(string key = default) where T : class
    {
        return this.getActivator<T>(this.getTypeByName(key.IsNullOrEmpty() ? typeof(T).toString() : key));
    }
    public T getActivator<T>(Type t) where T : class
    {
        //        return getActivator<T>(t, null, new object[1]{ null });
        return getActivator<T>(t, null, null);
    }
    public T getActivator<T>(Type t, Type[] pts, object[] os) where T : class
    {
        return Activator.CreateInstance(t, os) as T;
    }
}
