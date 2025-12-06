namespace LyuMonionCore.Client.Handlers;

/// <summary>
/// 心跳管理器
/// </summary>
public class HeartbeatHandler : IDisposable
{
    private readonly NotificationClient _client;
    private readonly TimeSpan _interval;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    /// <summary>
    /// 心跳发送事件（同步）
    /// </summary>
    public event Action? HeartbeatSent;

    /// <summary>
    /// 心跳发送事件（异步）
    /// </summary>
    public event Func<Task>? HeartbeatSentAsync;

    /// <summary>
    /// 心跳失败事件（同步）
    /// </summary>
    public event Action<Exception>? HeartbeatFailed;

    /// <summary>
    /// 心跳失败事件（异步）
    /// </summary>
    public event Func<Exception, Task>? HeartbeatFailedAsync;

    internal HeartbeatHandler(NotificationClient client, TimeSpan interval)
    {
        _client = client;
        _interval = interval;
    }

    internal void Start()
    {
        _client.ConnectionStateChangedSync += OnConnectionStateChanged;

        if (_client.IsConnected)
        {
            StartHeartbeatLoop();
        }
    }

    private void OnConnectionStateChanged(bool connected)
    {
        if (connected)
        {
            StartHeartbeatLoop();
        }
        else
        {
            StopHeartbeatLoop();
        }
    }

    private void StartHeartbeatLoop()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        _ = HeartbeatLoopAsync(_cts.Token);
    }

    private void StopHeartbeatLoop()
    {
        _cts?.Cancel();
    }

    private async Task HeartbeatLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _client.IsConnected)
        {
            try
            {
                await Task.Delay(_interval, ct);

                if (_client.IsConnected)
                {
                    await _client.SendHeartbeatAsync();
                    
                    HeartbeatSent?.Invoke();
                    if (HeartbeatSentAsync is not null)
                        await HeartbeatSentAsync.Invoke();
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                HeartbeatFailed?.Invoke(ex);
                if (HeartbeatFailedAsync is not null)
                    await HeartbeatFailedAsync.Invoke(ex);
                break;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _client.ConnectionStateChangedSync -= OnConnectionStateChanged;
        _cts?.Cancel();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
