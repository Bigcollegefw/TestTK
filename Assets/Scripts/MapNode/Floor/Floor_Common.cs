using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloorCommon : Floor
{
    public List<GameObject> floorSprites;

    public override void InitFloor()
    {
        base.InitFloor();
        return;
    }
    
    // isCanPass：判断 “进入目标格子是否可行”
    public override PassState isCanPass(int[] obstacle,Direction direction, ObstacleNode obNode = null, PointNode ptNode = null)
    {
        if (obNode != null)
        {
            return PassState.Dont;  //Dont = 0,不能进入
        }
        //public int[] obstacle { get; set; } // 边缘障碍物方位，不占格子，0上，1右，2下，3右
        if (obstacle != null && obstacle.Length > 0)
        {
            if (direction == Direction.Up)
            {
                if (obstacle.Contains(2))
                {
                    return PassState.Dont; 
                }else if (obstacle.Contains(0))
                {
                    return PassState.Stay; // 停留
                }
                else
                {
                    return PtPass(ptNode);
                }
            }else if (direction == Direction.Down)
            {
                if (obstacle.Contains(0)) { return PassState.Dont; }
                else if (obstacle.Contains(2))
                {
                    return PassState.Stay;
                }
                else
                {
                    return PtPass(ptNode);
                }
            }else if (direction == Direction.Left)
            {
                if (obstacle.Contains(1)) { return PassState.Dont; }
                else if (obstacle.Contains(3))
                {
                    return PassState.Stay;
                }
                else
                {
                    return PtPass(ptNode);
                }
            }
            else if (direction == Direction.Right)
            {
                if (obstacle.Contains(3)) { return PassState.Dont; }
                else if (obstacle.Contains(1))
                {
                    return PassState.Stay;
                }
                else
                {
                    return PtPass(ptNode);
                }
            }
        }
        else
        {
            return PtPass(ptNode);
        }
        return PassState.Pass;
        
    }

    
    public override bool isCanDead(PointNode ptNode = null)
    {
        if (ptNode != null && ptNode.isCanDead())
        {
            return true;
        }
        return false;
    }

    // LeaveSelf：判断 “离开当前格子是否可行”, 所以只用考虑当前格子的四面是否有边缘障碍物方位
    public override bool LeaveSelf(int[] obstacle, Direction direction)
    {
        if (obstacle != null && obstacle.Length > 0)
        {
            if (direction == Direction.Up)
            {
                if (!obstacle.Contains(0)) { return true; }
                else
                {
                    return false;
                }

            }
            else if (direction == Direction.Down)
            {
                if (!obstacle.Contains(2)) { return true; }
                else
                {
                    return false;
                }
            }
            else if (direction == Direction.Left)
            {
                if (!obstacle.Contains(3)) { return true; }
                else
                {
                    return false;
                }
            }
            else if (direction == Direction.Right)
            {
                if (!obstacle.Contains(1)) { return true; }
                else
                {
                    return false;
                }
            }
        }
        return true;
    }
}
