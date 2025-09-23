using System;
using UnityEngine;
using Random = UnityEngine.Random;

public static class FloatConvert
{
    public static float toCCRandom01(this float f)
    {
        return f * Random.Range(0f, 1f);
    }

    /// <summary>
    /// 线性映射，将oXY从origin区间映射到target区间
    /// </summary>
    /// <param name="defaultValue">origin区间无效，则返回该值</param>
    /// <returns></returns>
    public static float ReMapNumber(this float oXY, float originMin, float originMax, float targetMin, float targetMax, float defaultValue = 1)
    {
        if (Math.Abs(originMax - originMin) < float.Epsilon || originMin > originMax)
        {
            return defaultValue;
        }
        float result = 0;
        result = (targetMax - targetMin) / (originMax - originMin) * (oXY - originMin) + targetMin;
        return result;
    }
}
