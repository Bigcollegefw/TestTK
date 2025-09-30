using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using CustomJson;

public class PackManagerPath
{
    [MenuItem("Tools/BuildTool/PackPath")]
    public static void packPath()
    {
        var rootPath = BuildTools.rootPath;
        if (!Directory.Exists(rootPath)) {
            Debug.LogError("CreateAssetBundles Error AssetBundleResource文件夹缺失");
            return;
        }
        var dic = new Dictionary<string, string>();
        DirectoryInfo rootDirectory = new DirectoryInfo(rootPath);
        findPack(dic, rootDirectory);

        var json = MiniJson.JsonEncode(dic);
        //保存
        FileUtils.Instance.writeFileByStream(Path.Combine(Application.streamingAssetsPath,
            "mods", "native"), 
            "packpath.json",json);
    }

    static void findPack(Dictionary<string, string> dic, DirectoryInfo directory)
    {
        var fileName = directory.Name;
        if (BuildTools.CheckFileSuffixNeedIgnore(fileName)) {
            return;
        }
        
        //检查下面的文件
        foreach (var fileInfo in directory.GetFiles()) {
            if (BuildTools.CheckFileSuffixNeedIgnore(fileInfo.Name)) {
                continue;
            }

            var fn = fileInfo.Name;
            // var fn = fileInfo.Name;
            if (dic.ContainsKey(fn)) {
                Debug.LogError("PackPath 命名重复 ：" + fn);
                continue;
            }

            var replacePath = BuildTools.rootPath;
            var p = fileInfo.DirectoryName.Substring(replacePath.Length + 1);
            dic[fn] = p;
        }
        
        //检查下面的文件夹
        foreach (var directoryInfo in directory.GetDirectories()) {
            findPack(dic, directoryInfo);
        }
    }
}