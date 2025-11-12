

using System;
using System.Collections.Generic;

// 游戏过程中的游戏状态。
public class TKGameState
{
    public int[] PlayerPosition;

    public Direction PlayerDirection;//玩家朝向
    
    public HashSet<string> CollectedPointGroups; // 点位组，好几个为1个点位组

    public HashSet<PointLevel> UnlockedLevels;// 已解锁的等级

    public HashSet<string> CollapsedTiles;// 已坍塌的地块 (格式: "col,row")
    
    public Dictionary<string, bool> SpikeStates;// 地刺状态 (key: "col,row", value: 是否显示)
    
    public Dictionary<string, int> RotationStates;// 旋转门状态 (key: pairId, value: rotation)

    public float GameTime;// 游戏时间（秒）

    public int StepCount;//步数计数
    
    public TKGameState Parent;// 父状态（用于路径回溯）
    
    // 上一步移动方向,可空类型用于表示 “值可能不存在” 的场景。如 LastMove 为 null 时，表示 “尚未有任何移动”
    public Direction? LastMove;

    public TKGameState()
    {
        CollectedPointGroups = new HashSet<string>();
        UnlockedLevels = new HashSet<PointLevel>();
        CollapsedTiles = new HashSet<string>(); 
        SpikeStates = new Dictionary<string, bool>();
        RotationStates = new Dictionary<string, int>();
    }
}

// 完整解决方案
[Serializable]
public class Solution
{
    public Direction[] Path;

    public int TotalSteps;
    
    public float EstimatedTime; // 预计时间
    public TKGameState FinalState; // 最终状态
    public object Quality;  // 解法质量评估
    public int StatesExplored;  // 探索的状态数
    public float ComputationTime;   // 计算耗时（毫秒）
    public bool IsOptimal; // 是否为最优解
}

// 移动提示
public class MoveHint
{
    public Direction Direction;           // 建议的移动方向
    public float Confidence;              // 置信度 (0-1)
    public string Reason;                 // 提示原因
    public int[] TargetPosition;          // 目标位置
    public bool IsPartOfOptimalPath;      // 是否为最优路径的一部分
}

// 求解器配置
public class SolverConfig
{
    public int MaxSearchDepth = 200; //最大搜索深度
    public int TimeLimit = 5000; //时间限制（毫秒）
    public bool EnableCaching = true;// 启用缓存
    public bool EnableOptimization = true;// 启用优化
    public int MaxStatesPerFrame = 100;// 每帧最大处理状态数（协程用）
}

// 表现的度量
public class PerformanceMertrics
{
    public int StatesExplored;  //探索的状态数
    public int StatesInQueue;   //队列中的状态数
    public int CacheHits;   // 缓存命中数
    public float TimeMs;    // 计算时间
    public float MemoryMB;  //  内存使用（MB）
    public float SolutionQuality; //解法质量分数
}

