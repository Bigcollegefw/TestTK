using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleNode : MonoBehaviour
{
    [SerializeField] private GameObject bgSprite;
    
    private ObstacleType obstacleType = 0;
    
    public void InitObstacleNode(ObstacleType type)
    {
        this.obstacleType = type;
        this.bgSprite.SetActive(true);
        this.bgSprite.GetComponent<UnityEngine.UI.Image>().color = Color.black;
    }
}
