// MessageType信息类型
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
namespace TKFramework
{
    public class MessageType
    {
        public static string Player_Move_Start = "Player_Move_Start";

        /// <summary>
        /// 切换语言事件名称
        /// </summary>
        public static string Language_Change = "Language_Change";
    }
}

