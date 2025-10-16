using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIData
{
}

public class BaseUI : MonoBehaviour
{
    protected IUIData uiData;
    public DataBaseManager dataIns => DataBaseManager.Instance;
    public CanvasGroup canvasGroup { get; private set; }
    public MainData mainData => MainData.Instance;
    public virtual UICanvasType uiCanvasType => UICanvasType.System;
    public bool deleteLater { get; private set; }
    public virtual void start(IUIData uiData)
    {
        this.uiData = uiData;
        foreach (var obj in this.gameObject.GetComponentsInChildren<CustomUIComponent>(true))
        {
            if (obj ==null) continue;
            if (obj.unused) 
            {
                GameObject.DestroyImmediate(obj.gameObject);
            }
            obj.startComponent();
        }
        this.canvasGroup = this.gameObject.GetComponent<CanvasGroup>();
        if (this.canvasGroup == null)
        {
            this.canvasGroup = this.gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    public virtual void update(float dt)
    {
    }

    public void Release()
    {
        this.stop();
        GameObject.Destroy(this.gameObject);
    }
    
    protected virtual void stop()
    {
        var uiComponents = this.gameObject.transform.GetComponentsInChildren<CustomUIComponent>(true);
        foreach (var obj in uiComponents)
        {
            obj.stopComponent();
        }
    }
    public void closeUI()
    {
        this.deleteLater = true; // 这个设置为true的时候就会在UIManager的update里面驱动删除
    }
    
}
