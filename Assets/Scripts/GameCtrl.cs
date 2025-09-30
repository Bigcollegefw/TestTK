using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameCtrlStatus
{
    public const int None = 0;
    public const int Loading = 1;
    public const int Stay = 2;
    public const int Over = 3;
}
public class GameCtrl : MonoBehaviour
{
    private List<BaseLoadingObject> _loadingObjectList;
    private float totalLoadingCount;
    private float loadingDelay;
    private StateObject _stateObject;

    public static List<string> LoadingObjectSeq = new List<string>()
    {
        // typeof(InitSettingLoadingObject).toString(),
        typeof(AssetBundleLoadObject).toString(),
        typeof(ConfigLoadingObject).toString(),
    };
    
    private bool isStepComplete;
    
    //下面都是浅拷贝，本质上是返回 _stateObject.statusActions 字典的引用，而非创建新字典。
    protected Dictionary<int, Action> statusActions => _stateObject.statusActions;
    //也是关联到 _stateObject 内部的字典，赋值操作都会直接作用于 _stateObject 中的对应字典。
    protected Dictionary<int, Action<float>> updateActions => _stateObject.updateActions;
    protected Dictionary<int, Action> leaveActions => _stateObject.leaveActions;
    
    // 这个是执行的关键
    private int baseState
    {
        get => this._stateObject.status;
        set => this._stateObject.status = value;
    }
    void Awake()
    {
        this._stateObject = new StateObject();
        this._loadingObjectList = new List<BaseLoadingObject>();
        this.statusActions[GameCtrlStatus.Loading] = this.runLoading;
        this.updateActions[GameCtrlStatus.Loading] = this.updateLoading;
    }

    void Start()
    {
        this.baseState = 1;
    }
    void Update()
    {
        this._stateObject.update(Time.deltaTime);
        if (this.isStepComplete)
        {
            this.isStepComplete = false;
            this.baseState += 1;
        }
    }

    
    void runLoading()
    {
        Debug.Log("runLoading");
        // 创建要进行的步骤
        foreach (var className in LoadingObjectSeq)
        {
            var o = DataUtils.Instance.getActivator<BaseLoadingObject>(className); // 创建一个对象
            o.start();
            this._loadingObjectList.Add(o);
        }
        this.loadingDelay = 0.5f;
        this.totalLoadingCount = this._loadingObjectList.Count;
    }

    void updateLoading(float dt)
    {
        Debug.Log("updateLoading");
        if (this._loadingObjectList.Count == 0)
        {
            this.loadingDelay -= dt;
            if (this.loadingDelay <= 0)
            {
                this.stepComplete();
            }
            return;
        }
        var o = this._loadingObjectList[0];
        o.update(dt);
        if (o.isComplete)
        {
            o.stop();
            this._loadingObjectList.RemoveAt(0);
        }
    }
    
    void stepComplete()
    {
        this.isStepComplete = true;
    }
}
