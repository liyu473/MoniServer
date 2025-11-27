using MessagePack;

namespace MonionCore.Notification;

/// <summary>
/// 通用通知消息
/// </summary>
[MessagePackObject]
public class NotificationMessage
{
    /// <summary>
    /// 数据类型标识（如 "Person", "Order"）
    /// </summary>
    [Key(0)]
    public string Type { get; set; } = "";

    /// <summary>
    /// MessagePack 序列化后的数据
    /// </summary>
    [Key(1)]
    public byte[] Data { get; set; } = [];
}
