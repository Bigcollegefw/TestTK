// MessageCenter消息中心，用于管理消息流转
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
using System.Collections.Generic;
using UnityEngine;

namespace TKFramework
{
	public delegate void MessageEvent(Message message);
	//消息中心继承自单例
	public class MessageCenter : Singleton<MessageCenter>
	{
		//声明一个消息事件字典集，由 string 和 消息事件列表List<MessageEvent> 组成 键值对
		private Dictionary<string, List<MessageEvent>> dicMessageEvents = null;

		//初始化
		public override void Init()
		{
			dicMessageEvents = new Dictionary<string, List<MessageEvent>>();
		}


		#region 增加和移除监听
		/*被注销的内容
        //增加监听（重载1 by 消息类型&消息事件）
		public void AddListener(MessageType messageType, MessageEvent messageEvent)
		{
			AddListener(messageType.ToString(), messageEvent);
		}
        //移除监听（重载1 by 消息类型&消息事件）
		public void RemoveListener(MessageType messageType, MessageEvent messageEvent)
		{
			RemoveListener(messageType.ToString(), messageEvent);
		}
*/
		/// <summary>
		/// 增加监听（根方法 by 消息类型ToString名称
		/// </summary>
		/// <param name="messageName"></param>
		/// <param name="messageEvent"></param>&消息事件）
		public void AddListener(string messageName, MessageEvent messageEvent)
		{
			//Debug.Log("AddListener Name : " + messageName);
			//待执行的消息事件列表
			List<MessageEvent> list = null;
			//消息事件字典集中有该消息名时，将字典集中已有的消息事件列表读给list
			if (dicMessageEvents.ContainsKey(messageName))
			{
				list = dicMessageEvents[messageName];
			}
			//消息事件字典集中无该消息名时，将list置为一个空消息事件列表加入字典集
			else
			{
				list = new List<MessageEvent>();
				dicMessageEvents.Add(messageName, list);
			}
			//如果没有重复的消息事件，才向list（待执行的消息事件列表） 压入 传进来的新的消息事件
			if (!list.Contains(messageEvent))
			{
				list.Add(messageEvent);
			}
		}

		/// <summary>
		/// 移除监听（根方法 by 消息类型ToString名称
		/// </summary>
		/// <param name="messageName"></param>
		/// <param name="messageEvent"></param>&消息事件）
		public void RemoveListener(string messageName, MessageEvent messageEvent)
		{
			//消息事件字典集中有该消息名时，将字典集中已有的消息事件列表读给list，剔除对应的消息事件
			//Debug.Log("RemoveListener Name : " + messageName);
			if (dicMessageEvents.ContainsKey(messageName))
			{
				List<MessageEvent> list = dicMessageEvents[messageName];
				if (list.Contains(messageEvent))
				{
					list.Remove(messageEvent);
				}
				if (list.Count <= 0)
				{
					dicMessageEvents.Remove(messageName);
				}
			}
		}

		/// <summary>
		/// 移除所有监听（清空消息事件字典集）
		/// </summary>
		public void RemoveAllListener()
		{
			dicMessageEvents.Clear();
		}

		#endregion

		#region 发送消息

		/// <summary>
		/// 发送消息（根方法 by 消息类Message，调用了实际操作的信息调度方法DoMessageDispatcher）
		/// </summary>
		/// <param name="message"></param>
		public void SendMessage(Message message)
		{
			DoMessageDispatcher(message);
		}

		/// <summary>
		/// 发送消息（重载1 by 消息
		/// </summary>
		/// <param name="name"></param>
		/// <param name="sender"></param>&发件体）
		public void SendMessage(string name, object sender)
		{
			//此处使用了Message类的构造函数，传入参数并生成新的Message类，下同
			SendMessage(new Message(name, sender));
		}

		/// <summary>
		/// 发送消息（重载2 by 消息
		/// </summary>
		/// <param name="name"></param>
		/// <param name="sender"></param>
		/// <param name="content"></param>&发件体&内容）
		public void SendMessage(string name, object sender, object content)
		{
			SendMessage(new Message(name, sender, content));
		}

		/// <summary>
		/// 发送消息（重载3 by 消息&发件体&内容&可变数量（params）的object集合<将写入数据字典集>）
		/// </summary>
		public void SendMessage(string name, object sender, object content, params object[] dicParams)
		{
			SendMessage(new Message(name, sender, content, dicParams));
		}

		/// <summary>
		/// 信息调度方法（实际进行发送消息的方法）
		/// </summary>
		/// <param name="message"></param>
		private void DoMessageDispatcher(Message message)
		{
			//消息事件字典集找不到对应消息体名称时结束
			if (dicMessageEvents == null || !dicMessageEvents.ContainsKey(message.Name))
				return;

			//读取出消息事件字典集中的事件列表，并逐条对对应消息体进行消息事件
			List<MessageEvent> list = dicMessageEvents[message.Name];
			for (int i = 0; i < list.Count; i++)
			{
				MessageEvent messageEvent = list[i];
				if (null != messageEvent)
				{
					messageEvent(message);
				}
			}
		}

		#endregion

	}
}

