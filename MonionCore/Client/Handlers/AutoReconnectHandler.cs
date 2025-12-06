namespace LyuMonionCore.Client.Handlers;

/// <summary>
/// 自动重连管理器
/// </summary>
public class AutoReconnectHandler : IDisposable
{
    private readonly NotificationClient _client;
    private readonly int _maxRetries;
    private readonly TimeSpan _retryInterval;
    private CancellationTokenSource? _cts;
    private int _currentRetryCount;
    private bool _disposed;

    /// <summary>
    /// 重连尝试事件（同步）
    /// </summary>
    public event Action<int, int>? ReconnectAttempt;

    /// <summary>
    /// 重连尝试事件（异步）
    /// </summary>
    public event Func<int, int, Task>? ReconnectAttemptAsync;

    /// <summary>
    /// 重连失败事件（同步）
    /// </summary>
    public event Action? ReconnectFailed;

    /// <summary>
    /// 重连失败事件（异步）
    /// </summary>
    public event Func<Task>? ReconnectFailedAsync;

    internal AutoReconnectHandler(NotificationClient client, int maxRetries, TimeSpan retryInterval)
    {
        _client = client;
        _maxRetries = maxRetries;
        _retryInterval = retryInterval;
    }

    internal void Start()
    {
        _client.ConnectionStateChangedSync += OnConnectionStateChanged;
    }

    private async void OnConnectionStateChanged(bool connected)
    {
        if (!connected && !_disposed)
        {
            await TryReconnectAsync();
        }
        else if (connected)
        {
            _currentRetryCount = 0;
        }
    }

    private async Task TryReconnectAsync()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        while (!_disposed && (_maxRetries == -1 || _currentRetryCount < _maxRetries))
        {
            _currentRetryCount++;
            
            ReconnectAttempt?.Invoke(_currentRetryCount, _maxRetries);
            if (ReconnectAttemptAsync is not null)
                await ReconnectAttemptAsync.Invoke(_currentRetryCount, _maxRetries);

            try
            {
                await Task.Delay(_retryInterval, _cts.Token);
                await _client.ConnectInternalAsync();
                _ = _client.WaitForDisconnectAsync();
                return; // 重连成功
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch
            {
                // 继续重试
            }
        }

        ReconnectFailed?.Invoke();
        if (ReconnectFailedAsync is not null)
            await ReconnectFailedAsync.Invoke();
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
