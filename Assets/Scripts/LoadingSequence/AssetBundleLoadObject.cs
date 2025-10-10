using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleLoadObject : BaseLoadingObject
{
    public AssetBundleLoadObject() : base()
    {
        this.modList = new List<string>();
    }
    
    public override string desc => "LoadingAssetBundle";
    enum LoadState
    {
        None,
        PackPath, // 找路径
        Module,   // 加载模块
        Over,
    }
    private int maxCount;
    
    // public bool isComplete { get; private set; } //导致 AssetBundleLoadObject 中的 complete() 实际修改的是子类自己
    // 的 isComplete，而 GameCtrl 在 updateLoading 中判断的是父类 BaseLoadingObject 的 isComplete（始终为 false）
    private List<string> modList;
    private LoadState _loadState; // 一开始的默认值是枚举值为0的值
    private LoadState loadState
    {
        get => this._loadState;
        set //第一次主动修改 loadState 的值时，才会进入 set 访问器的逻辑。
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
                Debug.Log("AssetBundleLoadObject is complete");
            }
        }
    }

    public override void update(float dt)
    {
        base.update(dt);

        if (this.loadState == LoadState.None)
        {
            this.loadState = LoadState.PackPath;
        }
        else if (this.loadState == LoadState.Module)
        {

        }
        else if (this.loadState == LoadState.Over)
        {

        }
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
