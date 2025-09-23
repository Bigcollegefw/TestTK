using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class StringConvert
{
    public static bool IsNullOrEmpty(this string str)
    {
        if (str == "0")
        {
            return true;
        }

        return string.IsNullOrEmpty(str);
    }

    public static string findMidStrEx(this string source, string startStr, string stopStr)
    {
        var startIndex = source.IndexOf(startStr);
        if (startIndex < 0)
        {
            return string.Empty;
        }

        string tmpstr = source.Substring(startIndex + startStr.Length);
        var stopIndex = tmpstr.IndexOf(stopStr);
        if (stopIndex < 0)
        {
            return string.Empty;
        }
        var result = tmpstr.Remove(stopIndex);

        return result;
    }
    
    public static string cutMidStr(this string source, string startStr, string stopStr)
    {
        if (source.IsNullOrEmpty()) {
            return string.Empty;
        }

        if (!source.Contains(startStr)) {
            return string.Empty;
        }
        
        var startIndex = source.IndexOf(startStr);
        if (startIndex < 0) {
            return string.Empty;
        }

        string tmpstr = source.Substring(startIndex + startStr.Length);
        if (!source.Contains(stopStr)) {
            return string.Empty;
        }
        var stopIndex = tmpstr.IndexOf(stopStr);
        if (stopIndex < 0) {
            return string.Empty;
        }
        var result = tmpstr.Remove(stopIndex);

        return result;
    }

}
