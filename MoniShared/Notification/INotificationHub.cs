using MagicOnion;

namespace MoniShared.Notification;

/// <summary>
/// 服务器实现（主房间，客户端）
/// 一个客户端对应一个Hub实例
/// </summary>
public interface INotificationHub : IStreamingHub<INotificationHub, INotificationReceiver>
{
    /// <summary>
    /// 加入房间
    /// </summary>
    /// <param name="clientName"></param>
    /// <returns></returns>
    Task JoinAsync(string clientName);

    /// <summary>
    /// 客户端发送消息到服务器
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task SendAsync(string message);
}
