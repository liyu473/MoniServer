using Grpc.Net.Client;
using LyuMonionCore.Abstractions;
using LyuMonionCore.Models;
using MagicOnion.Client;
using MessagePack;

namespace LyuMonionCore.Client;

/// <summary>
/// 通知客户端 - 封装连接管理和消息订阅
/// </summary>
/// <example>
/// var client = new NotificationClient("https://localhost:5001");
/// client.On&lt;string&gt;(msg => Console.WriteLine(msg));
/// client.On&lt;Person&gt;(person => Console.WriteLine(person));
/// await client.ConnectAsync("MyClient");
/// 
/// // 断开连接
/// await client.DisconnectAsync();
/// </example>
public class NotificationClient : INotificationReceiver, IAsyncDisposable
{
    private readonly GrpcChannel _channel;
    private readonly Dictionary<string, Action<byte[]>> _handlers = [];
    private readonly Action<Action>? _dispatcher;
    private INotificationHub? _hub;
    private bool _disposed;

    /// <summary>
    /// 连接状态
    /// </summary>
    public bool IsConnected => _hub is not null;

    /// <summary>
    /// 客户端名称
    /// </summary>
    public string? ClientName { get; private set; }

    /// <summary>
    /// 创建通知客户端
    /// </summary>
    /// <param name="serverAddress">服务器地址</param>
    /// <param name="dispatcher">UI线程调度器（WPF: action => Dispatcher.Invoke(action)）</param>
    public NotificationClient(string serverAddress, Action<Action>? dispatcher = null)
    {
        _channel = GrpcChannel.ForAddress(serverAddress);
        _dispatcher = dispatcher;
    }

    /// <summary>
    /// 创建通知客户端（使用已有的 GrpcChannel）
    /// </summary>
    /// <param name="channel">gRPC 通道</param>
    /// <param name="dispatcher">UI线程调度器</param>
    public NotificationClient(GrpcChannel channel, Action<Action>? dispatcher = null)
    {
        _channel = channel;
        _dispatcher = dispatcher;
    }

    /// <summary>
    /// 注册消息处理器
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="handler">处理函数</param>
    public NotificationClient On<T>(Action<T> handler)
    {
        _handlers[typeof(T).Name] = data =>
        {
            var obj = MessagePackSerializer.Deserialize<T>(data);
            handler(obj);
        };
        return this;
    }

    /// <summary>
    /// 连接到服务器并加入通知
    /// </summary>
    /// <param name="clientName">客户端标识名称</param>
    public async Task ConnectAsync(string clientName)
    {
        if (_hub is not null)
            throw new InvalidOperationException("Already connected. Call DisconnectAsync first.");

        ClientName = clientName;
        _hub = await StreamingHubClient.ConnectAsync<INotificationHub, INotificationReceiver>(_channel, this);
        await _hub.JoinAsync(clientName);
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (_hub is not null)
        {
            await _hub.DisposeAsync();
            _hub = null;
            ClientName = null;
        }
    }

    /// <summary>
    /// 接收消息（由 MagicOnion 调用）
    /// </summary>
    void INotificationReceiver.OnMessage(NotificationMessage message)
    {
        void ProcessMessage()
        {
            if (_handlers.TryGetValue(message.Type, out var handler))
            {
                handler(message.Data);
            }
            else
            {
                OnUnknownMessage?.Invoke(message.Type, message.Data);
            }
        }

        if (_dispatcher is not null)
            _dispatcher(ProcessMessage);
        else
            ProcessMessage();
    }

    /// <summary>
    /// 收到未注册类型的消息时触发
    /// </summary>
    public event Action<string, byte[]>? OnUnknownMessage;

    /// <summary>
    /// 释放资源
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await DisconnectAsync();
        _channel.Dispose();
        GC.SuppressFinalize(this);
    }
}
