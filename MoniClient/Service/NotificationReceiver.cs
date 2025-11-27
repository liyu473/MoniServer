using MonionCore.Notification;
using MoniShared.SharedDto;
using System.Windows;

namespace MoniClient.Service;

/// <summary>
/// 通知接收器 - 继承 MonionCore 的基类
/// </summary>
public class NotificationReceiver : NotificationReceiverBase
{
    public event Action<string>? StringReceived;
    public event Action<Person>? PersonReceived;

    public NotificationReceiver()
    {
        // 注册类型处理器
        RegisterHandler<string>(str => StringReceived?.Invoke(str));
        RegisterHandler<Person>(person => PersonReceived?.Invoke(person));
    }

    /// <summary>
    /// WPF 需要切换到 UI 线程
    /// </summary>
    protected override void InvokeOnMainThread(Action action)
    {
        Application.Current.Dispatcher.Invoke(action);
    }

    protected override void OnUnknownMessage(string type, byte[] data)
    {
        // 接收到意外数据
    }    
}
