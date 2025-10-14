using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : CustomUIComponent
{
    public FloorType floorType;

    /// <summary>
    /// 父节点transform
    /// </summary>
    public Transform parentTransform;

    /// <summary>
    /// 父节点FloorNode
    /// </summary>
    public FloorNode parentFloorNode;




    public override void startComponent()
    {
        this.InitFloor();
    }

    public override void stopComponent()
    {
        this.ResetFloor();
    }

    void Update() { }
    public virtual void InitFloor()
    {
        this.parentTransform = this.rectTransform.parent;
        this.parentFloorNode = this.parentTransform.GetComponent<FloorNode>();
    }

    public virtual void ResetFloor()
    {

    }
    
    /// <summary>
    /// 能否离开自身点位
    /// </summary>
    /// <param name="obstacle"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public virtual bool LeaveSelf(int[] obstacle,Direction direction)
    {
        return true;
    }
    /// <summary>
    /// 是否可以通过
    /// </summary>
    /// <param name="obstacle"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public virtual PassState isCanPass(int[] obstacle, Direction direction, ObstacleNode obNode = null, PointNode ptNode = null)
    {
        return PassState.Pass;
    }
    /// <summary>
    /// 是否接触会死亡
    /// </summary>
    /// <returns></returns>
    public virtual bool isCanDead(PointNode ptNode = null)
    {
        return false;
    }
    
    // 是否能够穿过point点
    public virtual PassState PtPass(PointNode ptNode)
    {
        if (ptNode != null)
        {
            return PassState.Stay;
        }
        else
        {
            return PassState.Pass;
        }
    }
}
