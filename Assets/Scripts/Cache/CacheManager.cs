using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CacheManager : SingletonData<CacheManager>
{    
    private GameObject cacheObject;

    private Dictionary<string, Stack<GameObject>> gameobjectStack;
    
    protected override void OnInit()
    {
        this.gameobjectStack = new Dictionary<string, Stack<GameObject>>();
        this.cacheObject = new GameObject("cacheObject");
        this.cacheObject.AddComponent<Canvas>();
        GameObject.DontDestroyOnLoad(this.cacheObject);
        this.cacheObject.SetActive(false);
    }
    
    public GameObject popGameObject(string key, GameObject prafab, Transform parent)
    {
        var s = this.gameobjectStack.objectValue(key);
        if (s == null)
        {
            s = new Stack<GameObject>();
            this.gameobjectStack[key] = s;
        }

        if (s.Count == 0)
        {
            return GameObject.Instantiate(prafab, parent);
        }

        var o = s.Pop();
        o.transform.SetParent(parent);
        o.transform.localScale = Vector3.one;
        o.transform.localPosition = Vector3.zero;
        o.transform.localEulerAngles = Vector3.zero;
        return o;
    }
}
