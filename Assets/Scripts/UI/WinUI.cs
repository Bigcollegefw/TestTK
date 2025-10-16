using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinUI : BaseUI
{
    public Button ContinueBtn;
    
    public override void start(IUIData uiData)
    { 
        ContinueBtn.onClick.AddListener(continueBtnClick);
    }
    
    private void continueBtnClick()
    {
        MainNode.Instance.gameWindow.ReStartGame(false);
        this.closeUI();
    }
}