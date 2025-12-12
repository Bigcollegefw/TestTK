using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// 这个脚本把包打进StreamingAsset里面
public class BuildTools
{
    public static string rootPath = Path.Combine(Application.dataPath, "AssetBundleResource");

    private static List<AssetBundleBuild> _assetBundleBuilds = new List<AssetBundleBuild>();

    [MenuItem("Tools/BuildTool/CreateAssetBundles")]
    public static void BuildAssetBundles()
    {
        _assetBundleBuilds.Clear();

        if (!Directory.Exists(rootPath))
        {
            Debug.LogError("CreateAssetBundles Error AssetBundleResource文件夹缺失");
            return;
        }
        var targetPath = string.Format("{0}/{1}/{2}/{3}", Application.streamingAssetsPath, "mods", "native", "assetsBundles");

        if (Directory.Exists(targetPath)) // 删除已经生成的   
        {
            Directory.Delete(targetPath, true);
        }

        // 检查大写文件夹并警告
        Debug.Log("=== 开始检查文件夹命名 ===");
        CheckSubFolderNames(rootPath);
        Debug.Log("=== 文件夹命名检查完成 ===");
        
        //rootPath（即 Assets/AssetBundleResource 文件夹）为根目录，递归扫描所有子文件夹，
        //收集需要打包的资源信息，并生成对应的 AssetBundleBuild 配置（存储在 _assetBundleBuilds 列表中）
        DirectoryInfo rootDirectory = new DirectoryInfo(rootPath);
        createAssetBundlesByDir(rootDirectory);

        //确保输出路径 targetPath（格式为 StreamingAssets/mods/[ModName]/assetsBundles）存在，
        //如果不存在则自动创建该目录，避免后续打包时因路径不存在而报错。
        Directory.CreateDirectory(targetPath);

        var builds = _assetBundleBuilds.ToArray();
        BuildPipeline.BuildAssetBundles(targetPath,  //打包文件的输出目录。
            builds,  //AssetBundleBuild 数组，包含所有资源包的配置
            BuildAssetBundleOptions.ChunkBasedCompression,
            // LZ4 压缩算法, LZ4 压缩算法
            EditorUserBuildSettings.activeBuildTarget); //使用当前 Unity 编辑器激活的构建目标平台


        //保存当前 Unity 项目中的资源修改（确保打包前资源状态已保存）
        AssetDatabase.SaveAssets();
        //刷新 Unity 资源数据库，让编辑器识别到新生成的 AssetBundle 文件（否则在 Project 窗口中可能看不到打包结果）
        AssetDatabase.Refresh();

        //自定义工具类 PackManagerPath的packPath方法，可能是处理打包后的路径信息（例如记录资源包路径、生成路径配置文件等
        PackManagerPath.packPath();
    }

    /// <summary>
    /// 检查AssetBundleResource下的所有子文件夹名称是否包含大写字母
    /// </summary>
    /// <param name="assetBundleResourcePath">AssetBundleResource文件夹的完整路径</param>
    static void CheckSubFolderNames(string assetBundleResourcePath)
    {
        DirectoryInfo rootDir = new DirectoryInfo(assetBundleResourcePath);
        if (!rootDir.Exists)
        {
            return;
        }

        // 递归检查所有子文件夹，但不检查根目录本身
        CheckFoldersRecursive(rootDir, "", true);
    }
    
    /// <summary>
    /// 递归检查文件夹名称
    /// </summary>
    /// <param name="directory">当前目录</param>
    /// <param name="relativePath">相对于AssetBundleResource的路径</param>
    /// <param name="isRoot">是否为根目录（AssetBundleResource）</param>
    static void CheckFoldersRecursive(DirectoryInfo directory, string relativePath, bool isRoot = false)
    {
        DirectoryInfo[] subDirs = directory.GetDirectories();

        foreach (DirectoryInfo subDir in subDirs)
        {
            string folderName = subDir.Name;

            // 检查当前文件夹名是否包含大写字母
            if (HasUppercaseLetters(folderName))
            {
                string displayPath = isRoot ? folderName : $"{relativePath}/{folderName}";
                Debug.LogError($"[AssetBundle警告] 发现大写文件夹: '{displayPath}' - 建议使用小写文件夹名以避免跨平台兼容问题");
            }

            // 构建新的相对路径用于递归
            string newRelativePath = isRoot ? folderName : $"{relativePath}/{folderName}";

            // 递归检查子文件夹
            CheckFoldersRecursive(subDir, newRelativePath, false);
        }
    }
    
    /// <summary>
    /// 检查字符串是否包含大写字母
    /// </summary>
    /// <param name="text">要检查的字符串</param>
    /// <returns>如果包含大写字母返回true</returns>
    static bool HasUppercaseLetters(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
            {
                return true;
            }
        }
        return false;
    }
    
    static void createAssetBundlesByDir(DirectoryInfo directoryInfo)
    {
        // 排除根目录本身（只处理子目录及以下的资源）
        // 因为根目录下的资源会由子目录分别处理，避免将所有资源打包到根目录对应的包中
        if (!directoryInfo.FullName.Equals(rootPath))
        {
            // 存储当前目录下需要打包的资源路径列表
            var assetsNameList = new List<string>();
            // 获取当前目录下的所有文件
            var files = directoryInfo.GetFiles();
            foreach (var fileInfo in files)
            {
                // 检查文件是否需要忽略（如.meta、.cs等无效资源）
                if (CheckFileSuffixNeedIgnore(fileInfo.Name))
                {
                    continue;
                }

                // 计算文件相对于根目录（rootPath）的相对路径
                // 例如：rootPath是"Assets/AssetBundleResource"，文件路径是"Assets/AssetBundleResource/Textures/a.png"
                // 则p为"Textures/a.png"
                var p = fileInfo.FullName.Substring(rootPath.Length + 1);
                // 拼接Unity资源数据库识别的资源路径（必须以"Assets/"开头）
                // 例如："Assets/AssetBundleResource/Textures/a.png"
                var pp = String.Format("{0}/{1}/{2}", "Assets", "AssetBundleResource", p);
                // 将处理后的资源路径添加到列表
                assetsNameList.Add(pp);
            }

            if (assetsNameList.Count > 0)
            {
                // 计算当前目录相对于根目录的相对路径（作为AssetBundle的名称）
                // 例如：目录是"Assets/AssetBundleResource/Textures"，则p为"Textures"
                var p = directoryInfo.FullName.Substring(rootPath.Length + 1);
                // 创建一个AssetBundle打包配置实例
                AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
                // 设置AssetBundle的名称（使用当前目录相对路径，确保包名与目录结构对应）
                assetBundleBuild.assetBundleName = p;
                // 设置当前包需要包含的资源路径数组
                assetBundleBuild.assetNames = assetsNameList.ToArray();
                // 将该配置添加到打包列表中
                _assetBundleBuilds.Add(assetBundleBuild);
            }
        }
        // 获取当前目录下的所有子目录
        var directories = directoryInfo.GetDirectories();

        // 递归处理每个子目录（实现全目录树的遍历）
        foreach (var dirInfo in directories)
        {
            createAssetBundlesByDir(dirInfo);
        }
    }
    public static bool CheckFileSuffixNeedIgnore(string fileName)
    {
        if (fileName.EndsWith(".meta") || fileName.EndsWith(".DS_Store") ||
           fileName.EndsWith(".cs") || fileName.EndsWith(".lua") ||
           fileName.EndsWith(".manifest") || fileName.EndsWith(".plist"))
            return true;
        if (fileName.StartsWith("."))
            return true;
        return false;
    }

}
