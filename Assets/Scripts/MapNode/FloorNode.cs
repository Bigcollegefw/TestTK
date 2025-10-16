using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PassState
{
    Dont = 0,// 不能进入
    Stay = 1,// 可以进入，但不能通过
    Pass = 2 // 可以通过
}

/// <summary>
/// 具体的地块
/// </summary>
public class FloorNode : CustomUIComponent
{
    public List<GameObject> prefabs;
    /// <summary>
    /// 地块数据
    /// </summary>
    public FloorData fData;
    /// <summary>
    /// 地块类型
    /// </summary>
    public FloorType floorType;
    /// <summary>
    /// 目标地块节点
    /// </summary>
    public FloorNode targetFloorNode;
    private MainData mainData => MainData.Instance;
    
    public override void stopComponent()
    {
        this.Reset();
    }

    private Floor floor;
    
    public void InitFloorNode(FloorData data)
    {
        this.Reset();
        this.fData = data.DeepCopy(); // 这里为什么需要深拷贝？我应该直接传fData.type就不用多这一步了
        this.floorType = fData.type;
        this.targetFloorNode = mainData.GetFloorNodeAtGrid(fData.target.col, fData.target.row);
        GameObject floorPrefab = this.prefabs[this.floorType.toInt()]; // 根据预制体加载出具体的地板节点
        this.floor = CacheManager.Instance.popCompent<Floor>(this.floorType.toString(), 
            floorPrefab.GetComponent<Floor>(), this.transform);
    }
    
    public void Reset()
    {
        if (this.floor != null)
        {
            CacheManager.Instance.pushCompent<Floor>(this.floorType.toString(), this.floor);
            this.floor = null;  
        }
        
    }
    
    /// <summary>
    /// 能否离开自身点位
    /// </summary>
    /// <returns></returns>
    public bool LeaveSelf(Direction direction)
    {
        return this.floor.LeaveSelf(GetCombinedObstacle(),direction);
    }
    /// <summary>
    /// 是否可以通过
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="obNode"></param>
    /// <param name="ptNode"></param>
    /// <returns></returns>
    public PassState isCanPass(Direction direction,ObstacleNode obNode = null,PointNode ptNode = null)
    {
        return this.floor.isCanPass(GetCombinedObstacle(), direction, obNode, ptNode);
    }
    /// <summary>
    /// 是否接触会死亡
    /// </summary>
    /// <returns></returns>
    public bool isCanDead(PointNode ptNode = null)
    {
        return this.floor.isCanDead(ptNode);
    }

    public int[] GetCombinedObstacle()
    {
        HashSet<int> combinedSet = new HashSet<int>();  
        
        // 添加原始阻挡方向
        if (fData != null && fData.obstacle != null)
        {
            foreach (int ob in fData.obstacle)
            {
                combinedSet.Add(ob);    
            }
        }
        return combinedSet.ToArray();
    }
    
}
