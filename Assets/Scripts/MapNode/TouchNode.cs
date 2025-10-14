using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


// 方向枚举
public enum Direction
{
    None,   // 无方向状态
    Up,
    Down,
    Left,
    Right
}

public class TouchNode : CustomUIComponent
{
    public MapNode mapNode; // 地图节点
    
    private MainData mainData => MainData.Instance;
    
    private Vector2 touchStartPos; // 触摸开始的位置
    private Vector2 touchCurrentPos; // 触摸结束的位置
    private bool isDragging = false;    // 是否在拖拽中
    private float minDragDistance = 100f; // 最小拖拽距离，防止误触
    private Direction lastDirection = Direction.None; // 记录上一帧的方向，避免重复触发
    private bool _enableTouchEvents = true; // 是否启用触摸事件
    public bool enableTouchEvents
    {
        get => _enableTouchEvents;
        set
        {
            _enableTouchEvents = value;
            UpdateTouchEventsState(); // 当值改变时更新触摸事件状态
        }
    }
    public override void startComponent()
    {
        base.startComponent();
        this.AddTouchEventsTothis();
        this.UpdateTouchEventsState();
        this.InitTouch();
    }
    public override void stopComponent()
    {
        base.stopComponent();
        this.RemoveTouchEventsFromthis();
    }
    public void InitTouch()
    {
        this.enableTouchEvents = true;
    }

