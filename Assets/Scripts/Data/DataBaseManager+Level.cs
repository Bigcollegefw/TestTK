using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DataBaseManager
{
     // 解析LevelData的方法
    private LevelData ParseLevelData(Dictionary<string, object> levelDic)
    {
        try
        {
            LevelData levelData = new LevelData();

            // 解析基本属性
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
                            //mapData.obstacle[i] = ParseObstacleData(obstacleObj);
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
                            //mapData.point[i] = ParsePointData(pointObj);
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
}
