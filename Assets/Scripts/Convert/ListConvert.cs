using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public static class ListConvert
{
    public static T objectValue<T>(this List<T>list, int index)
    {
        if (list == default(List<T>)) return default(T);
        if (index >= 0 && index < list.Count) {
            return list[index];
        }
        return default(T);
    }
    /// <summary>
    /// 统计符合条件的元素个数
    /// </summary>
    /// <typeparam name="T">需要查找的数据类型</typeparam>
    /// <param name="list"></param>
    /// <param name="prediction">判断函数</param>
    /// <returns></returns>
    public static int countItem<T>(this List<T> list,Func<T,bool> prediction)
    {
        if (prediction == null || list == null) return IC.NotFound;
        int ret = 0;
        foreach (var item in list)
        {
            if (prediction(item)) ret++;
        }
        return ret;
    }
    
    public static void shuffle<T>(this IList<T> list,int startIndex = 0,int endIndex = int.MaxValue)
    {
        int n = Mathf.Min(endIndex, list.Count);
        while (n > startIndex + 1) {
            int k = startIndex + (n - startIndex).toCCRandomIndex();
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static T getRandomOne<T>(this List<T> thisList, bool remove = false)
    {
        if (thisList.Count == 0)
        {
            return default(T);
        }
        int index = thisList.Count.toCCRandomIndex();
        var ret = thisList[index];
        if (remove) thisList.RemoveAt(index);
        return ret;
    }

    public static T getRandomOneByRemove<T>(this List<T> thisList)
    {
        return thisList.getRandomOne<T>(true);
    }
}
