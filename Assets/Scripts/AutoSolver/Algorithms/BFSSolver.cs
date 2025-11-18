using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



// BFS求解器
public class BFSSolver
{
    private LevelData levelData;
    private SolverConfig config;
    private Dictionary<string, TKGameState> cache;
    private PerformanceMertrics metrics;

    private readonly string[] directionNames = { "无", "上", "下", "左", "右" };

    // 地块缓存
    private Dictionary<string, FloorData> floorCache;
    private Dictionary<string, ObstacleData> obstacleCache;

    public BFSSolver(LevelData levelData, SolverConfig config = null)
    {
        this.levelData = levelData;
        // ?? 空合并运算符
        // 如 config 不为 null，则 this.config 赋值为传入的 config；
        // 如 config 为 null（包括未传参数时的默认 null），则赋值为 new SolverConfig()（新创建的默认实例）
        this.config = config ?? new SolverConfig()
        {
            MaxSearchDepth = 100,
            TimeLimit = 5000,
            EnableCaching = true,
            EnableOptimization = true
        };

        this.cache = new Dictionary<string, TKGameState>();
        this.metrics = new PerformanceMertrics();

        // 构建缓存
        BuildCaches();
    }

    private void BuildCaches()
    {
        //(key: "col,row", value: 具体地块)
        floorCache = new Dictionary<string, FloorData>();
        obstacleCache = new Dictionary<string, ObstacleData>();

        if (levelData.mapData.floor != null)
        {
            foreach (var floor in levelData.mapData.floor)
            {
                string key = $"{floor.pos.col},{floor.pos.row}";
                floorCache[key] = floor;
            }
        }

        if (levelData.mapData.obstacle != null)
        {
            foreach (var obstacle in levelData.mapData.obstacle)
            {
                string key = $"{obstacle.pos.col},{obstacle.pos.row}";
                obstacleCache[key] = obstacle;
            }
        }
    }
    
    // 寻找下一步最佳移动并返回完整解决方案
    public IEnumerator FindNextBestMoveWithSolution(TKGameState state, Action<Direction?, Solution> callback)
    {
        yield return BFSSearchWithGreedyLogic(state, solution =>
        { // 这里的solution是在BFSSearchWithGreedyLogic里面传入Action<Solution> callback的参数
            if (solution != null && solution.Path != null && solution.Path.Length > 0)
            {
                callback(solution.Path[0], solution);
            }
            else
            {
                callback(null, null);   
            }
        });
    }
     
    // 基于贪心逻辑的BFS搜索————这里为什么是贪心呢，因为这个都是去选取预估的最优路劲，由无数个最优路径完成的最优解的找法。
    public IEnumerator BFSSearchWithGreedyLogic(TKGameState initialState, Action<Solution> callback)
    {
        // 使用A*算法的优先队列（最小堆）
        MinHeap<StateWithPriority> queue = new MinHeap<StateWithPriority>(1000);
        int sequenceCounter = 0; // 排队的数量

        float initalPriority = CalculateHeuristic(initialState); // 计算出F（n）

        queue.Push(new StateWithPriority 
        {
            State = initialState,
            Priority = initalPriority,
            Sequence = sequenceCounter++
        });

        HashSet<string> visited = new HashSet<string>();
        // A*经典顺序：右下左上
        Direction[] directions = { Direction.Right, Direction.Down, Direction.Left, Direction.Up };

        float startTime = Time.deltaTime; // 返回游戏从启动（或编辑器进入 Play 模式）到现在所经过的“真实时间”

        int statesProcessed = 0;

        while (queue.Count > 0) // 本质是层序遍历。
        {
            if ((Time.deltaTime - startTime) * 1000 > config.TimeLimit)
            {
                Debug.LogWarning("[A*Solver] 搜索超时");
                callback(null);
                yield break;
            }

            StateWithPriority currentItem = queue.Pop();
            TKGameState currentState = currentItem.State;
            string stateHash = GameStateManager.HashState(currentState);

            if (visited.Contains(stateHash))
            {
                continue;
            }

            visited.Add(stateHash);
            metrics.StatesExplored++;
            statesProcessed++;

            if (GameStateManager.IsWinningState(currentState, levelData))
            {
                List<Direction> path = GameStateManager.ExtractPath(currentState);
                
                callback(CreateSolution(currentState, path)); // 传入路径给UI处理了。
                yield break;
            }

            // 检查深度限制
            if (currentState.StepCount >= config.MaxSearchDepth)
            {
                continue;
            }

            // 生成后继状态
            foreach (Direction direction in directions)
            {
                TKGameState nextState = GetNextStateWithGreedyLogic(currentState, direction);
                if (nextState != null)
                {
                    // 计算优先级并入队
                    float priority = CalculateHeuristic(nextState);
                    queue.Push(new StateWithPriority
                    {
                        State = nextState,
                        Priority = priority,
                        Sequence = sequenceCounter++
                    });
                }
            }

            // 每处理100个状态让出一帧。
            if (statesProcessed % 100 == 0)
            {
                yield return null;
            }
        }

        callback(null); // 无解
    }

