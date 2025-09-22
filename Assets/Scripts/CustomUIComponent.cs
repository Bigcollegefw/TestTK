using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

public class CustomUIComponent : MonoBehaviour
{
    [Header("初始化时候是否被移除")]
    public bool unused;
    public RectTransform rectTransform => this.transform as RectTransform;
    public Rect rect => this.rectTransform.rect;
    private bool isStart;

    public virtual void startComponent()
    {

    }

    public virtual void stopComponent()
    {

    }

    public void changeParent(Transform tr)
    {
        this.transform.SetParent(tr);
    }

    public Vector2 getScreenPosByCenter()
    {
        var vec4 = new Vector3[4];
        this.rectTransform.GetWorldCorners(vec4);
        var center = Vector3.zero;
        foreach (var vec in vec4)
        {
            center += vec;
        }

        center = center / vec4.Length;
        return RectTransformUtility.WorldToScreenPoint(Camera.main, center);
    }

    public bool checkIsTopUI()
    {
        var screenPos = this.getScreenPosByCenter();
        if (screenPos.x <= 0 || screenPos.x >= Screen.width
            || screenPos.y <= 0 || screenPos.y >= Screen.height)
        {
            return false;
        }
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = this.getScreenPosByCenter();
        var results = ListPool<RaycastResult>.Get();
        // 进行射线投射
        EventSystem.current.RaycastAll(pointerEventData, results);
        var b = false;
        foreach (var result in results)
        {
            if (this.gameObject == result.gameObject)
            {
                b = true;
                break;
            }

            if (result.gameObject.transform.IsChildOf(this.transform))
            {
                b = true;
                break;
            }

            return false;
        }
        ListPool<RaycastResult>.Release(results);
        return b;
    }
}
