using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorNil : Floor  // 空白块
{
    public override PassState isCanPass(int[] obstacle, Direction direction, ObstacleNode obNode = null, PointNode ptNode = null)
    {
        return PassState.Stay;
    }

    public override bool isCanDead(PointNode ptNode = null)
    {
        return true;
    }
}
