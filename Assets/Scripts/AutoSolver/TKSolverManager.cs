using System;
using System.Collections;
using UnityEngine;

public class TKSolverManager : MonoBehaviour
{
    private static TKSolverManager _instance;
    public static TKSolverManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("TKSolverManager");
                _instance = go.AddComponent<TKSolverManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    private UnityGameAdapter adapter;
    private LevelData currentLevelData;
    private Coroutine currentHintCoroutine;

    public void Initialize(LevelData levelData)
    {
        currentLevelData = levelData;
        // 清空旧的缓存
        if (adapter != null)
        {
            adapter.ClearSolutionCache();
        }
        
        SolverConfig config = new SolverConfig
        {
            MaxSearchDepth = 50,
            TimeLimit = 1000,
            EnableCaching = true,
            EnableOptimization = true
        };
        
        adapter = new UnityGameAdapter(levelData, config);
        Debug.Log("[TKSolverManager] 求解器初始化成功");
    }

    // 提供实时提示
    public void ProvideRealTimeHint(MainData mainData, Action<MoveHint> callback)
    {
        if (adapter == null)
        {
            Debug.LogError("[TKSolverManager] 求解器未初始化");
            callback(null);
            return;
        }
        
        // 先尝试从缓存获取（同步，不需要Coroutine）
        MoveHint cachedHint = adapter.TryGetCachedHint(mainData);
            
        if (cachedHint != null)
        {
            callback(cachedHint);
            return;  // 缓存命中，直接返回
        }

        // 缓存失效，取消之前的异步计算
        if (currentHintCoroutine != null)
        {
            StopCoroutine(currentHintCoroutine);
        }
        
        // 开始新的异步计算
        currentHintCoroutine = StartCoroutine(adapter.CalculateHintAsync(mainData, hint =>
        {
            if (hint != null)
            {
                callback(hint);
                currentHintCoroutine = null;
            }
        }));
        
    }
    
}