    // 使用贪心逻辑获取下一个状态
    private TKGameState GetNextStateWithGreedyLogic(TKGameState state, Direction direction)
    {
        try
        {
            var moveResult = SimulateMove(state, direction);

            if (moveResult == null || moveResult.GameEnded) // 如果游戏已经结束.就结束掉
            {
                return null;
            }

            TKGameState newState = GameStateManager.CloneState(state);
            newState.PlayerPosition = moveResult.FinalPosition; // 通过模拟移动得出的最后一个位置
            newState.PlayerDirection = direction; // 这里也是最后一个方向
            newState.StepCount += 1;
            newState.GameTime += 1;
            newState.LastMove = direction; // 上一步移动方向

            // 应用移动结果的状态变化
            newState.CollectedPointGroups = new HashSet<string>(moveResult.NewState.CollectedPointGroups);
            newState.UnlockedLevels = new HashSet<PointLevel>(moveResult.NewState.UnlockedLevels);
            newState.CollapsedTiles = new HashSet<string>(moveResult.NewState.CollapsedTiles);
            newState.SpikeStates = new Dictionary<string, bool>(moveResult.NewState.SpikeStates);
            newState.RotationStates = new Dictionary<string, int>(moveResult.NewState.RotationStates);

            newState.Parent = state;
            return newState;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[BFSSolver] GetNextStateWithGreedyLogic失败: {e.Message}");
            return null;
        }
    }

    // 核心：模拟完整移动
    public MoveResult SimulateMove(TKGameState state, Direction direction, bool enableDebugLog = false)
    {
        int[] startPos = (int[])state.PlayerPosition.Clone();
        int[] currentPos = (int[])state.PlayerPosition.Clone();
        Direction currentDirection = direction;
        bool gameEnded = false;
        List<object> pointsCollected = new List<object>();

        if (enableDebugLog)
        {
            Debug.Log($"[SimulateMove] 开始：位置[{startPos[0]},{startPos[1]}]，方向{direction}");
        }

        TKGameState newState = GameStateManager.CloneState(state);

        HandleDepartureCollapse(startPos, newState); //处理离开当前位置的坍塌

        int steps = 0;
        int maxSteps = 20;

        while (steps < maxSteps && !gameEnded)
        {
            steps++;
            int[] nextPos = GetNextPosition(currentPos, currentDirection);

            if (enableDebugLog)
            {
                Debug.Log(
                    $"[SimulateMove] 第{steps}步：当前[{currentPos[0]},{currentPos[1]}]，下一个[{nextPos[0]},{nextPos[1]}]");
            }

            // 检查边界
            bool isOutOfBounds = nextPos[0] < 0 || nextPos[0] >= levelData.col || nextPos[1] < 0 ||
                                 nextPos[1] >= levelData.row;

            bool isBlocked = IsMovementBlocked(currentPos, currentDirection, newState);

            if (enableDebugLog)
            {
                Debug.Log($"[SimulateMove] 出界={isOutOfBounds}，阻挡={isBlocked}");
            }

            if (isBlocked)
            {
                if (enableDebugLog)
                {
                    Debug.Log($"[SimulateMove] 被阻挡，停在[{currentPos[0]},{currentPos[1]}]");
                }

                break; // 被阻挡，停在当前位置
            }

            if (isOutOfBounds)
            {
                if (enableDebugLog)
                {
                    Debug.Log($"[SimulateMove] 出界，游戏结束");
                }

                gameEnded = true; // 出界失败
                break;
            }

            // 移动到新位置,移动后检查致命地块
            currentPos = nextPos;

            
            if (IsDealyPosition(currentPos, newState))
            {
                gameEnded = true;
                break;
            }
            
            // 检查安全性，主要检查未解锁点位，我感觉这里逻辑可以和上面IsDealyPosition合起来。
            if (!IsSafePosition(newState, currentPos))
            {
                gameEnded = true;
                break;
            }
            FloorData floorTile = GetFloorAt(currentPos);
            // 处理地块效果
            if (floorTile != null) 
            {
                var tileResult = HandleFloorTile(floorTile, currentPos, currentDirection, newState, true);
                if (tileResult.GameEnded)
                {
                    gameEnded = true;
                    break;
                }
                if (tileResult.NewDirection.HasValue)
                {
                    currentDirection = tileResult.NewDirection.Value;  
                }
                
                if (tileResult.TeleportTo != null)
                {
                    currentPos = tileResult.TeleportTo;
                        
                    // 传送后检查点位收集
                    var teleportPoint = CheckPointCollection(currentPos, newState);
                    if (teleportPoint != null)
                    {
                        pointsCollected.Add(teleportPoint);
                    }
                    break; // 传送后停止
                }
                if (tileResult.ShouldStop)
                {
                    break;
                }
            }
            // 检查点位收集
            var pointCollected = CheckPointCollection(currentPos, newState);
            if (pointCollected != null)
            {
                pointsCollected.Add(pointCollected);
                break; // 收集点位后停止
            }
        }
        HandlePathCollapses(startPos, currentPos, newState);
        //关键修复：更新newState的玩家位置和方向
        newState.PlayerPosition = currentPos;
        newState.PlayerDirection = currentDirection;
            
        return new MoveResult
        {
            FinalPosition = currentPos,
            GameEnded = gameEnded,
            PointsCollected = pointsCollected,
            NewState = newState
        };
        
    }

