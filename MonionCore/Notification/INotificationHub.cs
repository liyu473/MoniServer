using MagicOnion;

namespace LyuMonionCore.Notification;

/// <summary>
/// 服务器实现（主房间，客户端）
/// 一个客户端对应一个Hub实例
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