    private void AddTouchEventsTothis()
    {
        // 添加EventTrigger组件来处理触摸事件
        EventTrigger eventTrigger = this.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = this.gameObject.AddComponent<EventTrigger>();
        }
        // 清空现有事件
        eventTrigger.triggers.Clear();
        // 开始添加触摸事件
        EventTrigger.Entry beginEntry = new EventTrigger.Entry();
        beginEntry.eventID = EventTriggerType.PointerDown;
        beginEntry.callback.AddListener((data) =>
        {
            OnPointerDown((PointerEventData)data);
        });
        eventTrigger.triggers.Add(beginEntry);
        // 添加触摸移动事件
        EventTrigger.Entry dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.Drag;
        dragEntry.callback.AddListener((data) =>
        {
            OnDrag((PointerEventData)data);
        });
        eventTrigger.triggers.Add(dragEntry);
        // 添加触摸结束事件
        EventTrigger.Entry endEntry = new EventTrigger.Entry();
        endEntry.eventID = EventTriggerType.PointerUp;
        endEntry.callback.AddListener((data) =>
        {
            OnPointerUp((PointerEventData)data);
        });
        eventTrigger.triggers.Add(endEntry);
    }

    private void RemoveTouchEventsFromthis()
    {   
        Debug.Log("移除触摸事件");
        EventTrigger eventTrigger = this.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger != null)
        {
            eventTrigger.triggers.Clear();
        }
    }
    
    /// <summary>
    /// 当enableTouchEvents改变时更新触摸事件状态
    /// </summary>
    private void UpdateTouchEventsState()
    {
        if (this == null) return;
        EventTrigger eventTrigger = this.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger != null)
        {
            eventTrigger.enabled = _enableTouchEvents;

            if (_enableTouchEvents)
            {
                Debug.Log("触摸事件已启用");
            }
            else
            {
                Debug.Log("触摸事件已禁用");
                // 如果正在拖拽，强制结束拖拽状态
                if (isDragging)
                {
                    isDragging = false;
                    lastDirection = Direction.None;
                    Debug.Log("触摸事件被禁用，强制结束拖拽状态");
                }
            }
        }
    }

    /// <summary>
    /// 触摸开始
    /// </summary>
    /// <param name="data"></param>
    private void OnPointerDown(PointerEventData eventData)
    {
        // 检查触摸事件是否启用
        if (!_enableTouchEvents) return;
        touchStartPos = eventData.position;
        touchCurrentPos = touchStartPos;
        isDragging = false;
        lastDirection = Direction.None;
        Debug.Log($"触摸开始位置: {touchStartPos}");
    }
    
    /// <summary>
    ///  触摸拖拽中 - 每一帧都会调用
    /// </summary>
    /// <param name="eventData"></param>
    private void OnDrag(PointerEventData eventData)
    {
        if (!_enableTouchEvents) return;
        if (!isDragging)
        {
            isDragging = true;  // 只有在拖拽中才会设置为true
        }
        touchCurrentPos = eventData.position;
        
        // 计算当前拖拽向量
        Vector2 dragDelta = touchCurrentPos - touchStartPos;
        float dragDistance = dragDelta.magnitude;  //计算出长度
        
        // 是否达到最小位移阈值（本帧状态）
        bool reached = dragDistance >= minDragDistance;
        
        // 没达到最小位移
        if (!reached)
        {
            Debug.Log($"未达到最小位移({minDragDistance:F1})，当前距离: {dragDistance:F1}");
            this.mapNode.arrowNode.setActiveByCheck(false);
            return;
        }
        
        // 达到最小位移时：计算方向并输出；若方向变化则额外输出变化日志
        Direction currentDirection = GetDragDirection(dragDelta);
        
        // 方向变化时
        if (currentDirection != lastDirection)
        {
            Debug.Log($"方向变化: {lastDirection} -> {currentDirection}，距离: {dragDistance:F1}");
            lastDirection = currentDirection;

            // 实时处理拖拽方向
            SetArrowNodePositionAndRotation(currentDirection);
        }
    }

    private void OnPointerUp(PointerEventData eventData)
    {
        this.mapNode.arrowNode.setActiveByCheck(false);
        // 检查触摸事件是否启用
        if (!_enableTouchEvents) return;

        if (isDragging)
        {
            Vector2 dragDelta = touchCurrentPos - touchStartPos;
            float dragDistance = dragDelta.magnitude;
            if (dragDistance >= minDragDistance)
            {
                Direction finalDirection = GetDragDirection(dragDelta);
                Debug.Log($"拖拽结束 - 最终方向: {finalDirection}, 总距离: {dragDistance:F1}");

                // 触摸结束时检查路径并移动玩家
                CheckAndMovePlayer(finalDirection);
            }
            else
            {
                Debug.Log("拖拽结束 - 未达到最小位移，可能是点击");
            }
        }
        
        isDragging = false;
        lastDirection = Direction.None;
    }

    /// <summary>
    /// 设置拖拽方向，四个方向。
    /// </summary>
    /// <param name="dragDelta"></param>
    /// <returns></returns>
    private Direction GetDragDirection(Vector2 dragDelta)
    {
        float absX = Mathf.Abs(dragDelta.x);
        float absY = Mathf.Abs(dragDelta.y);
        // 如果水平移动距离大于垂直移动距离，判断为水平方向
        if (absX > absY)
        {
            return dragDelta.x > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            return dragDelta.y > 0 ? Direction.Up : Direction.Down;
        }
    }

    /// <summary>
    /// 设置箭头位置和方向
    /// </summary>
    /// <param name="direction"></param>
    private void SetArrowNodePositionAndRotation(Direction direction)
    {
        if (this.mapNode?.arrowNode == null) return;
        
        // 获取玩家当前位置的世界坐标
        Vector2 playerWorldPos = GetPlayerWorldPosition();
        // 设置箭头节点位置
        RectTransform arrowRectTransform = this.mapNode.arrowNode.GetComponent<RectTransform>();
        if (arrowRectTransform != null)
        {
            float arrowHeight = arrowRectTransform.sizeDelta.y; // 获取箭头的高度
            Vector2 adjustedPosition = playerWorldPos + new Vector2(0, arrowHeight / 2);
            arrowRectTransform.anchoredPosition = adjustedPosition; // 设置箭头的开始位置

            // 根据方向设置角度
            float rotationAngle = GetRotationAngleByDirection(direction);
            arrowRectTransform.localRotation = Quaternion.Euler(0, 0, rotationAngle);

            // 显示箭头
            this.mapNode.arrowNode.setActiveByCheck(true);
        }
    }
    // 获取玩家当前位置的世界坐标
    private Vector2 GetPlayerWorldPosition()
    {
        int col = this.mainData.playerGrid.col;
        int row = this.mainData.playerGrid.row;
        return this.mainData.GetVector2(col, row);
    }
    
    // 根据方向获取旋转角度
    private float GetRotationAngleByDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return 0f;      // 向上，不旋转
            case Direction.Down:
                return 180f;    // 向下，旋转180度
            case Direction.Left:
                return 90f;     // 向左，旋转90度
            case Direction.Right:
                return -90f;    // 向右，旋转-90度
            default:
                return 0f;
        }
    }

    // 检查并移动玩家
    private void CheckAndMovePlayer(Direction direction)
    {
        if (mapNode == null) return;

        // 禁用触摸事件
        this.enableTouchEvents = false;

        // 调用MapNode的检查并移动方法
        mapNode.CheckAndMovePlayer(direction);
    }
}
