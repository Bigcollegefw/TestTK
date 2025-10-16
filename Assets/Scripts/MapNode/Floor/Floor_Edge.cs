using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorEdge : FloorCommon
{
    public List<GameObject> BgList;

    // 这个会在FloorNode的popCompent下驱动
    public override void InitFloor()
    {
        base.InitFloor();
        BgList.ForEach(obj => obj.SetActive(false));
        this.floorType = FloorType.EDGE; ;
        if (!parentFloorNode) return;
        var obstacleList = parentFloorNode.fData.obstacle;
        if (obstacleList != null && obstacleList.Length > 0)
        {
            for (int i = 0; i < obstacleList.Length; i++)
            {
                int bgIndex = obstacleList[i];
                if (bgIndex >= 0 && bgIndex < BgList.Count)
                {
                    BgList[bgIndex].setActiveByCheck(true);
                }
            }
        }
    }
    
    public override void ResetFloor()
    {
        base.ResetFloor();
        BgList.ForEach(e => { e.setActiveByCheck(false); });
    }
}
