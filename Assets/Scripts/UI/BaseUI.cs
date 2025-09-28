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
    public CanvasGroup canvasGroup { get; private set; }

    public void Start()
    {
        OnStart(uiData);
    }

    public virtual void OnStart(IUIData uiData)
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
}
