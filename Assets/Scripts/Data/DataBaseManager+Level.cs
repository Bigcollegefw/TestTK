using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DataBaseManager: SingletonData<DataBaseManager>
{
    private string levelJson;
    
    /// <summary>
    /// 获取本关卡玩家初始等级
    /// </summary>
    /// <returns></returns>
    public PointLevel GetPlayerLevel()
    {
        return this.curLevelConfig.playerLevel;
    }

    /// <summary>
    /// 获取本关卡玩家等级上限
    /// </summary>
    /// <returns></returns>
    public PointLevel GetMaxPlayerLevel()
    {
        var list = this.curLevelConfig.mapData.point;
        var max = PointLevel.A;
        for (var i = 0; i < list.Length; i++)
        {
            if (list[i].level >= max)
            {
                max = list[i].level;
            }
        }
        return max;
    }
    
    /// <summary>
    /// 获取本关卡步数上限
    /// </summary>
    /// <returns></returns>
    public int GetStepLimit()
    {
        return this.curLevelConfig.stepLimit;
    }
    
     // 解析LevelData的方法
    private LevelData ParseLevelData(Dictionary<string, object> levelDic)
    {
        try
        {
            LevelData levelData = new LevelData();

            // 解析基本属性,这个时候的levelDic字典里面就有很多键值对 如：“mapData”:具体的内容,"id":1；
            if (levelDic.ContainsKey("id"))
                levelData.id = Convert.ToInt32(levelDic["id"]);

            if (levelDic.ContainsKey("playerLevel"))
                levelData.playerLevel = (PointLevel)Convert.ToInt32(levelDic["playerLevel"]);

            if (levelDic.ContainsKey("row"))
                levelData.row = Convert.ToInt32(levelDic["row"]);

            if (levelDic.ContainsKey("col"))
                levelData.col = Convert.ToInt32(levelDic["col"]);

            if (levelDic.ContainsKey("stepLimit"))
                levelData.stepLimit = Convert.ToInt32(levelDic["stepLimit"]);

            if (levelDic.containsKey("timeLimit"))
            {
                levelData.timeLimit = Convert.ToInt32(levelDic["timeLimit"]);
            }

            // 解析开始位置
            if (levelDic.ContainsKey("startPos"))
            {
                var startPosArray = levelDic["startPos"] as List<object>;
                if (startPosArray != null && startPosArray.Count >= 2)
                {
                    levelData.startPos = new GridPos(
                        Convert.ToInt32(startPosArray[0]),
                        Convert.ToInt32(startPosArray[1])
                    );
                }
            }

            // 解析地图数据
            if (levelDic.ContainsKey("mapData"))
            {
                var mapDic = levelDic["mapData"] as Dictionary<string, object>; 
                if (mapDic != null)
                {
                    levelData.mapData = ParseMapData(mapDic);
                }
            }

            return levelData;
        }
        catch (Exception e)
        {
            Debug.LogError($"解析LevelData失败: {e.Message}");
            return null;
        }
    }
    
    // 添加解析MapData的辅助方法
    private MapData ParseMapData(Dictionary<string, object> mapDic)
    {
        try
        {
            //// 这个时候的字典里面就有很多键值对 如：“floor”:floor的type和pos,"obstacle":obstacle的type和pos
            MapData mapData = new MapData();
            if (mapDic.ContainsKey("floor"))
            {
                var floorArray = mapDic["floor"] as List<object>;
                if (floorArray != null)
                {
                    mapData.floor = new FloorData[floorArray.Count];
                    for (int i = 0; i < floorArray.Count; i++)
                    {
                        var floorObj = floorArray[i] as Dictionary<string, object>;
                        if (floorObj != null)
                        {
                            mapData.floor[i] = ParseFloorData(floorObj);
                        }
                    }
                }
            }
            // 解析障碍物数据
            if (mapDic.ContainsKey("obstacle"))
            {
                var obstacleArray = mapDic["obstacle"] as List<object>;
                if (obstacleArray != null)
                {
                    mapData.obstacle = new ObstacleData[obstacleArray.Count];
                    for (int i = 0; i < obstacleArray.Count; i++)
                    {
                        var obstacleObj = obstacleArray[i] as Dictionary<string, object>;
                        if (obstacleObj != null)
                        {
                            mapData.obstacle[i] = ParseObstacleData(obstacleObj);
                        }
                    }
                }
            }
            // 解析点位数据
            if (mapDic.ContainsKey("point"))
            {
                var pointArray = mapDic["point"] as List<object>;
                if (pointArray != null)
                {
                    mapData.point = new PointData[pointArray.Count];
                    for (int i = 0; i < pointArray.Count; i++)
                    {
                        var pointObj = pointArray[i] as Dictionary<string, object>;
                        if (pointObj != null)
                        {
                            mapData.point[i] = ParsePointData(pointObj);
                        }
                    }
                }
            }
            return mapData;
        }
        catch (Exception e)
        {
            Debug.LogError($"解析MapData失败: {e.Message}");
            return null;
        }
    }
        // 添加解析其他数据类型的辅助方法
    private FloorData ParseFloorData(Dictionary<string, object> floorDic)
    {
        //这个时候的字典里面就有很多键值对 如：“type”:4,"pos":[0,0]
        FloorData floorData = new FloorData();
        if (floorDic.ContainsKey("type"))
            floorData.type = (FloorType)Convert.ToInt32(floorDic["type"]);
        if (floorDic.ContainsKey("pos"))
        {
            var posArray = floorDic["pos"] as List<object>;
            if (posArray != null && posArray.Count >= 2)
            {
                floorData.pos = new GridPos(
                    Convert.ToInt32(posArray[0]),
                    Convert.ToInt32(posArray[1])
                );
            }
        }
        // 边缘障碍方位
        if (floorDic.ContainsKey("obstacle"))
        {
            var obstacleArray = floorDic["obstacle"] as List<object>;
            if (obstacleArray != null && obstacleArray.Count > 0)
            {
                floorData.obstacle = new int[obstacleArray.Count];
                for (int i = 0; i < obstacleArray.Count; i++)
                {
                    floorData.obstacle[i] = Convert.ToInt32(obstacleArray[i]);
                }
            }
        }

        // 地刺间歇时间（秒）
        if (floorDic.ContainsKey("time"))
        {
            try
            {
                floorData.time = Convert.ToSingle(floorDic["time"]);
            }
            catch
            {
                // 兼容整型写法
                floorData.time = Convert.ToInt32(floorDic["time"]);
            }
        }

        // 目标位置/机关位置 [x, y]
        if (floorDic.ContainsKey("target"))
        {
            var targetArray = floorDic["target"] as List<object>;
            if (targetArray != null && targetArray.Count >= 2)
            {
                int col = Convert.ToInt32(targetArray[0]);
                int row = Convert.ToInt32(targetArray[1]);
                // TODO 补丁：如果type==9，target的row和col都+1
                if (floorData.type == FloorType.ROTATION)
                {
                    col += 1;
                    row += 1;
                }
                floorData.target = new GridPos(col, row);
            }
        }

        // 方向（0上，1右，2下，3左）
        if (floorDic.ContainsKey("direction"))
        {
            floorData.direction = Convert.ToInt32(floorDic["direction"]);
        }

        // 是否旋转位
        if (floorDic.ContainsKey("isRevolve"))
        {
            var value = floorDic["isRevolve"];
            floorData.isRevolve = Convert.ToInt32(value);
        }

        // 旋转方向（0横向，1纵向）
        if (floorDic.ContainsKey("rotation"))
        {
            floorData.rotation = Convert.ToInt32(floorDic["rotation"]);
        }
        return floorData;
    }

    private ObstacleData ParseObstacleData(Dictionary<string, object> obstacleDic)
    {
        ObstacleData obstacleData = new ObstacleData();
        // 这个时候的字典里面就有很多键值对 如：“type”:4,"pos":[0,0]
        if (obstacleDic.ContainsKey("type"))
        {
            obstacleData.type = (ObstacleType)Convert.ToInt32(obstacleDic["type"]);
        }
        if (obstacleDic.ContainsKey("pos"))
        {
            var posArray = obstacleDic["pos"] as List<object>;
            if (posArray != null && posArray.Count >= 2)
            {
                obstacleData.pos = new GridPos(
                    Convert.ToInt32(posArray[0]),
                    Convert.ToInt32(posArray[1])
                );
            }
        }
        return obstacleData;
    }

    private PointData ParsePointData(Dictionary<string, object> pointDic)
    {
        PointData pointData = new PointData();
        // 这个时候的字典里面就有很多键值对 如：“type”:4,"pos":[0,0]
        if(pointDic.ContainsKey("type"))
        {
            pointData.level = (PointLevel)Convert.ToInt32(pointDic["type"]);
        }
        // 这里配置的时候可能“type”:4,"pos":[[x1,y1], [x2,y2], ...]，所以要多考虑一种情况
        if (pointDic.ContainsKey("pos"))
        {
            var posArray = pointDic["pos"] as List<object>;
            if (posArray != null && posArray.Count > 0)
            {
                var firstElement = posArray[0];
                if (firstElement is List<object>)
                {
                    // 多个位置的情况：[[x1,y1], [x2,y2], ...]
                    pointData.pos = new GridPos[posArray.Count];
                    for (int i = 0; i < posArray.Count; i++)
                    {
                        var singlePos = posArray[i] as List<object>;
                        if (singlePos != null && singlePos.Count >= 2)
                        {
                            pointData.pos[i] = new GridPos(
                                Convert.ToInt32(singlePos[0]),
                                Convert.ToInt32(singlePos[1])
                            );
                        }
                    }
                }
                else
                {
                    // 单个位置的情况：[x, y]
                    if (posArray.Count >= 2)
                    {
                        pointData.pos = new GridPos[1];
                        pointData.pos[0] = new GridPos(
                            Convert.ToInt32(posArray[0]),
                            Convert.ToInt32(posArray[1])
                        );
                    }
                }
            }
        }
        return pointData;
    }
}
