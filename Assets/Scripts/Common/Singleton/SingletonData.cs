using System;

public abstract class SingletonData<T> where T : new()
{
    protected abstract void OnInit();
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
            }
            return _instance;
        }
    }

    protected SingletonData()
    {
        UnityEngine.Debug.Log("init " + typeof(T));
        OnInit();
    }
}
