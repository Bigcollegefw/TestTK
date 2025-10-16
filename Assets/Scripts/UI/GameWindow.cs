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
    
    public override void start(IUIData uiData)
    { 
        this.mainData.InitGameData(); // 这个必须放在base.start前面，
                                      // 因为在base.start里面会初始化地图，
                                      // 数据都是直接从DatabaseManager取的
        base.start(uiData);
        RestartButton.onClick.AddListener(restartBtnClick);
        ContinueBtn.onClick.AddListener(continueBtnClick);
    }

    protected override void stop() //游戏界面被关闭时候触发
    {
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
    }
}
