using System.Collections.Generic;
using UnityEngine;

// 游戏状态适配器 - 将Unity的MainData转换为求解器的TKGameState
public class GameStateAdapter
{
    // 将MainData转换为TKGameState
    public static TKGameState ConvertGameStateToTK(MainData mainData, LevelData levelData)
    {
        TKGameState state = new TKGameState();
        
        // 玩家位置
        state.PlayerPosition = mainData.playerGrid.ToArray();
        
        // 玩家朝向（如果是None，默认为Right）
        if (mainData.playerDirection == Direction.None)
        {
            state.PlayerDirection = Direction.Right;  // 默认向右
        }
        else
        {
            state.PlayerDirection = mainData.playerDirection;
        }
        
        // 已收集的点位组
        // 重要：pointNodes字典只包含未收集的点位！
        // 所以我们需要找出哪些点位已经被收集（不在字典中）
        state.CollectedPointGroups = new HashSet<string>();
        if (levelData.mapData.point != null)
        {
            foreach (var pointGroup in levelData.mapData.point)
            {
                foreach (GridPos pos in pointGroup.pos)
                {
                    // 检查这个点位是否还在pointNodes中（未收集）
                    bool isUncollected = false;
                    foreach (var kvp in mainData.pointNodes)
                    {
                        PointNode pointNode = kvp.Key;
                        GridPos nodePos = kvp.Value;
                            
                        // 检查位置和类型是否匹配
                        if (nodePos.col == pos.col && nodePos.row == pos.row &&
                            pointNode.GetPointType() == pointGroup.level)
                        {
                            isUncollected = true;
                            break;
                        }
                    }
                        
                    // 如果不在pointNodes中，说明已经被收集
                    if (!isUncollected)
                    {
                        string groupId = GameStateManager.GetPointGroupId(pointGroup.level, pos.ToArray());
                        state.CollectedPointGroups.Add(groupId);
                    }
                }
            }
        }
        
        // 已解锁的等级
        state.UnlockedLevels = new HashSet<PointLevel>();
        state.UnlockedLevels.Add(mainData.playerLevel);
        
        // 添加所有小于等于当前等级的等级
        for (PointLevel level = PointLevel.A; level <= mainData.playerLevel; level++)
        {
            state.UnlockedLevels.Add(level);
        }
        
        // 已坍塌的地块
        // state.CollapsedTiles = new HashSet<string>(mainData.collapsedTiles);
        
        // 地刺状态 - 从FloorNode中获取
        state.SpikeStates = new Dictionary<string, bool>();
        foreach (var kvp in mainData.floorNodes)
        {
            FloorNode floorNode = kvp.Key;
            GridPos gridPos = kvp.Value;
                
            if (floorNode != null && floorNode.fData.type == FloorType.SPIKES)
            {
                string key = $"{gridPos.col},{gridPos.row}";
                // 地刺的Floor组件控制显示状态
                // 简化处理：假设地刺当前是激活的
                state.SpikeStates[key] = true;
            }
        }
        
        // 旋转门状态
        state.RotationStates = new Dictionary<string, int>();
        if (levelData.mapData.floor != null)
        {
            foreach (var floor in levelData.mapData.floor)
            {
                if (floor.type == FloorType.ROTATION)
                {
                    string gateKey = $"{floor.target.col},{floor.target.row}";
                    state.RotationStates[gateKey] = floor.rotation;
                }
            }
        }
        
        // 游戏时间和步数
        // MainData没有gameTime字段，使用倒计时时间或设为0
        state.GameTime = 0;
        
        // MainData没有moveHistory字段，使用剩余步数计算
        if (levelData.stepLimit > 0)
        {
            // 步数计数 = 步数上限 - 剩余步数;
            state.StepCount = levelData.stepLimit - mainData.remainStep;
        }
        else
        {
            state.StepCount = 0;
        }
            
        // 父状态和最后移动
        state.Parent = null;
        state.LastMove = null;
            
        return state;
        
    }
}
