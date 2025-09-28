using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleLoadObject 
{
    enum LoadState
    {
        None,
        PackPath, // 找路径
        Module,   // 加载模块
        Over,
    }
    private int maxCount;
    public bool isComplete { get; private set; }
    private List<string> modList;
    private LoadState _loadState;
    private LoadState loadState
    {
        get => this._loadState;
        set
        {
            if (this._loadState == value)
            {
                return;
            }
            this._loadState = value;
            if (this._loadState == LoadState.None)
            {
                this._loadState = LoadState.PackPath;
            }
            else if (this._loadState == LoadState.PackPath)
            {
                CacheManager.Instance.loadAssetBundleModAsync(() =>
                {
                    this.loadState = LoadState.Module;
                });
            }else if (this._loadState == LoadState.Module)
            {
                //暂时刚进入游戏后，加载全部ab包，后续要按需加载
                foreach (var kvp in CacheManager.Instance.assetBundlePaths)
                {
                    var mod = kvp.Value;
                    if (this.modList.Contains(mod))
                    {
                        continue;
                    }
                    // 从内存机制看，string 作为引用类型，代码中的复制是浅拷贝（复制引用）。
                    // 但由于 string 的不可变性，其表现效果类似值类型的 “独立复制”，不会出现引用类型浅拷贝的常见副作用
                    this.modList.Add(mod);
                }

                this.maxCount = this.modList.Count;
                this.tryLoadMod();
            }else if (this._loadState == LoadState.Over)
            {
                this.complete();
            }
        }
    }
    public void complete()
    {
        this.isComplete = true;
    }
    void tryLoadMod()
    {
        if (this.modList.Count == 0)
        {
            this.loadState = LoadState.Over;
            return;
        }
        var mod = this.modList[0];
        this.modList.RemoveAt(0);
        CacheManager.Instance.loadModuleByAssetBundleAysnc(mod, bundle => this.tryLoadMod());
        // 按顺序逐个加载 . 通过回调实现。
    }
}