    private void HandlePathCollapses(int[] startPosition, int[] targetPosition, TKGameState state)
    {
        List<int[]> movePath = CalculateMovementPath(startPosition, targetPosition); 
        foreach (var position in movePath)
        {
            int col = position[0];
            int row = position[1];
                
            // 跳过最终停留位置
            if (col == targetPosition[0] && row == targetPosition[1])
            {
                continue;
            }
                
            FloorData floor = GetFloorAt(position);
                
            if (floor != null && floor.type == FloorType.COLLAPSE)
            {
                string collapseKey = $"{col},{row}";
                    
                if (!state.CollapsedTiles.Contains(collapseKey))
                {
                    state.CollapsedTiles.Add(collapseKey);
                }
            }
        }
    }
    
    // 计算移动路劲
    private List<int[]> CalculateMovementPath(int[] startPosition, int[] targetPosition)
    {
        List<int[]> path = new List<int[]>(); // 记录路过的每一个点
        int startCol = startPosition[0];
        int startRow = startPosition[1];
        int targetCol = targetPosition[0];
        int targetRow = targetPosition[1]; 
        
        if (startCol == targetCol && startRow == targetRow)
        {
            return path;
        }
        
        int deltaCol = targetCol > startCol ? 1 : (targetCol < startCol ? -1 : 0);
        int deltaRow = targetRow > startRow ? 1 : (targetRow < startRow ? -1 : 0);
        
        int currentCol = startCol;
        int currentRow = startRow;
        
        while (currentCol != targetCol || currentRow != targetRow)
        {
            currentCol += deltaCol;
            currentRow += deltaRow;
            path.Add(new int[] { currentCol, currentRow });
                
            if (path.Count > 100)
            {
                Debug.LogError("[BFSSolver] 路径计算超出限制");
                break;
            }
        }
        return path;
    }
    
    // 检查点位收集
    private object CheckPointCollection(int[] pos, TKGameState state)
    {
        if (levelData.mapData.point == null) return null;

