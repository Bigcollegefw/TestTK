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
    
    // 玩家是否到达终点或将到达终点
    private bool _arriveEnd;
    public bool arriveEnd
    {
        get { return _arriveEnd; }
        set { _arriveEnd = value; }
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
