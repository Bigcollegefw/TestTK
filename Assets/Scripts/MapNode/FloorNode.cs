using UnityEngine;

public enum PassState
{
    Dont = 0,// 不能进入
    Stay = 1,// 可以进入，但不能通过
    Pass = 2 // 可以通过
}

public class FloorNode : CustomUIComponent
{
    public override void stopComponent()
    {
        this.Reset();
    }

    private Floor floor;
    
    public void InitFloorNode(FloorData data)
    {
       
    }
    
    public void Reset()
    {
       
    }
}
