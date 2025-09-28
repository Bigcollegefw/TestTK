using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWindow : BaseUI
{
    public MapNode mapNode;

    public MainData mainData;
    public override void OnStart(IUIData uiData)
    { 
        this.mainData = new MainData();
        base.OnStart(uiData);
    }
}
