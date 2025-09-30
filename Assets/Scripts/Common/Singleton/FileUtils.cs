using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class FileUtils : SingletonData<FileUtils>
{
    protected override void OnInit()
    {
    }

    public void readFileStringAsync(string path, Action<string> callBack)
    {
        CoroutineUtils.Instance.StartCoroutine(load(path, callBack));

        IEnumerator load(string path, Action<string> callBack)
        {
            var uri = new Uri(path);
            var www = UnityWebRequest.Get(uri);
            yield return www.SendWebRequest();
            if (www.error != null)
            {
                Debug.LogWarning("UnityWebRequest Error :" + www.error);
                callBack?.Invoke(null);
            }
            if (www.downloadHandler.isDone) {
                var bytes = www.downloadHandler.data;
                var str =bytes == null ? string.Empty : Encoding.UTF8.GetString(bytes).Replace("\uFEFF", string.Empty);
                callBack?.Invoke(str);
            }
        }
    }
    
    public void writeFileByStream(string path,string fileName, string value, bool append = false)
    {
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
        var p = Path.Combine(path, fileName);
        var sw = new StreamWriter(p, append, Encoding.UTF8);
        //json方法写入
        sw.Write(value);
        //清空缓冲区
        sw.Flush();
        //关闭流
        sw.Close();
        sw.Dispose();
    }
}
