using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataBaseManager
{
}
public struct GridPos
{
    public int col;
    public int row;

    public GridPos(int col, int row)
    {
        this.col = col;
        this.row = row;
    }
}

// 格子类型枚举
public enum FloorType
{
    FLOOR = 0,    // 地板
    FAIL = 1,  // 禁入区域，接触掉落
    EDGE = 2, // 边缘地块，不占格子，拦截
    TRAP = 3, // 陷阱，接触失败
    NIL = 4, // 空白块，接触失败
    SPIKES = 5, // 地刺，接触失败，间歇性变化
    COLLAPSE = 6, // 崩塌，首次接触为空地，后续变为空白
    TELEPORT = 7, // 传送，接触后传送至指定位置
    DIRECTION = 8, // 方向块，接触后改变角色移动方向
    ROTATION = 9, // 旋转块，有机关触发旋转
}

// 障碍物类型枚举
public enum ObstacleType
{
    COMMON = 0,  // 普通障碍物
}

// 点位类型枚举
public enum PointLevel
{
    A = 1,  // A级
    B = 2,  // B级
    C = 3,  // C级
    D = 4,   // D级
    E = 5,  // E级
    F = 6   // F级
}

// 地板数据类
public class FloorData
{
    public FloorType type { get; set; }
    public GridPos pos { get; set; }  // GridPos 格式的位置坐标

    public int[] obstacle { get; set; } // 边缘障碍物方位，不占格子，0上，1右，2下，3右

    public float time { get; set; } // 地刺间歇性变化的时间，单位秒

    public GridPos target { get; set; } // GridPos 目标位置，接触后传送至指定位置，或者触发的机关的位置

    public int direction { get; set; } // 改变角色移动方向，0上，1右，2下，3右

    public int isRevolve { get; set; } // 是否旋转，触发机关旋转; 1是旋转位，0或无是触发位。

    public int rotation { get; set; } // 旋转方向，0横向，1纵向

    public FloorData DeepCopy()
    {
        var copy = new FloorData
        {
            type = this.type,
            pos = this.pos,
            time = this.time,
            target = this.target,
            direction = this.direction,
            isRevolve = this.isRevolve,
            rotation = this.rotation,
            obstacle = this.obstacle?.ToArray() // 拷贝数组
        };
        return copy;
    }
}

// 障碍物数据类
public class ObstacleData
{
    public ObstacleType type { get; set; }
    public GridPos pos { get; set; }  // GridPos 格式的位置坐标
}

// 点位数据类
public class PointData
{
    public PointLevel level { get; set; }
    public GridPos[] pos { get; set; }  // GridPos 格式的位置坐标
}



// 地图数据类
public class MapData
{
    public FloorData[] floor { get; set; }      // 地板位置
    public ObstacleData[] obstacle { get; set; } // 障碍物位置
    public PointData[] point { get; set; }      // 点位位置
}

// 关卡数据类
public class LevelData
{
    public int id { get; set; }                    // 关卡id
    public PointLevel playerLevel { get; set; }     // 玩家点位等级(可选，默认一级)
    public int row { get; set; }                   // 行数
    public int col { get; set; }                   // 列数
    public GridPos startPos { get; set; }           // 角色开始位置 GridPos
    public MapData mapData { get; set; }          // 地图数据
    public int stepLimit { get; set; }            // 步数上限（可选）
    public float timeLimit { get; set; }       // 时间限制（默认180s）
}