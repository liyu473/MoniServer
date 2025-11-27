using MessagePack;

namespace MonionCore.Notification;

/// <summary>
/// 通知接收器基类 - 客户端继承此类并注册类型处理器
/// </summary>
public abstract class NotificationReceiverBase : INotificationReceiver
{
    private readonly Dictionary<string, Action<byte[]>> _handlers = new();

    /// <summary>
    /// 注册类型处理器
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="handler">处理函数</param>
    protected void RegisterHandler<T>(Action<T> handler)
    {
        _handlers[typeof(T).Name] = data =>
        {
            var obj = MessagePackSerializer.Deserialize<T>(data);
            handler(obj);
        };
    }

    /// <summary>
    /// 接收消息（由 MagicOnion 调用）
    /// </summary>
    public void OnMessage(NotificationMessage message)
    {
        // 切换到主线程（子类可重写）
        InvokeOnMainThread(() =>
        {
            if (_handlers.TryGetValue(message.Type, out var handler))
            {
                handler(message.Data);
            }
            else
            {
                OnUnknownMessage(message.Type, message.Data);
            }
        });
    }

    /// <summary>
    /// 在主线程执行（WPF/WinForms 需要重写此方法）
    /// </summary>
    protected virtual void InvokeOnMainThread(Action action)
    {
        action();
    }

    /// <summary>
    /// 收到未注册类型的消息时调用
    /// </summary>
    protected virtual void OnUnknownMessage(string type, byte[] data) { }
}
