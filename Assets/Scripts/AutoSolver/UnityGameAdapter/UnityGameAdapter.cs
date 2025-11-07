using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// Unity游戏适配器
public class UnityGameAdapter
{
    private BFSSolver solver;
    
    private LevelData levelData;

    private SolverConfig config;
    
    // 解决方案缓存
    private Solution cachedSolution = null;
    private int currentStepIndex = 0; // 就是用来 跟踪“缓存路径中，下一步该用哪一步” 的
    private TKGameState cachedInitialState = null; // 缓存初始状态
    private BFSSolver cachedSolver = null; // 保存求解器，在SimulateSingleMoveForCache方法中用于准确的移动模拟
    
    public UnityGameAdapter(LevelData levelData, SolverConfig config = null)
    {
        this.levelData = levelData;
        this.config = config ?? new SolverConfig
        {
            MaxSearchDepth = 200,
            TimeLimit = 5000,
            EnableCaching = true,
            EnableOptimization = true
        };
            
        this.solver = new BFSSolver(levelData, this.config);
    }

    public MoveHint TryGetCachedHint(MainData mainData)
    {
        try
        {
            TKGameState currentState = GameStateAdapter.ConvertGameStateToTK(mainData,levelData);
            string currentStateHash = GameStateManager.HashState(currentState);
            if (IsCacheValid(currentState, currentStateHash))
            {
                // 使用缓存！超快响应！
                Direction nextMove = cachedSolution.Path[currentStepIndex];
                int remainingSteps = cachedSolution.Path.Length - currentStepIndex - 1;
                
                Debug.Log($"⚡ [缓存命中] 使用缓存方案，当前第{currentStepIndex + 1}步，剩余{remainingSteps}步");
                
                // 更新索引
                currentStepIndex++;
                
                return new MoveHint
                {
                    Direction = nextMove,
                    Reason = $"缓存方案第{currentStepIndex}步，剩余{remainingSteps}步",
                    Confidence = 0.95f
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TryGetCachedHint] 异常: {ex.Message}");
            return null;
        }
    }

    private bool IsCacheValid(TKGameState currentState, string currentStateHash)
    {
        // 缓存不存在
        if (cachedSolution == null || cachedSolution.Path == null || cachedInitialState == null)
        {
            return false;
        }
        
        // currentStepIndex 是缓存解法路径的“读取指针”，表示“下次应该把路径中的哪一步作为提示返回给游戏界面”
        // 只有当缓存中还有未执行的步骤时，才考虑使用缓存。
        if (currentStepIndex >= cachedSolution.Path.Length)
        {
            return false;
        }
        
        // 检查3：状态匹配
        // 从初始状态模拟执行前currentStepIndex步，看是否与当前状态一致
        string expectedHash = SimulateStepsForCache(cachedInitialState, currentStepIndex);
        if (currentStateHash != expectedHash)
        {
            return false;
        }
            
        return true;
    }

    private string SimulateStepsForCache(TKGameState initialState, int steps)
    {
        if (steps == 0)
        {
            return GameStateManager.HashState(initialState);
        }
        // 克隆初始状态
        TKGameState state = GameStateManager.CloneState(initialState);
            
        // 模拟执行每一步
        for (int i = 0; i < steps && i < cachedSolution.Path.Length; i++)
        {
            Direction move = cachedSolution.Path[i];
                
            // 简化版移动模拟：只更新关键状态
            state = SimulateSingleMoveForCache(state, move);
                
            if (state == null)
            {
                // 移动失败
                return "";
            }
        }
            
        return GameStateManager.HashState(state);
    }

    // 模拟单步移动（使用BFSSolver的完整模拟逻辑）
    private TKGameState SimulateSingleMoveForCache(TKGameState state, Direction direction)
    {
        if (cachedSolver == null)
        {
            return null;
        }
        
        var moveResult = cachedSolver.SimulateMove(state, direction, enableDebugLog: false);
        if (moveResult == null || moveResult.NewState == null)
        {
            return null;
        }
            
        return moveResult.NewState;
    }
    
    
    public IEnumerator CalculateHintAsync(MainData mainData, Action<MoveHint> callback)
    {
        // 转换游戏状态
        TKGameState currentState = GameStateAdapter.ConvertGameStateToTK(mainData, levelData);
        
        
        // 使用快速求解器（A*算法）
        BFSSolver quickSolver = new BFSSolver(levelData, new SolverConfig
        {
            MaxSearchDepth = 50,
            TimeLimit = 60000, // 60秒，复杂关卡需要更多时间
            EnableCaching = config.EnableCaching,
            EnableOptimization = config.EnableOptimization
        });
        
        MoveHint hint = null;
        
        // 使用FindNextBestMoveWithSolution获取下一步和完整解决方案（A*算法）
        yield return quickSolver.FindNextBestMoveWithSolution(currentState, (bestMove, solution) =>
        {
            if (bestMove.HasValue && solution != null && solution.Path != null && solution.Path.Length > 0)
            {
                cachedSolution = solution;
                cachedInitialState = GameStateManager.CloneState(currentState);
                cachedSolver = quickSolver; // 保存求解器引用，用于模拟移动
                currentStepIndex = 1; // 即将返回第0步，所以下次从第1步开始
                
                Debug.Log($"🏆 找到 {solution.Path.Length} 步解决方案");
                
                // 返回第一步
                float confidence = CalculateConfidence(currentState);
                string reason = GenerateHintReason(bestMove.Value, currentState);
                    
                hint = new MoveHint
                {
                    Direction = bestMove.Value,
                    Confidence = confidence,
                    Reason = reason
                };
            }
            else
            {
                // 无解，清空缓存
                ClearSolutionCache();
            }
        });
        callback(hint);
    }
    
    // 计算提示置信度
    private float CalculateConfidence(TKGameState state)
    {
        float baseConfidence = 0.8f;
        // 根据已收集点位  组 数量调整
        int collectedPoints = state.CollectedPointGroups.Count;
        int totalPoints = 0;
        if (levelData.mapData.point != null)
        {
            foreach (var pg in levelData.mapData.point)
            {
                totalPoints += pg.pos.Length; // 计算总点位的数量
            }
        }
        
        if (totalPoints > 0)
        {
            float progress = (float)collectedPoints / totalPoints;
            baseConfidence += progress * 0.15f; // 收集的点位组越多，可信度越高
        }
        
        // 根据步数调整
        if (state.StepCount > 50)
        {
            baseConfidence -= 0.1f; // 步数越多，置信度越低
        }
            
        return Mathf.Clamp01(baseConfidence);
    }
    
    // 生成提示原因
    private string GenerateHintReason(Direction direction, TKGameState state)
    {
        // Direction: None=0, Up=1, Down=2, Left=3, Right=4
        string[] directionNames = { "无", "上", "下", "左", "右" };
        string dirName = directionNames[(int)direction];
            
        // 检查是否有未收集的点位
        int remainingPoints = GetRemainingPointsCount(state);
            
        if (remainingPoints > 0)
        {
            return $"向{dirName}移动可以接近目标点位";
        }
        else
        {
            return $"向{dirName}移动是最优路径";
        }
    }
    
    // 获取剩余点位数量
    private int GetRemainingPointsCount(TKGameState state)
    {
        int remaining = 0;
        if (levelData.mapData.point != null)
        {
            foreach (var pointGroup in levelData.mapData.point)
            {
                if (state.UnlockedLevels.Contains(pointGroup.level))
                {
                    // 检查这个点位组是否已收集
                    bool isCollected = pointGroup.pos.Any(pos =>
                        state.CollectedPointGroups.Contains(
                            GameStateManager.GetPointGroupId(pointGroup.level, pos.ToArray())));
                        
                    if (!isCollected)
                    {
                        remaining++;
                    }
                }
            }
        }
        return remaining;
    }
    
    // 清空解决方案缓存
    public void ClearSolutionCache()
    {
        cachedSolution = null;
        currentStepIndex = 0;
        cachedInitialState = null;
        cachedSolver = null;
        Debug.Log($"🗑️ [缓存] 已清空");
    }
    
}