        foreach (var pointGroup in levelData.mapData.point)
        {
            if (!state.UnlockedLevels.Contains(pointGroup.level)) // 如果点位已经收集，跳过后续流程
            {
                continue;
            }
            bool isInthisGroup = pointGroup.pos.Any(p=> p.col == pos[0] && p.row == pos[1]);

            if (isInthisGroup)
            {
                bool groupAlreadyCollected = pointGroup.pos.Any(p =>
                {
                    //Any(） 方法的参数正是一个返回 bool 的委托（Func<Point，bool>）
                    string pointGroupId = GameStateManager.GetPointGroupId(pointGroup.level, p.ToArray());
                    return state.CollectedPointGroups.Contains(pointGroupId);
                });

                if (!groupAlreadyCollected)
                {
                    string pointGroupId = GameStateManager.GetPointGroupId(pointGroup.level, pos);
                    state.CollectedPointGroups.Add(pointGroupId);
                    
                    PointLevel currentLevel = pointGroup.level;
                    
                    bool allGroupsCollected = AreAllPointGroupsCollected(state, currentLevel);

                    if (allGroupsCollected)
                    {
                        int nextLevelInt = (int)currentLevel + 1;
                        if (nextLevelInt <= 5)
                        {
                            PointLevel nextLevel = (PointLevel)nextLevelInt;
                            if (!state.UnlockedLevels.Contains(nextLevel))
                            {
                                state.UnlockedLevels.Add(nextLevel);
                            }
                        }
                    }
                    return new { type = pointGroup.level, pos = pos };
                }

                return null;
            }
        }
        return null;
    }

    private bool AreAllPointGroupsCollected(TKGameState state, PointLevel level)
    {
        if (levelData.mapData.point == null) return true;
        var pointGroupsAtLevel = levelData.mapData.point.Where(pg => pg.level == level);
        foreach (var pointGroup in pointGroupsAtLevel)
        {
            bool isGroupCollected = pointGroup.pos.Any(pos =>
                state.CollectedPointGroups.Contains(
                    GameStateManager.GetPointGroupId(pointGroup.level, pos.ToArray())));
                
            if (!isGroupCollected)
            {
                return false;
            }
        }
        return true;
    }
    
    // 处理地块效果
    private TileResult HandleFloorTile(FloorData tile, int[] pos, Direction direction,
        TKGameState state, bool skipRotationGate)  
    {
        switch (tile.type)
        {
            case FloorType.TELEPORT:
                return new TileResult
                {
                    TeleportTo = tile.target.ToArray(),
                    ShouldStop = true,
                    GameEnded = false
                };
            case FloorType.DIRECTION:
                Direction newDirection = tile.direction switch
                {
                    0 => Direction.Up,
                    1 => Direction.Right,
                    2 => Direction.Down,
                    3 => Direction.Left,
                    _ => Direction.None
                };
                return new TileResult
                {
                    NewDirection = newDirection,
                    ShouldStop = false,
                    GameEnded = false
                };
            case FloorType.COLLAPSE:
                string collapseKey = $"{pos[0]},{pos[1]}";
                bool isAlreadyCollapsed = state.CollapsedTiles.Contains(collapseKey);
                    
                if (isAlreadyCollapsed)
                {
                    return new TileResult
                    {
                        ShouldStop = true,
                        GameEnded = true
                    };
                }
                else
                {
                    return new TileResult
                    {
                        ShouldStop = false,
                        GameEnded = false
                    };
                }
            case FloorType.ROTATION:
                if (!skipRotationGate)
                {
                    // TODO 处理旋转门机关ProcessRotationGate(state, tile);
                }
                return new TileResult
                {
                    ShouldStop = false,
                    GameEnded = false
                };
            default:
                return new TileResult
                {
                    ShouldStop = false,
                    GameEnded = false
                };
        }
    }

    // 检查位置是否安全
    private bool IsSafePosition(TKGameState state, int[] pos)
    {
        int x = pos[0];
        int y = pos[1];
        
        if (x < 0 || x >= levelData.col || y < 0 || y >= levelData.row)
        {
            return false; // 超出边界就不安全。
        }
        // 检查未解锁点位 
        if (levelData.mapData.point != null)
        {
            foreach (var pointGroup in levelData.mapData.point)
            {
                if (!state.UnlockedLevels.Contains(pointGroup.level))
                {
                    foreach (var pointPos in pointGroup.pos)
                    {
                        if (pointPos.col == x && pointPos.row == y)
                            return false;
                    }
                }
            }
        }
        
        // 检查地块类型
        FloorData floorTile = GetFloorAt(pos);
        if (floorTile != null)
        {
            switch (floorTile.type)
            {
                case FloorType.TRAP:
                case FloorType.NIL:
                    return false;
            }
        }
        return true;
    }
    
