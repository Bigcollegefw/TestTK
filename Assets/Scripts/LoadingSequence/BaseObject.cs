using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObject
{
    protected virtual bool needStatusObject => false; // 是否需要状态机
    public MainNode mainNode => MainNode.Instance;
    
    protected StateObject stateObject;
    
    public string key { get; private set; }
    public event Action onStateChangeAction;
    
    public virtual void start()
    {
        
    }
    public virtual void update(float dt)
    {
        if (this.needStatusObject)
        {
            this.stateObject.update(dt);
        }
    }
    
    public virtual void stop()
    {
        this.over();
    }

    protected virtual void over()
    {
        if (this.needStatusObject)
        {
            this.stateObject?.clearStatus();
            this.stateObject?.clearAction();
            this.stateObject = null;
        }
    }
}
