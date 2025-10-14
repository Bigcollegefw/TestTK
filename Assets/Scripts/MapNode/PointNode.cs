using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointNode : MonoBehaviour
{
    public Text pointText;

    private PointLevel pointType;
    
    private MainData mainData => MainData.Instance; 
    
    // 标志id，用来区别是不是同一组的点位
    public int commonId;

    public PointLevel GetPointType()
    {
        return pointType;
    }

    public void SetPointType(PointLevel pointType)
    {
        pointType = pointType;
        this.pointText.text = pointType.toInt().ToString(); //显示的是枚举的数值
    }
    
    public void InitPointNode(PointLevel _type, int _commonId)
    {
        this.SetPointType(_type);
        commonId = _commonId;
    }
    
    public bool isCanDead()
    {
        if (mainData.playerLevel < this.pointType)
        {
            return true;
        }
        return false;
    }
}
