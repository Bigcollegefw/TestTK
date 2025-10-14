using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainNode
{
    public static MainNode Instance;
    
    public GameWindow gameWindow;
    
    public MainData mainData { get; private set; }
    public MainNode()
    {
        MainNode.Instance = this;
        this.mainData = new MainData(this);
    }


    public void ShowGameWindow()
    {
        gameWindow = UIManager.Instance.OpenUI<GameWindow>();
    }
}
