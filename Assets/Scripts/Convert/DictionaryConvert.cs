using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class DictionaryConvert
{
    public static List<B> getValuesList<A, B>(this Dictionary<A, B> dictionary)
    {
        return new List<B>(dictionary.Values);
    }

    public static B objectValue<A, B>(this Dictionary<A, B> dictionary, A key, B defaultValue = default(B))
    {
        if (dictionary == default(Dictionary<A, B>))
        {
            return defaultValue;
        }
        if (dictionary.TryGetValue(key, out B val))
        {
            return val;
        }
        return defaultValue;
    }

    public static Dictionary<string, object> dictionaryValue<A,B>(this Dictionary<A,B>dictionary, A key, Dictionary<string, object> defaultValue = null)
    {
        return dictionary.dictionaryValue<A,B,object>(key, defaultValue);
    }

    public static Dictionary<string, C> dictionaryValue<A,B,C>(this Dictionary<A,B>dictionary, A key, Dictionary<string, C> defaultValue = null)
    {
        if (dictionary == null) {
            return defaultValue;
        }

        return objectValue<A, B>(dictionary, key).toDictionary<C>(defaultValue);
    }

    public static List<object> listValue<A,B>(this Dictionary<A,B>dictionary, A key, List<object> defaultValue = null)
    {
        return dictionary.listValue<A,B,object>(key, defaultValue);
    }

    public static List<C> listValue<A,B,C>(this Dictionary<A,B>dictionary, A key, List<C> defaultValue = null)
    {
        if (dictionary == null) return defaultValue;
        return objectValue<A, B>(dictionary, key).toList<C>(defaultValue);
    }

    public static int intValue<A,B>(this Dictionary<A,B>dictionary, A key, int defaultValue = 0)
    {
        if (dictionary == null) return defaultValue;
        return objectValue<A, B>(dictionary, key).toInt(defaultValue);
    }

    public static long longValue<A,B>(this Dictionary<A,B>dictionary, A key, long defaultValue = 0L)
    {
        if (dictionary == null) return defaultValue;
        return objectValue<A, B>(dictionary, key).toLong(defaultValue);
    }

    public static float floatValue<A,B>(this Dictionary<A,B>dictionary, A key, float defaultValue = 0f)
    {
        if (dictionary == null) return defaultValue;
        return objectValue<A,B>(dictionary, key).toFloat(defaultValue);
    }

    public static double doubleValue<A, B>(this Dictionary<A, B> dictionary, A key, double defaultValue = 0f)
    {
        if (dictionary == null) return defaultValue;
        return objectValue<A, B>(dictionary, key).toDouble(defaultValue);
    }

    public static bool boolValue<A,B>(this Dictionary<A,B>dictionary, A key, bool defaultValue = false)
    {
        if (dictionary == null) return defaultValue;
        return objectValue<A, B>(dictionary, key).toBool(defaultValue);
    }

    public static string stringValue<A,B>(this Dictionary<A,B>dictionary, A key, string defaultValue = null)
    {
        if (dictionary == null) return defaultValue;
        return objectValue<A, B>(dictionary, key).toString(defaultValue);
    }

    public static bool containsKey<A,B>(this Dictionary<A,B>dictionary, A key)
    {
        return dictionary == null ? false : dictionary.ContainsKey(key);
    }

    public static List<int> intListValue(this Dictionary<string, object> dictionary, string key)
    {
        var list = new List<int>();
        var valueList = dictionary.listValue(key);
        if (valueList != null) {
            foreach (var item in valueList) {
                list.Add(item.toInt());
            }
        }
        return list;
    }

    public static List<float> floatListValue(this Dictionary<string, object> dictionary, string key)
    {
        var list = new List<float>();
        var valueList = dictionary.listValue(key);
        if (valueList != null) {
            foreach (var item in valueList) {
                list.Add(item.toFloat());
            }
        }
        return list;
    }

    public static List<double> doubleListValue(this Dictionary<string, object> dictionary, string key)
    {
        var list = new List<double>();
        var valueList = dictionary.listValue(key);
        if (valueList != null) {
            foreach (var item in valueList) {
                list.Add(item.toDouble());
            }
        }
        return list;
    }

    public static List<string> stringListValue(this Dictionary<string, object> dictionary, string key)
    {
        var list = new List<string>();
        var valueList = dictionary.listValue(key);
        if (valueList != null) {
            foreach (var item in valueList) {
                list.Add(item.toString());
            }
        }
        return list;
    }

    // public static List<T> objectListValue<T>(this Dictionary<string, object> dictionary, string key) where T : BaseConfigObject, new()
    // {
    //     var list = new List<T>();
    //     var valueList = dictionary.listValue(key);
    //     if (valueList != null) {
    //         foreach (var item in valueList) {
    //             var o = new T();
    //             o.initialize(item.toDictionary());
    //             list.Add(o);
    //         }
    //     }
    //     return list;
    // }

    // public static A objectData<A, B>(this Dictionary<string, object> dictionary, string key, Dictionary<int, B> configs, B defaultConfig) where A : UnitSimpleData<B>, new() where B : BaseConfigObject, new()
    // {
    //     var data = new A();
    //     var para = dictionary.dictionaryValue(key);
    //     if (para == null) {
    //         data.reloadData(null, defaultConfig);
    //         return data;
    //     }

    //     var config = configs.objectValue(para.intValue("id"));
    //     if (config == null) {
    //         data.reloadData(null, defaultConfig);
    //         return data;
    //     }

    //     data.reloadData(para, config);
    //     return data;
    // }

    // public static A objectConfig<A>(this Dictionary<string, object> dictionary, string key) where A : BaseConfigObject, new()
    // {
    //     var para = dictionary.dictionaryValue(key);
    //     var con = new A();
    //     if (para != null) {
    //         con.initialize(para);
    //     }
    //     else {
    //         con.initialize(new Dictionary<string, object>());
    //     }

    //     return con;
    // }

    public static List<List<int>> intListListValue(this Dictionary<string, object> dictionary, string key)
    {
        var list = new List<List<int>>();
        var valueList = dictionary.listValue(key);
        if (valueList != null)
        {
            foreach (var item in valueList)
            {
                var itemList = new List<int>();
                var intList = item.toList();
                foreach (var intItem in intList)
                {
                    itemList.Add(intItem.toInt());
                }
                list.Add(itemList);
            }
        }
        return list;
    }

    // public static List<List<T>> objectListListValue<T>(this Dictionary<string, object> dictionary, string key) where T : BaseConfigObject, new()
    // {
    //     var list = new List<List<T>>();
    //     var valueList = dictionary.listValue(key);
    //     if (valueList != null)
    //     {
    //         foreach (var item in valueList)
    //         {
    //             var itemList = new List<T>();
    //             var objList = item.toList();
    //             foreach (var objItem in objList)
    //             {
    //                 var o = new T();
    //                 o.initialize(objItem.toDictionary());
    //                 itemList.Add(o);
    //             }
    //             list.Add(itemList);
    //         }
    //     }
    //     return list;
    // }

    // public static Dictionary<int, T> objectDictValueByDict<T>(this Dictionary<string, object> dictionary, string key) where T : BaseConfigObject, new()
    // {
    //     var dict = new Dictionary<int, T>();
    //     var valueList = dictionary.dictionaryValue(key);
    //     if (valueList != null) {
    //         foreach (var item in valueList) {
    //             var o = new T();
    //             o.initialize(item.Key, item.Value.toDictionary());
    //             dict[o.id] = o;
    //         }
    //     }
    //     return dict;
    // }

    public static Dictionary<int, int> intDictValue(this Dictionary<string, object> dictionary, string key)
    {
        var intDict = new Dictionary<int, int>();
        var valueDict = dictionary.dictionaryValue(key);
        if (valueDict != null)
        {
            foreach (var item in valueDict)
            {
                intDict[item.Key.toInt()] = item.Value.toInt();
            }
        }
        return intDict;
    }

    public static Dictionary<string, int> stringIntDictValue(this Dictionary<string, object> dictionary, string key)
    {
        var strDict = new Dictionary<string, int>();
        var valueDict = dictionary.dictionaryValue(key);
        if (valueDict != null) {
            foreach (var item in valueDict) {
                strDict[item.Key] = item.Value.toInt();
            }
        }
        return strDict;
    }

    public static Dictionary<string, string> stringStringDictValue(this Dictionary<string, object> dictionary, string key)
    {
        var strDict = new Dictionary<string, string>();
        var valueDict = dictionary.dictionaryValue(key);
        if (valueDict != null) {
            foreach (var item in valueDict) {
                strDict[item.Key] = item.Value.toString();
            }
        }
        return strDict;
    }
}
