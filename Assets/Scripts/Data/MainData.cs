using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainData
{
    private DataBaseManager dataBaseMgr = DataBaseManager.Instance;
    public MainNode mainNode { get; private set; }
    
    public MainData()
    {
        Instance = this;
    }

    public static MainData Instance;    // 这里不是单例，如果多次new的话会覆盖

    public PlayerNode player;    // 玩家节点
    
    public Dictionary<FloorNode, GridPos> floorNodes = new Dictionary<FloorNode, GridPos>(); // 存所有的floorNode
    
    // 玩家当前位置格子点
    private GridPos _playerGrid;
    public GridPos playerGrid
    {
        get { return _playerGrid; }
        set
        {
            _playerGrid = value;
            this._curFloor = GetFloorNodeAtGrid(value.col, value.row);
        }
    }
    // 当前玩家脚底下的floor
    private FloorNode _curFloor;
    public FloorNode curFloor
    {
        get { return _curFloor; }
        set { _curFloor = value; }
    }
    
    // 目标位置格子
    private GridPos _targetGrid;
    public GridPos targetGrid
    {
        get { return _targetGrid; }
        set { _targetGrid = value; }
    }
    // 目标位置坐标点
    private Vector2 _targetPos;
    public Vector2 targetPos
    {
        get { return _targetPos; }
        set { _targetPos = value; }
    }
    // 玩家是否正在移动
    private bool _isMoving;
    public bool isMoving
    {
        get { return _isMoving; }
        set { _isMoving = value; }
    }
    // 玩家是否死亡或将死
    private bool _playerDead;
    public bool playerDead
    {
        get { return _playerDead; }
        set { _playerDead = value; }
    }
    // 是否有步数限制
    public bool hasStepLimit;
    // 剩余步数
    private int _remainStep;
    public int remainStep
    {
        get { return _remainStep; }
        set { _remainStep = value; }
    }
    
    /// <summary>
    /// 通过格子坐标转为实际坐标
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    public Vector2 GetVector2(int col, int row)
    {
        float gridWidth = 125f;  //格子宽高
        int levelCol = dataBaseMgr.curLevelConfig.col;
        int levelRow = dataBaseMgr.curLevelConfig.row;

        float totalWidth = levelCol * gridWidth;
        float totalHeight = levelRow * gridWidth;

        float startX = -totalWidth / 2f + gridWidth / 2f;
        float startY = totalHeight / 2f - gridWidth / 2f;

        float posX = startX + col * gridWidth;
        float posY = startY - row * gridWidth;

        return new Vector2(posX, posY);
    }
    
    /// <summary>
    /// 根据位置获取FloorNode
    /// </summary>
    public FloorNode GetFloorNodeAtGrid(int col, int row)
    {
        foreach (var kvp in floorNodes)
        {
            if (kvp.Value.col == col && kvp.Value.row == row)
            {
                return kvp.Key;
            }
        }
        return null;
    }
}
