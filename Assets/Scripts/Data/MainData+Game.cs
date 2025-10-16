using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MainData
{
    // 玩家当前所达到点位大小。
    private PointLevel _playerLevel;
    public PointLevel playerLevel
    {
        get { return _playerLevel; }
        set { _playerLevel = value; }
    }
    // 最大玩家等级
    private PointLevel _maxPlayerLevel;
    public PointLevel maxPlayerLevel
    {
        get { return _maxPlayerLevel; }
        set { _maxPlayerLevel = value; }
    }
    // 玩家是否到达终点或将到达终点
    private bool _arriveEnd;
    public bool arriveEnd
    {
        get { return _arriveEnd; }
        set { _arriveEnd = value; }
    }

    public void InitGameData()
    {
        this.gameResult = GameResult.common;
        this.playerGrid = new GridPos(0, 0);
        this.targetGrid = this.playerGrid;
        this.isMoving = false;
        this.playerDead = false;
        this.arriveEnd = false;
        this.playerLevel = dataBaseMgr.GetPlayerLevel();
        this.maxPlayerLevel = dataBaseMgr.GetMaxPlayerLevel();
    }
    
    public void stopGameData()
    {
        // CustomGlobalConfig.IsInGame = false;
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
    
    /// <summary>
    /// 根据位置获取ObstacleNode
    /// </summary>
    public ObstacleNode GetObstacleNodeAtGrid(int col, int row)
    {
        foreach (var kvp in obstacleNodes)
        {
            if (kvp.Value.col == col && kvp.Value.row == row)
            {
                return kvp.Key;
            }
        }

        return null;
    }

    /// <summary>
    /// 根据位置获取PointNode
    /// </summary>
    public PointNode GetPointNodeAtGrid(int col, int row)
    {
        foreach (var kvp in pointNodes)
        {
            if (kvp.Value.col == col && kvp.Value.row == row)
            {
                return kvp.Key;
            }
        }

        return null;
    }
}
