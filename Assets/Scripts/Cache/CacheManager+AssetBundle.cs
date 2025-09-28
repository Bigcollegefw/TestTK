using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CustomJson;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class CacheManager
{
    // StreamingAsset 下存ab包的路径
    public Dictionary<string, string> assetBundlePaths { get; private set; }
    //通过AB包模块加载出来的
    private Dictionary<string, (AssetBundle, Dictionary<string, object>)> assetBundles;
    //没有提前通过ab包模块加载出来的
    private Dictionary<string, Dictionary<string, object>> assetBundleResources;

    void initAssetBundle()
    {
        this.assetBundlePaths = new Dictionary<string, string>();
    }
    
    public string getAssetBundleMod(string fileName)
    {
        return this.assetBundlePaths.objectValue(fileName);
    }

    //先从 StreamingAssets 里的指定位置读取一个记录了 AssetBundle 路径映射关系的 JSON 配置文件，
    //把这些路径信息存起来，方便之后根据配置去加载对应的 AssetBundle 资源
    public void loadAssetBundleModAsync(Action callBack)
    {
        var path = Path.Combine(Application.streamingAssetsPath, "mods","native", "packpath.json");
        FileUtils.Instance.readFileStringAsync(path, (str) =>
        {
            var dic = MiniJson.JsonDecode(str);
            foreach (var kvp in dic.toDictionary())
            {
                this.assetBundlePaths[kvp.Key] = kvp.Value.toString();
            }
            callBack?.Invoke();
        });
    }

    public void loadModuleByAssetBundleAysnc(string module, Action<AssetBundle> callBack)
    {
        if (this.assetBundles.containsKey(module))
        {
            callBack?.Invoke(this.assetBundles[module].Item1);
            return;
        }
        CoroutineUtils.Instance.StartCoroutine(load(module, callBack));

        IEnumerator load(string module, Action<AssetBundle> callBack)
        {
            var path = Path.Combine(Application.streamingAssetsPath, "mods",  "native", "assetsBundles", module);
            var bundleLoadRequest = AssetBundle.LoadFromFileAsync(path);
            yield return bundleLoadRequest;
            
            var myLoadedAssetBundle = bundleLoadRequest.assetBundle;
            if (myLoadedAssetBundle == null)
            {
                Debug.Log($"Failed to load asset bundle: {path}");
                callBack?.Invoke(null); 
                yield break;    
            }
            
            this.assetBundles[module] = (myLoadedAssetBundle, new Dictionary<string, object>());
            callBack?.Invoke(myLoadedAssetBundle);
        }
    }

    public T loadResourceByAssetBundle<T>(string fileName) where T : Object
    {
        var module = this.getAssetBundleMod(fileName);
        if (module.IsNullOrEmpty())
        {
            return null;
        }
#if UNITY_EDITOR
        var path = Path.Combine("Assets", "AssetBundleResource", module, fileName);
        return path.loadResource<T>();
#endif
        if (this.assetBundles.containsKey(module))
        {
            var item = this.assetBundles[module];
            var o = item.Item2.objectValue(fileName);
            if (o != null)
            {
                return o as T;
            }
            var t = item.Item1.LoadAsset<T>(fileName);
            item.Item2[fileName] = t;
            return t;
        }
        
        Debug.LogWarning($"资源对应的ab包mod没有提前加载,这里加载出来并且同步到assetBundleResources里");
        var dic = this.assetBundleResources.objectValue(module);
        if (dic == null)
        {
            dic = new Dictionary<string, object>();
            this.assetBundleResources.Add(module, dic);
        }
        var obj = dic.objectValue(fileName);
        if (obj == null)
        {
            var o = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "mods", "native", "assetsBundles", module));
            if (o == null)
            {
                Debug.LogError(string.Format("资源加载失败 module : {0}  fileName : {1}", module, fileName));
                return null;
            }

            obj = o.LoadAsset<T>(fileName);
            o.Unload(false);

            dic[fileName] = obj;
        }
        return obj as T;
    }
}

