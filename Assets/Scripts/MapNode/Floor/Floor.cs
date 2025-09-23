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

}
