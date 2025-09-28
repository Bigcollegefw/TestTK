using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineUtils : MonoBehaviour
{
    private static object syncRoot = new object();  // 不用多线程但是这样可能会更加严谨
    
    private static CoroutineUtils _instance;

    public static CoroutineUtils Instance {
        get {
            lock (syncRoot)
            {
                if (_instance == null) {
                    GameObject go = new GameObject("CoroutineUtils");
                    UnityEngine.Object.DontDestroyOnLoad(go);
                    _instance = go.AddComponent<CoroutineUtils>();
                }
            }
            return _instance;
        }
    }
    
    private Dictionary<string, List<Coroutine>> coroutines = new Dictionary<string, List<Coroutine>>();

    public Coroutine startCoroutine(IEnumerator enumerator, string tag)
    {
        var list = this.coroutines.objectValue(tag);
        if (list == null) {
            list = new List<Coroutine>();
            this.coroutines[tag] = list;
        }
        var c= this.StartCoroutine(enumerator);
        list.Add(c);
        return c;
    }
    public void stopCoroutine(string tag)
    {
        var list = this.coroutines.objectValue(tag);
        if (list == null) {
            return;
        }

        foreach (var c in list) {
            this.StopCoroutine(c);
        }
        list.Clear();
    }
}
