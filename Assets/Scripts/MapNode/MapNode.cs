using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNode : CustomUIComponent
{
    private MainData mainData => MainData.Instance;
    
    public TouchNode touchNode;
    [Header("层级节点")]
    [SerializeField] private GameObject floorLayer; // 地面层
    [SerializeField] private GameObject ObstacleLayer; // 障碍物层
    [SerializeField] private GameObject PointLayer; // 点位层
    [SerializeField] private GameObject playerLayer; // 玩家层
    public GameObject arrowNode; // 箭头节点
    [Header("预制件")]
    [SerializeField] private FloorNode floorPrefab; // 地板预制件
    [SerializeField] private PlayerNode playerPrefab; // 玩家预制件
    [SerializeField] private ObstacleNode obstaclePrefab; // 障碍物预制件
    [SerializeField] private PointNode pointPrefab; // 点位预制件
    
    public override void startComponent()
    {
        base.startComponent();
        this.InitMap();
    }

    public override void stopComponent()
    {
        base.stopComponent();
        this.RecycleMapChildren();
    }

    public void InitMap()
    {
        this.arrowNode.setActiveByCheck(false);
        this.RecycleMapChildren();
        this.rectTransform.sizeDelta = new Vector2(1000f, 1000f);
        var scale = (float)8 / DataBaseManager.Instance.curLevelConfig.col;
        this.rectTransform.localScale = new Vector3(scale, scale, scale);
        this.CreateMapChildren();
    }

    public void CreateMapChildren()
    {
        var curLevelConfig = DataBaseManager.Instance.curLevelConfig;   
        this.CreateFloorChildren(curLevelConfig);
        this.CreatePlayer(curLevelConfig);
        this.CreateObstacleChildren(curLevelConfig);
        this.CreatePointChildren(curLevelConfig);
    }

    public void CreateFloorChildren(LevelData levelData)
    {
        foreach (var floorData in levelData.mapData.floor)
        {
            int floorCol = floorData.pos.col; // 当前地板数据的列数
            int floorRow = floorData.pos.row; // 当前地板数据的行数
            
            Vector2 position = mainData.GetVector2(floorCol, floorRow);
            
            FloorNode floorNode = CacheManager.Instance.popCompent<FloorNode>(typeof(FloorNode).toString(), floorPrefab, floorLayer.transform);
            GridPos grid = new GridPos(floorCol, floorRow);
            mainData.floorNodes.Add(floorNode, grid);
            floorNode.InitFloorNode(floorData);
            RectTransform rectTransform = floorNode.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = position;
            }
        }
    }

    public void CreateObstacleChildren(LevelData levelData)
    {
        if (levelData.mapData == null || levelData.mapData.obstacle == null) return;

        foreach (var obstacleData in levelData.mapData.obstacle)
        {
            int obsCol = obstacleData.pos.col;
            int obsRow = obstacleData.pos.row;
            Vector2 position = mainData.GetVector2(obsCol, obsRow);
            
            ObstacleNode obstacleNode = CacheManager.Instance.popCompent<ObstacleNode>(
                typeof(ObstacleNode).toString(), 
                obstaclePrefab, 
                ObstacleLayer.transform);
            mainData.obstacleNodes.Add(obstacleNode,new GridPos(obsCol, obsRow));   
            obstacleNode.InitObstacleNode(obstacleData.type);
            
            RectTransform rt = obstacleNode.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = position;
            }
        }
    }

    public void CreatePointChildren(LevelData levelData)
    {
        if (levelData.mapData == null || levelData.mapData.obstacle == null) return;

        for (var s = 0; s < levelData.mapData.point.Length; s++)
        { 
            PointData pointData = levelData.mapData.point[s];
            if (pointData.pos == null || pointData.pos.Length == 0) continue;

            for (int i = 0; i < pointData.pos.Length; i++)
            {
                int pointCol = pointData.pos[i].col;
                int pointRow = pointData.pos[i].row;
                
                Vector2 position = mainData.GetVector2(pointCol, pointRow);
                PointNode pointNode = CacheManager.Instance.popCompent<PointNode>(
                    typeof(PointNode).toString(),
                    pointPrefab, 
                    PointLayer.transform);
                mainData.pointNodes.Add(pointNode, new GridPos(pointCol, pointRow));
                pointNode.InitPointNode(pointData.level, s);  // 设置点位信息。
                
                RectTransform rt = pointNode.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = position;
                }
            }
        }
    }

    public void CreatePlayer(LevelData levelData)
    {
        int pCol = levelData.startPos.col; // 玩家的列数
        int pRow = levelData.startPos.row; // 玩家的行数
        
        Vector2 position = mainData.GetVector2(pCol, pRow);
        mainData.player = CacheManager.Instance.popCompent<PlayerNode>(typeof(PlayerNode).toString(), playerPrefab, playerLayer.transform);
        mainData.playerGrid = new GridPos(pCol, pRow);
        RectTransform rt = mainData.player.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = position;
        }
    }
    
    
    
    /// <summary>
    /// 回收子层级，FloorNode是一种地面层级，有不同的地块
    /// </summary>
    public void RecycleMapChildren()
    {
        if (mainData.player != null)
        {
            Destroy(mainData.player.gameObject);
        }
        mainData.player = null;
        
        foreach (var floorNode in mainData.floorNodes.Keys)
        {
            CacheManager.Instance.pushCompent<FloorNode>(typeof(FloorNode).toString(), floorNode);
        }
        mainData.floorNodes.Clear();
        
        foreach (var obstacleNode in mainData.obstacleNodes.Keys)
        {
            CacheManager.Instance.pushCompent<ObstacleNode>(typeof(ObstacleNode).toString(), obstacleNode);
        }
        mainData.obstacleNodes.Clear();
        foreach (var pointNode in mainData.pointNodes.Keys)
        {
            CacheManager.Instance.pushCompent<PointNode>(typeof(PointNode).toString(), pointNode);
        }
        mainData.pointNodes.Clear();
    }
    
    
    /// <summary>
    /// 检查并移动玩家
    /// </summary>
    /// <param name="direction">移动方向</param>
    /// <param name="addWayPoint">是否添加途径点位</param>
    public void CheckAndMovePlayer(Direction direction, bool addWayPoint = true)
    {
        if (mainData.player == null || mainData?.playerGrid == null) return;
        
        // 检查路径
        GridPos targetGrid = CheckPath(mainData.playerGrid, direction);
        mainData.targetGrid = targetGrid;
        // 通过格子位置获取坐标位置
        mainData.targetPos = mainData.GetVector2(targetGrid.col, targetGrid.row);

        if (targetGrid.col != mainData.playerGrid.col || targetGrid.row != mainData.playerGrid.row)
        {
            if (addWayPoint)
            {
                //mainData.AddWayPoint(mainData.playerGrid); // 途径点位可能用于悔步
            }
            // 移动玩家到新位置
            MovePlayerToPosition(targetGrid.col, targetGrid.row);
        }
        else
        {
            // 写在这里实在是太难受了。
            touchNode.enableTouchEvents = true;
        }
    }

    /// <summary>
    /// 移动玩家到指定位置
    /// </summary>
    /// <param name="col">目标行</param>
    /// <param name="row">目标列</param>
    /// <param name="cb"></param>
    public void MovePlayerToPosition(int col, int row, Action cb = null)
    {
        if (mainData.player == null) return;
        mainData.targetGrid = new GridPos(col, row);
        mainData.targetPos = mainData.GetVector2(col, row);
        if (mainData.hasStepLimit && mainData.remainStep > 0) // 是否有步数限制且剩余步数大于0
        {
            mainData.remainStep -= 1;
        }

        mainData.player.StartMovement();
    }

    /// <summary>
    /// 移动之前检查路径
    /// </summary>
    /// <param name="gridPos"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public GridPos CheckPath(GridPos startPos, Direction direction)
    {
        int currentCol = startPos.col;
        int currentRow = startPos.row;
        // 记录开始的位置
        int lastValidCol = currentCol;
        int lastValidRow = currentRow;
        
        // 按距离从近到远逐格检查
        int step = 1;
        while (true)
        {
            // 根据方向和步数计算下一个位置
            int nextCol = currentCol;
            int nextRow = currentRow;

            switch (direction)
            {
                case Direction.Up:
                    nextRow = currentRow - step;
                    break;
                case Direction.Down:
                    nextRow = currentRow + step;
                    break;
                case Direction.Left:
                    nextCol = currentCol - step;
                    break;
                case Direction.Right:
                    nextCol = currentCol + step;
                    break;
                default:
                    return new GridPos(lastValidCol, lastValidRow); 
            }
            FloorNode flNode = mainData.curFloor;
            if (nextCol < 0 || nextCol >= DataBaseManager.Instance.curLevelConfig.col ||
                nextRow < 0 || nextRow >= DataBaseManager.Instance.curLevelConfig.row)
            {
                // 如果下一个位置是超过边界的且这个flNode仍能够离开去下一个位置那就玩家死亡。
                if (flNode.LeaveSelf(direction))
                {
                    mainData.playerDead = true; // 玩家死亡
                    // ERROR 到达边界，返回最后一个有效位置
                    return new GridPos(nextCol, nextRow);
                }
            }

            if (lastValidCol == mainData.playerGrid.col && lastValidRow == mainData.playerGrid.row)
            {
                if (!flNode.LeaveSelf(direction)) // 不能离开直接返回
                {
                    return new GridPos(lastValidCol, lastValidRow);
                }
            }
            
            FloorNode floorNode = mainData.GetFloorNodeAtGrid(nextCol, nextRow);
            ObstacleNode obstacleNode = mainData.GetObstacleNodeAtGrid(nextCol, nextRow);
            PointNode pointNode = mainData.GetPointNodeAtGrid(nextCol, nextRow);
            // 如果没有找到FloorNode，说明该位置不可通行
            if (floorNode == null)
            {
                return new GridPos(lastValidCol, lastValidRow);
            }
            
            // 检查FloorNode是否可通过
            if (floorNode.isCanPass(direction, obstacleNode, pointNode) == PassState.Dont)
            {
                return new GridPos(lastValidCol, lastValidRow);
            }
            else if (floorNode.isCanPass(direction, obstacleNode, pointNode) == PassState.Stay)
            {
                // 检查FloorNode是否会导致死亡
                if (floorNode.isCanDead(pointNode))
                {
                    mainData.playerDead = true;
                }
                return new GridPos(nextCol, nextRow);
            }
            else
            {
                // 如果可通过，更新最后一个有效位置，继续检查下一个格子
                lastValidCol = nextCol;
                lastValidRow = nextRow;
                step++;
            }
        }
        // 达到最大步数仍未终止，返回最后有效位置（避免无限循环）
        return new GridPos(lastValidCol, lastValidRow);
    }

    /// <summary>
    /// 停下来之后检查游戏的结果
    /// </summary>
    public void CheckGameResult()
    {
        if (mainData.playerDead)
        {
            // 玩家死亡
            mainData.gameResult = GameResult.fail;
            Debug.Log("Game Fail"); 
            return;
        }
        this.CheckPlayerLevel();
        if (mainData.arriveEnd)
        {
            mainData.gameResult = GameResult.win;
            UIManager.Instance.OpenUI<WinUI>();
            return;
        }
        
        // TODO 步数限制和事件限制。
        touchNode.enableTouchEvents = true;
    }

    public void CheckPlayerLevel()
    {
        int curCol = mainData.playerGrid.col;
        int curRow = mainData.playerGrid.row;
        PointNode curPoint = mainData.GetPointNodeAtGrid(curCol, curRow);
        if (curPoint != null && curPoint.GetPointType() == mainData.playerLevel)
        {
            int commonId = curPoint.commonId;
            List<PointNode> list = new List<PointNode>();
            foreach (var p in mainData.pointNodes.Keys)
            {
                // 标识id相同，是同一组，同时消除
                if (p.commonId == commonId)
                {
                    list.Add(p);
                }
            }
            foreach (var c in list)
            {
                mainData.pointNodes.Remove(c);
                CacheManager.Instance.pushCompent<PointNode>(typeof(PointNode).toString(), c);
            }
        }

        bool hasSameLevelPoint = false;
        PointLevel currentLevelInt = mainData.playerLevel;
        
        foreach (var kvp in mainData.pointNodes)
        {
            PointNode pointNode = kvp.Key;
            if (pointNode != null && pointNode.GetPointType() == currentLevelInt)
            {
                hasSameLevelPoint = true;
                break;
            }
        }

        if (!hasSameLevelPoint && mainData.playerLevel < mainData.maxPlayerLevel)
        {
            mainData.playerLevel = (PointLevel)(mainData.playerLevel.toInt() + 1);
        }

        if (mainData.pointNodes.Count == 0)
        {
            mainData.arriveEnd = true;
        }
    }
}
