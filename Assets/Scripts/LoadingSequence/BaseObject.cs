using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObject
{
    protected virtual bool needStatusObject => false; // 是否需要状态机
    public MainNode mainNode => MainNode.Instance;
    public string key { get; private set; }
    public event Action onStateChangeAction;
    
    public virtual void start()
    {
        
    }
    public virtual void update(float dt)
    {
        
    }
}
