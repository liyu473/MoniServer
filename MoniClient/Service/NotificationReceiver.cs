using MessagePack;
using MoniShared.Notification;
using MoniShared.SharedDto;
using System.Windows;

namespace MoniClient.Service;

public class NotificationReceiver : INotificationReceiver
{
    /// <summary>
    /// 收到字符串消息
    /// </summary>
    public event Action<string>? StringReceived;

    /// <summary>
    /// 收到 Person 对象
    /// </summary>
    public event Action<Person>? PersonReceived;

    /// <summary>
    /// 收到未知类型（返回类型名和原始数据）
    /// </summary>
    public event Action<string, byte[]>? UnknownReceived;

    public void OnMessage(NotificationMessage message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            switch (message.Type)
            {
                case nameof(String):
                    var str = MessagePackSerializer.Deserialize<string>(message.Data);
                    StringReceived?.Invoke(str);
                    break;

                case nameof(Person):
                    var person = MessagePackSerializer.Deserialize<Person>(message.Data);
                    PersonReceived?.Invoke(person);
                    break;

                default:
                    // 未知类型，交给外部处理
                    UnknownReceived?.Invoke(message.Type, message.Data);
                    break;
            }
        });
    }
}