    // 检查位置是否致命
    private bool IsDealyPosition(int[] pos, TKGameState state)
    {
        ObstacleData obstacle = GetObstacleAt(pos);
        if (obstacle != null)
        {
            return false; // 障碍物位置不会到达,故而不会致命
        }
        // 若无地块数据（floorTile == null），默认安全（返回 false）若有地块数据，再通过 switch 判断具体类型是否致命
        FloorData floorTile = GetFloorAt(pos);
        if (floorTile == null)  
        {
            return false; // 默认地板是安全的, 实际中应该不会进到这里，应该都会有数据吧。
        }

        switch (floorTile.type)
        {
            case FloorType.FAIL:
                return false; // 边缘地块本身不致命
            case FloorType.TRAP:
                return true;
            case FloorType.NIL:
                return true;
            case FloorType.SPIKES:
                return false; // 求解中视为安全
            case FloorType.COLLAPSE:
                string collapseKey = $"{pos[0]},{pos[1]}";
                if (state.CollapsedTiles.Contains(collapseKey))
                {
                    return true;
                }
                break;
        }
        return false;
    }
    


    // 检查移动是否被阻挡
    private bool IsMovementBlocked(int[] fromPos, Direction direction, TKGameState state)
    {
        // 1.检查起点边缘拦截
        if (IsEdgeBlocking(fromPos, direction))
        {
            return true;
        }
        
        // 注意这里要双向拦截。
        // 2.试图向右移动到下一个格子，但下一个格子的左侧有边缘拦截（阻止从左边进入），此时也会被阻挡。
        int[] nextPos = GetNextPosition(fromPos, direction); // fromPos代表起点。
        Direction oppositeDirection = GetOppositeDirection(direction); // 反向

        if (IsEdgeBlocking(nextPos, oppositeDirection))
        {
            return true;
        }
        
        // 3.检查障碍物
        ObstacleData obstacle = GetObstacleAt(nextPos);
        if (obstacle != null)
        {
            return true;
        }
        
        // TODO 4. 检查旋转门阻挡
        //if (IsBlockedByRotationGate(fromPos, nextPos, direction, state))
        //{
        //    return true;
        //}
            
        return false;
    }

    // 检查边缘拦截,这里的direction代表的是将要移动的位置。
    private bool IsEdgeBlocking(int[] pos, Direction direction)
    {
        FloorData floorTile = GetFloorAt(pos);
        
        if (floorTile == null || floorTile.type != FloorType.EDGE) // 不占地块的边缘地块
        {
            return false;
        }
            
        if (floorTile.obstacle == null || floorTile.obstacle.Length == 0)
        {
            return false;
        }
        
        int gameDirection = direction switch
        {
            Direction.Up => 0,
            Direction.Right => 1,
            Direction.Down => 2,
            Direction.Left => 3,
            _ => -1
        };
            
        return floorTile.obstacle.Contains(gameDirection);
    }
    
    private int[] GetNextPosition(int[] pos, Direction direction)
    {
        int x = pos[0];
        int y = pos[1];
        
        switch (direction)
        {
            case Direction.Up: return new int[] { x, y - 1 };
            case Direction.Down: return new int[] { x, y + 1 };
            case Direction.Left: return new int[] { x - 1, y };
            case Direction.Right: return new int[] { x + 1, y };
            default: return pos;
        }
    }
    
