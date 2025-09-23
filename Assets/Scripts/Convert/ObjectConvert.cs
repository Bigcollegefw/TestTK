using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class ObjectConvert
{
    public static CultureInfo EnCultureInfo = new CultureInfo("en");
    public static Dictionary<int, string> MPDict = new Dictionary<int, string>(){
        {3, "K"},
        {6, "M"},
        {9, "B"},
        {12, "T"},
        {15, "P"},
        {18, "E"},
        {21, "Z"},
        {24, "Y"},
    };

    public static int toInt(this object obj, int defaultValue = 0)
    {
        return  obj == null ? defaultValue : Convert.ToInt32(obj, EnCultureInfo);
    }

    public static long toLong(this object obj, long defaultValue = 0L)
    {
        return  obj == null ? defaultValue : Convert.ToInt64(obj, EnCultureInfo);
    }

    public static float toFloat(this object obj, float defaultValue = 0f)
    {
        return obj == null ? defaultValue : (float)Convert.ToDouble(obj, EnCultureInfo);
    }

    public static double toDouble(this object obj, double defaultValue = 0)
    {
        return obj == null ? defaultValue : Convert.ToDouble(obj, EnCultureInfo);
    }

    public static bool toBool(this object obj, bool defaultValue = false)
    {
        return obj == null ? defaultValue : Convert.ToBoolean(obj, EnCultureInfo);
    }

    public static string toString(this object obj, string defaultValue = null)
    {
        return obj == null ? defaultValue : Convert.ToString(obj, EnCultureInfo);
    }

    public static Dictionary<string,T> toDictionary<T>(this object obj, Dictionary<string,T> defaultValue = null)
    {
        return obj == null ? defaultValue : obj as Dictionary<string,T>;
    }

    public static Dictionary<string,object> toDictionary(this object obj, Dictionary<string,object> defaultValue = null)
    {
        return obj.toDictionary<object>(defaultValue);
    }

    public static List<T> toList<T>(this object obj, List<T> defaultValue = null)
    {
        return obj == null ? defaultValue : obj as List<T>;
    }

    public static List<object> toList(this object obj, List<object> defaultValue = null)
    {
        return obj.toList<object>(defaultValue);
    }

    public static List<object> convertToObjectList<T>(this List<T> list)
    {
        var l = new List<object>();
        foreach (var o in list) {
            l.Add(o);
        }

        return l;
    }
    
    /// <summary>
    /// 转换为带单位的字符串
    /// </summary>
    public static string toMPString(this object obj, double cut = 999)
    {
        double number = obj.toDouble();
        if (number is < 10 and > 0)
        {
            return number.ToString("0.##");
        }

        if (number < 0 && number > -cut)
        {
            return number.toLong().ToString();
        }
        if (number >= 0 && number < cut)
        {
            // var n = (Mathf.RoundToInt(obj.toFloat() * 10)) / 10f;
            // return n.toString();
            //return number.ToString();
            return number.toLong().ToString();
        }

        int length = (int)System.Math.Log10(number);
        length = length - length % 3;
        var sn = number / Mathf.Pow(10f, length);
        string k = string.Empty;
        if (length >= 27)
        {
            //要转AA BB CC
            //ASCII 65 -> A
            int AAsciiCode = 65;
            //计算3的位数
            int l = (length - 27) / 3;
            //计算AABBCC的位数
            int w = Mathf.FloorToInt((float)l / 26f);
            int ii = (w > 0) ? 2 : 0;
            while (w > 26)
            {
                w = Mathf.FloorToInt((float)w / 26f);
                ii++;
            }
            var lll = ii == 0 ? 10 : ii;
            byte[] byteArray = new byte[lll];
            if (ii == 0)
            {
                //默认带A 
                byteArray[0] = (byte)(AAsciiCode);
                byteArray[1] = (byte)(AAsciiCode + l);
            }
            else
            {
                while (ii > 0)
                {
                    int code = l % 26;
                    byteArray[ii - 1] = (byte)(AAsciiCode + code);
                    l = Mathf.FloorToInt((float)l / 26f) - 1;
                    ii--;
                }
            }
            ASCIIEncoding asciiEncoding = new ASCIIEncoding();
            k = asciiEncoding.GetString(byteArray);
            k = k.Replace("\0", "");
        }
        else
        {
            k = MPDict.stringValue(length);
        }
        //判断小数点位数 
        //确保保证 是4位数 
        //10000 => 10.0k
        //100000 => 1.00M
        //1000000 => 10.0M 
        string snumber = sn.toString();
        //没有小数点
        if (!snumber.Contains("."))
        {
            if (snumber.Length == 1)
            {
                return string.Format("{0}.00{1}", snumber, k);
            }
            else if (snumber.Length == 2)
            {
                return string.Format("{0}.0{1}", snumber, k);
            }
            else
            {
                return string.Format("{0}{1}", snumber, k);
            }
        }
        else
        {
            //拆分
            string[] strArr = snumber.Split('.');
            if (strArr.Length == 1 && snumber.IndexOf('.') == 0)
            {
                string decimalStr = strArr[0];
                decimalStr = decimalStr.PadRight(2, '0');
                decimalStr = decimalStr.Substring(0, 2);
                return string.Format("{0}{1}", string.Format("0.{0}", decimalStr), k);
            }
            else
            {
                //小数点前部分
                string numStr = strArr[0];
                //小数点后部分
                string decimalStr = strArr.Length == 1 ? "0" : strArr[1];
                //如果前部分大于等于3以上 则后面部分全部不显示 不足3则根据后部分补0裁剪到合适
                if (numStr.Length == 1)
                {
                    decimalStr = decimalStr.PadRight(2, '0');
                    decimalStr = decimalStr.Substring(0, 2);
                    return string.Format("{0}{1}", string.Format("{0}.{1}", strArr[0], decimalStr), k);
                }
                else if (numStr.Length == 2)
                {
                    decimalStr = decimalStr.PadRight(1, '0');
                    decimalStr = decimalStr.Substring(0, 1);
                    return string.Format("{0}{1}", string.Format("{0}.{1}", strArr[0], decimalStr), k);
                }
                else
                {
                    return string.Format("{0}{1}", numStr, k);
                }
            }

        }
    }

   
    public static T getStaticProperty<T>(this Type type, string propertyName) where T : class
    {
        if (type == null || propertyName == null) {
            return default(T);
        }

        BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        PropertyInfo info = type.GetProperty(propertyName, flags);
        if (info == null) {
            return default(T);
        }
        return info.GetValue(null, null) as T;
    }
}