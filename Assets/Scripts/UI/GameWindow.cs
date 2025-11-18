using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameWindow : BaseUI
{
    public MapNode mapNode;
    public TouchNode touchNode;
    
    public Button RestartButton;
    public Button ContinueBtn;
    public Button AutoPlayBtn;
    
    public MainData mainData => MainData.Instance;
    
    public override void start(IUIData uiData)
    { 
        this.mainData.InitGameData(); // 这个必须放在base.start前面，
                                      // 因为在base.start里面会初始化地图，
                                      // 数据都是直接从DatabaseManager取的
        base.start(uiData);
        
        LevelData levelData = DataBaseManager.Instance.curLevelConfig;
        TKSolverManager.Instance.Initialize(levelData);
        
        RestartButton.onClick.AddListener(restartBtnClick);
        ContinueBtn.onClick.AddListener(continueBtnClick);
        AutoPlayBtn.onClick.AddListener(TipBtnClick);
    }

    public void TipBtnClick()
    {
        // 调用求解器获取提示
        TKSolverManager.Instance.ProvideRealTimeHint(mainData, hint =>
        {
            if (hint != null)
            {
                ShowHintVisual(hint); ;
            }
            else
            {
                Debug.LogWarning("[提示灯] 当前状态无解");
            }
        });
    }

    private void ShowHintVisual(MoveHint hint)
    {
        Debug.LogWarning($"[提示灯] 建议移动方向: {hint.Direction}, 置信度: {hint.Confidence}");
        
        string directionText = GetDirectionText(hint.Direction);
        Debug.Log($"💡 提示：向{directionText}移动");
    }
    
    /// <summary>
    /// 获取方向文本
    /// </summary>
    private string GetDirectionText(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return "上";
            case Direction.Down: return "下";
            case Direction.Left: return "左";
            case Direction.Right: return "右";
            default: return "未知";
        }
    }
    protected override void stop() //游戏界面被关闭时候触发
    {
        RestartButton.onClick.RemoveAllListeners();
        ContinueBtn.onClick.RemoveAllListeners();
        AutoPlayBtn.onClick.RemoveAllListeners();
        this.mainData.stopGameData();
        base.stop();
    }
    
    private void restartBtnClick()
    {
        ReStartGame();
    }
    private void continueBtnClick()
    {
        ReStartGame(false);
    }
    
    public void ReStartGame(bool isRestart = true)
    {
        
        if (!isRestart)
        {
            if (dataIns.curLevel < dataIns.levelIdLimit)
            {
                dataIns.curLevel += 1;
            }
        }
        this.mainData.InitGameData();
        this.mapNode.InitMap();
        this.touchNode.InitTouch();

        // 初始化求解器 , 切记每回都需要重新初始化求解器，并且更新这个关卡数据啊。
        LevelData levelData = DataBaseManager.Instance.curLevelConfig;
        TKSolverManager.Instance.Initialize(levelData);  
    }
}
