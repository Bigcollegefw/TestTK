
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;

public class GameStateManager
{

    // 检查是否为胜利状态。
    public static bool IsWinningState(TKGameState state, LevelData levelData)
    {
        if (levelData.mapData.point == null || levelData.mapData.point.Length == 0)
        {
            return false;
        } 
        
        // 检查所有点位组是否都已收集
        // 重要：不检查UnlockedLevels，只要点位组中任意一个位置被收集即可
        foreach (var pointGroup in levelData.mapData.point)
        {
            bool isCollected = false;
            foreach (var pos in pointGroup.pos)
            {
                string groupId = GetPointGroupId(pointGroup.level, pos.ToArray());
                if (state.CollectedPointGroups.Contains(groupId))
                {
                    isCollected = true;
                    break;
                }
            }

            if (!isCollected)
            {
                return false;   
            }
        }
        return true;    // 所有点位组都已收集
    }

    // 完全克隆
    public static TKGameState CloneState(TKGameState state)
    {
        TKGameState clone = new TKGameState();
        
        clone.PlayerPosition = (int[])state.PlayerPosition.Clone(); 
        clone.PlayerDirection = state.PlayerDirection;  
        clone.CollectedPointGroups = new HashSet<string>(state.CollectedPointGroups);
        clone.UnlockedLevels = new HashSet<PointLevel>(state.UnlockedLevels);
        clone.CollapsedTiles = new HashSet<string>(state.CollapsedTiles);
        clone.SpikeStates = new Dictionary<string,bool>(state.SpikeStates);
        clone.RotationStates = new Dictionary<string, int>(state.RotationStates);
        clone.GameTime = state.GameTime;
        clone.StepCount = state.StepCount;
        clone.Parent = state.Parent;
        clone.LastMove = state.LastMove;

        return clone;
    }
    
    
    // 生成点位组ID
    public static string GetPointGroupId(PointLevel type, int[] position)
    {
        return $"{(int)type}-{position[0]},{position[1]}";
    }

    /// <summary>
    /// 生成状态哈希（用于去重）
    /// </summary>
    public static string HashState(TKGameState state)
    {
        StringBuilder sb = new StringBuilder(); 
        
        // 玩家位置
        sb.Append($"{state.PlayerPosition[0]},{state.PlayerPosition[1]}-");
        
        // 玩家朝向
        string directionStr = state.PlayerDirection switch
        {
            Direction.None => "none",
            Direction.Up => "up",
            Direction.Down => "down",
            Direction.Left => "left",
            Direction.Right => "right",
            _ => "unknown"
        };
        sb.Append($"{directionStr}-");
        
        // 已收集点位组（排序后拼接）  确保相同的点位组集合无论原始顺序如何，最终生成的字符串都完全一致
        List<string> sortedPoints = new List<string>(state.CollectedPointGroups);
        sortedPoints.Sort(); // 对列表按字符串默认规则排序（字典序）
        sb.Append(string.Join(",", sortedPoints)); //用逗号拼接排序后的列表
        sb.Append("-");

        List<string> sortedCollapsed = new List<string>(state.CollapsedTiles);
        sortedCollapsed.Sort();
        sb.Append(string.Join(",", sortedCollapsed));
        sb.Append("-");
        
        // 地刺状态（排序后拼接）
        List<string> spikeKeys = new List<string>(state.SpikeStates.Keys);
        spikeKeys.Sort();
        foreach (string key in spikeKeys)
        {
            sb.Append($"{key}:{state.SpikeStates[key].ToString().ToLower()},");
        }
        sb.Append("-");
            
        // 旋转门状态（排序后拼接）
        List<string> rotationKeys = new List<string>(state.RotationStates.Keys);
        rotationKeys.Sort();
        foreach (string key in rotationKeys)
        {
            sb.Append($"{key}:{state.RotationStates[key]},");
        }
            
        return sb.ToString();
    }

    // 提取路径（从当前状态回溯到初始状态）
    public static List<Direction> ExtractPath(TKGameState state)
    {
        List<Direction> path = new List<Direction>();
        TKGameState current = state;

        while (current.Parent != null && current.LastMove.HasValue)
        {
            path.Insert(0, current.LastMove.Value); 
            current = current.Parent;   
        }
        return path;    
    }
}
