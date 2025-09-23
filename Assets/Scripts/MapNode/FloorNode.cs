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
        this.fData = data.DeepCopy();
        this.floorType = fData.type;
        this.targetFloorNode = mainData.GetFloorNodeAtGrid(fData.target.col, fData.target.row);
    }
    
    public void Reset()
    {
       
    }
    
    /// <summary>
    /// 能否离开自身点位
    /// </summary>
    /// <returns></returns>
    public bool LeaveSelf(Direction direction)
    {
        return this.floor.LeaveSelf(direction);
    }
    /// <summary>
    /// 是否可以通过
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="obNode"></param>
    /// <param name="ptNode"></param>
    /// <returns></returns>
    public PassState isCanPass(Direction direction)
    {
        return this.floor.isCanPass(direction);
    }
    /// <summary>
    /// 是否接触会死亡
    /// </summary>
    /// <returns></returns>
    public bool isCanDead()
    {
        return this.floor.isCanDead();
    }

}
