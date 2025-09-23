using System;
using UnityEngine;
using Random = UnityEngine.Random;
public static class IntConvert
{
    /// <summary>
    /// 从0到i-1
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public static int toCCRandomIndex(this int i)
    {
        return Random.Range(0, i);
    }

    //00:00:00格式
    public static string formatTimeHour(this int second)
    {
        if (second > 0)
        {
            int hours = second / (60 * 60);
            int minute = (second - hours * 60 * 60) / 60;
            int s = second - hours * 60 * 60 - minute * 60;
            return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minute, s);
        }
        else
        {
            return "00:00:00";
        }
    }
    
    public static string getTimeStringBySecond(this int second)
    {
        var s = second % 60;
        var m = second / 60 % 60;
        var h = second / 60 / 60;
        if (second < 60 * 60)
        {
            return $"{m:00}:{s:00}";
        }
        return $"{h}:{m:00}:{s:00}";
    }
}

public static class IC
{
    public static readonly int NotFound = -1;
}
