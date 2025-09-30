using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateObject
{
    public StateObject()
    {
        this.statusActions = new Dictionary<int, Action>();
        this.updateActions = new Dictionary<int, Action<float>>();
        this.leaveActions = new Dictionary<int, Action>();
    }
    // 状态进入时的逻辑
    public Dictionary<int, Action> statusActions { get; private set; }
    // 在特定状态下每帧需要执行逻辑
    public Dictionary<int, Action<float>> updateActions { get; private set; }
    // 离开该状态时的逻辑
    public Dictionary<int, Action> leaveActions { get; private set; }

    private int _status;
    public int status
    {
        get { return _status; }
        set
        {
            if (_status == value)
            {
                return;
            }
            this.leaveActions.objectValue(_status)?.Invoke();
            _status = value;
            this.statusActions.objectValue(_status)?.Invoke();
        }
    }
    public void update(float dt)
    {
        this.updateActions.objectValue(_status)?.Invoke(dt);
    }
    public void clearAction()
    {
        this.statusActions.Clear();
        this.updateActions.Clear();
        this.leaveActions.Clear();
    }
    public void clearStatus()
    {
        this.status = IC.NotFound;
    }
}
