/*泛型单例类，UI管理器、资源管理器继承自该单例*/
/*因为单例Instance是静态的，所以只有一个，避免出现多个UI管理器进行操作*/
/*Instance的作用：初始化Instance后，可在其他脚本中直接通过声明Instance变量所在类的名称来访问Instance对象，
   而不需要实例化该类。本案中UIManager继承了Singleton的Instance，因此可直接使用UIManager.Instance来操作
   使用UIManager中的公共属性或方法。*/
using System;
using UnityEngine;

namespace TKFramework
{
    public abstract class Singleton<T> where T : class, new()
    {

        //定义静态单例
        protected static T _Instance = null;
        public static T Instance
        {
            get
            {
                if (null == _Instance)
                {
                    _Instance = new T();
                }
                return _Instance;
            }
        }

        //初始化单例
        protected Singleton()
        {
            //如果实例不为空，抛出异常
            if (null != _Instance)
                throw new SingletonException("This " + (typeof(T)).ToString() + " Singleton Instance is not null !!!");

            //初始化单例
            Init();
        }

        //初始化单例的内容
        public virtual void Init() { }
    }
}
