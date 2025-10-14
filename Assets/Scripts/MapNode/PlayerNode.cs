using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNode : MonoBehaviour
{
    private float moveSpeed = 1200f;
    public RectTransform rectTransform; // RectTransform组件
    private MainNode mainNode => MainNode.Instance;
    private MainData mainData => MainData.Instance;
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (!mainData.isMoving) return;
        if (mainData.gameResult != GameResult.common) return;
        
        // 计算当前位置到目标位置的距离
        Vector2 currentPos = rectTransform.anchoredPosition;
        Vector2 targetPosition = mainData.targetPos;
        float distance = Vector2.Distance(currentPos, targetPosition);
        
        // 计算移动方向
        Vector2 direction = (targetPosition - currentPos).normalized;
        
        // 计算这一帧的移动距离，限制deltaTime避免突然的大跳跃
        float clampedDeltaTime = Mathf.Min(Time.deltaTime, 1f / 60f);
        float moveDistance = this.moveSpeed * clampedDeltaTime;

        if (moveDistance >= distance)
        {
            rectTransform.anchoredPosition = targetPosition;
            moveEnd();
            return;
        }
        // 移动玩家
        rectTransform.anchoredPosition = currentPos + direction * moveDistance;
    }
    
    /// <summary>
    /// 开始移动
    /// </summary>
    public void StartMovement()
    {
        mainData.isMoving = true;
    }
    
    /// <summary>
    /// 结束移动
    /// </summary>
    public void moveEnd()
    {
        Debug.Log("玩家移动结束");
        mainData.isMoving = false;
        // 更新玩家位置到目标格子位置
        mainData.playerGrid = mainData.targetGrid;
        Debug.Log($"玩家位置已更新到: ({mainData.targetGrid.col}, {mainData.targetGrid.row})");
        //TODO 停下来检查游戏的结果
        mainNode.gameWindow.mapNode.CheckGameResult();
    }
}
