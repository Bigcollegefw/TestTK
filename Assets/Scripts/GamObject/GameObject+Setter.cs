using System;
using UnityEngine;
using Animation = UnityEngine.Animation;

public static class GameObjectSetter
{
    public static void setActiveByCheck(this GameObject go, bool isActive)
    {
        if (go.activeSelf == isActive)
        {
            return;
        }

        go.SetActive(isActive);
    }

    /// <summary>
    /// 设置透明度
    /// </summary>
    public static GameObject setAlpha(this GameObject go, float alpha)
    {
        // 限制透明度范围在 0-1 之间
        alpha = Mathf.Clamp01(alpha);

        // 尝试获取 CanvasGroup 组件
        CanvasGroup canvasGroup = go.GetComponent<CanvasGroup>();

        // 没有就添加组件
        if (canvasGroup == null)
        {
            canvasGroup = go.AddComponent<CanvasGroup>();
        }

        // 设置透明度
        canvasGroup.alpha = alpha;

        return go;
    }

    /// <summary>
    /// 设置显示状态，控制可见性、交互性和射线检测
    /// </summary>
    /// <param name="go">目标游戏对象</param>
    /// <param name="isShow">是否显示：true=显示且可交互，false=隐藏且不可交互</param>
    public static GameObject setCanvasShow(this GameObject go, bool isShow)
    {
        // 获取或添加 CanvasGroup 组件
        CanvasGroup canvasGroup = go.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = go.AddComponent<CanvasGroup>();
        }

        if (isShow)
        {
            // 显示状态：完全可见、可交互、阻挡射线
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            // 隐藏状态：完全透明、不可交互、射线穿透
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        return go;
    }


    public static void playAni(this Animator animator, string aniName, string path = "animator")
    {
        if (aniName.IsNullOrEmpty())
        {
            return;
        }
        if (animator.runtimeAnimatorController != null)
        {
            var aniCtrl = animator.runtimeAnimatorController;
            if (aniCtrl.name == aniName)
            {
                return;
            }
        }

        //TODO:push
        var preAni = animator.runtimeAnimatorController;
        //TODO:pop
        // var nextAni = DataUtils.Instance.getAnimator(aniName);
        // if (nextAni == null) {
        //     Debug.LogError("没有找到对应的动画：" + aniName);
        //     return;
        // }
        // animator.runtimeAnimatorController = nextAni;
    }

    public static float playAni(this Animation animation, string aniName, bool replay = true)
    {
        if (aniName.IsNullOrEmpty())
        {
            return 0;
        }

        if (!replay && animation.clip != null && animation.clip.name == aniName)
        {
            return 0;
        }
        animation.clip = null;

        if (!animation.Play(aniName))
        {
            return 0;
        }

        return animation[aniName].length;
    }
}