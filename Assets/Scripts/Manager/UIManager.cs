using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UICanvasType
{
    None,
    Main,
    System,
    Popup,
    Debug,
    Tip,
    Screen,
}
public class UIManager : Singleton<UIManager>
{
    public event Action<BaseUI> onOpenUIAction;
    public event Action<BaseUI> onCloseUIAction;
    
    private List<BaseUI> uiList { get; set; }

    // 用于存储需要删除的UI
    private List<BaseUI> tempDeleteUIList;

    // 临时存储UI列表，用来遍历UI
    private List<BaseUI> tempUIList;

    public override void Init()
    {
        this.uiList = new List<BaseUI>();
        this.tempUIList = new List<BaseUI>();
        this.tempDeleteUIList = new List<BaseUI>();
    }
    
    // 这个update再GameCtrl中的Update里面驱动
    public void update(float deltaTime)
    {
        tempDeleteUIList.Clear();
        tempUIList.Clear();
        tempUIList.AddRange(this.uiList); // 将一个集合的所有元素添加到目标列表的末尾

        foreach (var ui in tempUIList)
        {
            ui.update(deltaTime);
            if (ui.deleteLater)
            {
                this.onCloseUIAction?.Invoke(ui);
                ui.Release();
                this.tempDeleteUIList.Add(ui);
            }
        }

        if (this.tempDeleteUIList.Count > 0)
        {
            foreach (var ui in this.tempDeleteUIList)
            {
                this.uiList.RemoveAll(x => x == ui);
                // 如果 x == ui 返回true就删除。
            }
        }
    }
    //使用UI类型来获取UI对象（约束T继承自BaseUI）
    public T GetUI<T>() where T : BaseUI
    {
        foreach (var ui in uiList)
        {
            if (ui is T)
            {
                return ui as T;
            }
        }
        return null;
    }

    public T OpenUI<T>(IUIData uiData = null) where T : BaseUI
    {
        var pui = this.uiList.Find(x => x.GetType() == typeof(T));
        if (pui != null)
        {
            return pui as T;    
        }
        string _path = UIManager.GetUIPathByType(typeof(T));
        var prefabObj = Resources.Load(_path) as GameObject;
        
        var uiObject = MonoBehaviour.Instantiate(prefabObj, GameCtrl.instance.getCanvas(UICanvasType.None).transform) as GameObject;
        var rt = uiObject.transform as RectTransform;
        rt.offsetMax = new Vector2(0, 0);

        var ui = uiObject.GetComponent<T>();
        if (ui == null)
        {
            ui = uiObject.AddComponent<T>() as T;
        }
        
        ui.transform.SetParent(GameCtrl.instance.getCanvas(ui.uiCanvasType).transform);
        ui.start(uiData);
        this.uiList.Add(ui);
        Debug.Log($"Open UI:{ui.GetType()}");
        return ui;
    }
    
    private static Dictionary<Type, string> UIPathsByType = new Dictionary<Type, string>()
    {
    };
    public static string GetUIPathByType(Type type)
    {
        return UIPathsByType.objectValue(type, $"Prefabs/UI/{type.toString()}/{type.toString()}");
    }

}