    private void HandleDepartureCollapse(int[] position, TKGameState state)
    {
        FloorData floor = GetFloorAt(position);

        if (floor != null && floor.type == FloorType.COLLAPSE)
        {
            string collapseKey = $"{position[0]},{position[1]}";
            if (!state.CollapsedTiles.Contains(collapseKey))
            {
                state.CollapsedTiles.Add(collapseKey);  // 把坍塌的地块加到哈希集合中
            }
        }
    }
    private Direction GetOppositeDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return Direction.Down;
            case Direction.Right: return Direction.Left;
            case Direction.Down: return Direction.Up;
            case Direction.Left: return Direction.Right;
            default: return direction;
        }
    }
    
    // 获取地块
    private FloorData GetFloorAt(int[] pos)
    {
        string key = $"{pos[0]},{pos[1]}";
        return floorCache.ContainsKey(key) ? floorCache[key] : null;
    }
    
    // 获取障碍物
    private ObstacleData GetObstacleAt(int[] pos)
    {
        string key = $"{pos[0]},{pos[1]}";
        return obstacleCache.ContainsKey(key) ? obstacleCache[key] : null;
    }
    
    /// 计算A*算法的启发式值 f(n) = g(n) + h(n)
    private float CalculateHeuristic(TKGameState state)
    {
        float g = state.StepCount; // g(n): 已走步数
        
        float h = CalculateRemainingCost(state);// h(n): 启发式估计
        
        return g + h;// f(n) = g(n) + h(n)
    }

    // 计算启发式估计的剩余代价，使用“未收集点位数 * 权重 + 到最近点位的曼哈顿距离”
    // 这是一个可采纳启发式，保证找到最优解
    private float CalculateRemainingCost(TKGameState state)
    {
        if (levelData.mapData.point == null || levelData.mapData.point.Length == 0)
        {
            return 0;
        }
        
        int uncollectedPoints = CountUncollectedPoints(state);

        // 如果所有点位都已收集，代价为0
        if (uncollectedPoints == 0) 
            return 0;
        
        float minDistance = GetMinDistanceToNearestUncollectedPoint(state);
        
        // 启发式估计：每个点位估计需要5步 + 到最近点位的距离
        // 权重5是经验值
        return uncollectedPoints * 2.0f + minDistance;
    }

    // 计算未收集的点位组数量
    private int CountUncollectedPoints(TKGameState state)
    {
        if (levelData.mapData.point == null) return 0;
        int uncollected = 0;

        // 这里计算点位组能对完全是因为每一个组都是一个，每个同级点位都被拆开成了一个组
        foreach (var pointGroup in levelData.mapData.point) // 里面的每个点位组
        {
            // pointGroup对应一个{“type”:1,"pos":[  [4,2],[4,8] ] }
            // 检查这个点位组是否已收集（任意一个位置被收集即可）,
            bool isCollected = false;
            foreach (var pos in pointGroup.pos)
            {
                string groupId = GameStateManager.GetPointGroupId(pointGroup.level, pos.ToArray());
                if (state.CollectedPointGroups.Contains(groupId))
                {
                    isCollected = true;
                    break;
                }
            }

            if (!isCollected)
            {
                uncollected++;  
            }
        }  
        return uncollected; 
    }

    // 获取到最近未收集点曼哈顿距离
    private float GetMinDistanceToNearestUncollectedPoint(TKGameState state)
    { 
        float minDistance = float.MaxValue;
        int playerCol = state.PlayerPosition[0];
        int playerRow = state.PlayerPosition[1];

        foreach (var pointGroup in levelData.mapData.point)
        {
            // 检查这个点位组是否已收集
            bool isCollected = false;
            foreach (var pos in pointGroup.pos)
            {
                string groupId = GameStateManager.GetPointGroupId(pointGroup.level, pos.ToArray());
                if (state.CollectedPointGroups.Contains(groupId))
                {
                    isCollected = true;
                    break;
                }
            }

            // 如果未收集，计算到该点位组所有位置的最小距离
            if (!isCollected)
            {
                foreach (var pos in pointGroup.pos)
                {
                    float distance = Math.Abs(playerCol - pos.col) + Math.Abs(playerRow - pos.row);
                    minDistance = Math.Min(minDistance, distance);  
                }
            }
        }
        return minDistance == float.MaxValue ? 0 : minDistance;       
    }


    private Solution CreateSolution(TKGameState finalState, List<Direction> path)
    {
        SolutionQuality quality = EvaluateSolutionQuality(finalState, path);

        return new Solution
        {
            Path = path.ToArray(),
            TotalSteps = path.Count,
            EstimatedTime = finalState.GameTime,
            IsOptimal = true, // 真的是最优解吗
            Quality = quality
        };
    }

    private SolutionQuality EvaluateSolutionQuality(TKGameState finalState, List<Direction> path)
    {
        // 最优性：与理论最小步数的接近程度
        int minSteps = GetMinimumSteps();
        float optimality = Math.Max(0, 1-(path.Count / minSteps) / 50.0f);
        
        // 优雅度：方向变化的平滑度
        float elegance = CalculatePathElegance(path);

        // 稳健性：BFS解法通常比较稳定
        float robustness = 0.8f;

        // 学习价值：路径的复杂度和教学意义
        float learningValue = CalculateLearningValue(path);

        float totalScore = optimality * 0.4f + elegance * 0.2f + robustness * 0.2f + learningValue * 0.2f;
        
        return new SolutionQuality
        {
            Optimality = optimality,
            Elegance = elegance,
            Robustness = robustness,
            LearningValue = learningValue,
            TotalScore = totalScore
        };
    }

    // 获取理论最小步数
    private int GetMinimumSteps()
    {
        int totalDistance = 0;
        int[] startPos = levelData.startPos.ToArray();

        if (levelData.mapData.point != null)
        {
            // 遍历完1级遍历2级
            foreach (var pointGroup in levelData.mapData.point)
            {
                int minDistance = int.MaxValue;
                // 遍历每一级的所有收集物，找出最短的路径
                foreach (var pos in pointGroup.pos)
                {
                    int distance = Math.Abs(pos.col - startPos[0] + Math.Abs(pos.row - startPos[1]));
                    minDistance = Math.Min(minDistance, distance);
                }

                if (minDistance != int.MaxValue)
                {
                    totalDistance += minDistance;
                }
            }
        }
        return totalDistance;
    }
    
    // 计算路径优雅度
    private float CalculatePathElegance(List<Direction> path)
    {
        if (path.Count == 0) return 1;

        int directionChanges = 0; // 优雅度

        for (int i = 1; i < path.Count; i++)
        {
            if (path[i] != path[i - 1])
            {
                directionChanges++;
            }
        }
        
        // 方向变化越多月不平滑
        float eleganceRatio = 1 - (float)directionChanges / path.Count;
        return Math.Max(0, eleganceRatio);
    }
    
    // 计算学习价值
    private float CalculateLearningValue(List<Direction> path)
    {
        // 学习价值基于路径的复杂度 和 教学意义
        float complexity = path.Count / 100.0f; // 标准化复杂度 较长的路径能提供更多实践机会（但过长可能导致冗余，因此/100标准化，避免极端值影响）
        float variation = CalucatePathVariation(path); // 而方向分布均匀的路径能覆盖更多操作类型，帮助学习者全面理解规则
        return Math.Min(1,complexity * 0.6f +  variation * 0.4f);
    }

    // 计算路劲变化度
    private float CalucatePathVariation(List<Direction> path)
    {
        if (path.Count == 0) return 0;
        
        Dictionary<Direction,int> directionsCounts = new Dictionary<Direction, int>();

        foreach (Direction dir in path) // 统计每一个方向出现的次数
        {
            if (!directionsCounts.ContainsKey(dir))
            {
                directionsCounts[dir] = 0;
            }
            directionsCounts[dir]++;    
            // 若路径是 [Right, Right, Down, Left]，则统计结果为：Right:2，Down:1，Left:1。
        }

        // 方向使用的均匀性
        float entropy = 0;
        foreach (int count in directionsCounts.Values)
        {
            //路径总长度为 4，则：(p{Right} = 2/4 = 0.5)，(p{Down} = 1/4 = 0.25)，(p{Left} = 1/4 = 0.25)。
            float p = (float)count / path.Count;
            if (p > 0)
            {
                // 因为公式这里是-的所以是-=
                entropy -= p * Mathf.Log(p, 2);
            }
        }
        //（如 4 个方向），熵的最大值出现在所有方向均匀分布时（此时(H = 2)），最小值为 0
        return Math.Min(1, entropy / 2);    // 标准化到0-1
    }
    
    // 解法质量评估
    public class SolutionQuality
    {
        public float Optimality; // 最优性

        public float Elegance;  // 优雅度
        
        public float Robustness;    // 稳健性
    
        public float LearningValue;     // 学习价值
    
        public float TotalScore;    // 总分
    }

    public class MoveResult
    {
        public int[] FinalPosition;

        public bool GameEnded;

        public List<object> PointsCollected;
        
        public TKGameState NewState;
    }

    public class TileResult
    {
        public bool GameEnded;  // 游戏结束
        public Direction? NewDirection;  // 被转的新方向
        public int[] TeleportTo;    // 传送到哪
        public bool ShouldStop;     // 应该停下
    }
    
}
