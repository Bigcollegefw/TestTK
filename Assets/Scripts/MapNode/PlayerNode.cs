using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNode : MonoBehaviour
{
    public RectTransform rectTransform; // RectTransform组件
    private MainData mainData => MainData.Instance;
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// 开始移动
    /// </summary>
    public void StartMovement()
    {
        mainData.isMoving = true;
    }
}
