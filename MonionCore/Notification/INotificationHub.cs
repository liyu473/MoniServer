using MagicOnion;

namespace MonionCore.Notification;

/// <summary>
/// 通知 Hub 接口 - 定义客户端可以调用的方法
/// </summary>
public interface INotificationHub : IStreamingHub<INotificationHub, INotificationReceiver>
{
    /// <summary>
    /// 加入通知订阅
    /// </summary>
    /// <param name="clientName">客户端标识名称</param>
    Task JoinAsync(string clientName);

    /// <summary>
    /// 发送消息（预留接口）
    /// </summary>
    Task SendAsync(string message);
}
