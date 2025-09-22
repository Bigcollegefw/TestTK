//1.接收方在 信息中心 注册信息类型和对应的事件；
//2.发送方发送同类型名的信息给 信息中心；
//3.信息中心找到对应的接收方事件，并运行它。
//详细:
//1.信息接收方在OnAwake里增加监听，将监听信息注册到信息中心的字典集内，参数为 需要在消息中心注册的信息类型  和 信息处理方法
//				MessageCenter.Instance.AddListener(MessageType.Net_MessageTestOne, UpdateGold);
//（需配套在OnRelease里移除监听）
//				MessageCenter.Instance.RemoveListener(MessageType.Net_MessageTestOne, UpdateGold);

//2.信息发送方 创造信息内容，并通过指定正确的信息类型MessageType来确定要触发的事件
//例：Message message = new Message(MessageType.Net_MessageTestOne.ToString(), this);
//3.信息发送方 调用信息中心的发送信息来发送
//例：MessageCenter.Instance.SendMessage(message);

//4.信息中心获得信息发送方的MessageType和信息体，在字典集中找到对应的事件，并以信息体为参数，运行信息接收方的信息处理方法

using System;
using System.Collections;
using System.Collections.Generic;

namespace TKFramework
{
    //消息类
    //实现了IEnumerable接口，此类为可枚举类，被枚举的类型为KeyValuePair（键值对）
    public class Message : IEnumerable<KeyValuePair<string, object>>
    {
        //数据字典集
        private Dictionary<string, object> dicDatas = null;
        //名字
        public string Name { get; private set; }
        //发件体？
        public object Sender { get; private set; }
        //内容
        public object Content { get; set; }

        //set本处实现了“message[key] = value”，将 键和值 快捷写入 dicDatas数据字典集；
        //get本处实现了“data = message[key]”，用 键 快捷读取 dicDatas数据字典集 中对应的值
        #region 使用消息类（this）的索引器直接读写dicDatas数据字典集
        //索引器：索引器对象为类本身（this），和属性类似使用set/get，但索引器可以传入参数
        public object this[string key]
        {
            //读取时，使用key在 数据字典集 中检索和返回对象，找不到则为空
            get
            {
                if (null == dicDatas || !dicDatas.ContainsKey(key))
                    return null;
                return dicDatas[key];
            }
            //写入时，覆盖或新增对应key的条目到 数据字典集
            set
            {
                if (null == dicDatas)
                    dicDatas = new Dictionary<string, object>();
                if (dicDatas.ContainsKey(key))
                    dicDatas[key] = value;
                else
                    dicDatas.Add(key, value);
            }
        }

        #endregion

        #region 实现本类的GetEnumerator方法，返回dicDatas中所有键值对组成的枚举器
        //IEnumerator用于返回枚举器，这里是枚举器的迭代器模式
        //IEnumerator返回枚举器，GetEnumerator方法默认用于返回对象的枚举器，KeyValuePair为可枚举类型
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            if (null == dicDatas)
                yield break;
            //遍历并逐个返回dicDatas数据字典集中的所有键值对
            foreach (KeyValuePair<string, object> kvp in dicDatas)
            {
                yield return kvp;
            }
        }
        #endregion

        #region 实现IEnumerable.GetEnumerator方法，返回dicDatas的枚举器
        //返回dicDatas数据字典集自身的枚举器
        IEnumerator IEnumerable.GetEnumerator()
        {
            return dicDatas.GetEnumerator();
        }

        #endregion

        #region 实现消息的构造函数：Message(多个参数)，将需传输的消息写入该消息类的dicDatas
        //实例化该类时自动生成（重载1 by 名字&发件体）
        public Message(string name, object sender)
        {
            Name = name;
            Sender = sender;
            Content = null;
        }

        //实例化该类时自动生成（重载2 by 名字&发件体&内容）	
        public Message(string name, object sender, object content)
        {
            Name = name;
            Sender = sender;
            Content = content;
        }

        //实例化该类时自动生成（重载3 by 名字&发件体&内容&可变数量（params）的object集合<将写入数据字典集>）
        public Message(string name, object sender, object content, params object[] _dicParams)
        {
            Name = name;
            Sender = sender;
            Content = content;
            //如果object集合是由<string, object>键值对组成的同类型集合时
            if (_dicParams.GetType() == typeof(Dictionary<string, object>))
            {
                //将每个object集合通过索引器写入dicDatas数据字典集
                foreach (object _dicParam in _dicParams)
                {
                    foreach (KeyValuePair<string, object> kvp in _dicParam as Dictionary<string, object>)
                    {
                        this[kvp.Key] = kvp.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XHFrameWork.Message"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public Message(Message message)
        {
            Name = message.Name;
            Sender = message.Sender;
            Content = message.Content;
            foreach (KeyValuePair<string, object> kvp in message.dicDatas)
            {
                this[kvp.Key] = kvp.Value;
            }
        }

        #endregion

        #region 增加和减少Message（的dicDatas字典集）的消息条目

        //增加消息到Message（的dicDatas字典集） by string键&object值
        public void Add(string key, object value)
        {
            this[key] = value;
        }

        //移除消息出Message（的dicDatas字典集） by string键
        public void Remove(string key)
        {
            if (null != dicDatas && dicDatas.ContainsKey(key))
            {
                dicDatas.Remove(key);
            }
        }
        #endregion

        #region Send()发送本实例消息

        //发送本实例消息（调用MessageCenter消息中心的Instance.SendMessage发送自己）
        public void Send()
        {
            MessageCenter.Instance.SendMessage(this);
        }
        #endregion

    }
}

