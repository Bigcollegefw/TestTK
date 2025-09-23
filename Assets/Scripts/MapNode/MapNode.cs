using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNode : CustomUIComponent
{
    public TouchNode touchNode;
    [Header("层级节点")]
    [SerializeField] private GameObject floorLayer; // 地面层
    [SerializeField] private GameObject ObstacleLayer; // 障碍物层
    [SerializeField] private GameObject PointLayer; // 点位层
    [SerializeField] private GameObject playerLayer; // 玩家层
    public GameObject arrowNode; // 箭头节点
    [Header("预制件")]
    [SerializeField] private FloorNode floorPrefab; // 地板预制件

    /// <summary>
    /// 检查并移动玩家
    /// </summary>
    /// <param name="direction">移动方向</param>
    /// <param name="addWayPoint">是否添加途径点位</param>
    public void CheckAndMovePlayer(Direction direction, bool addWayPoint = true)
    {
        //TODO
    }
}
