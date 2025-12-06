using MagicOnion;

namespace LyuMonionCore.Client.Polling;

/// <summary>
/// 轮询服务 - 定时调用服务获取数据
/// </summary>
public class PollingService<TService, TResult> : IDisposable
    where TService : IService<TService>
{
    private readonly IMonionService _monion;
    private readonly Func<TService, Task<TResult>> _fetchFunc;
    private readonly TimeSpan _interval;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    /// <summary>
    /// 是否正在运行
    /// </summary>
    public bool IsRunning => _cts is not null && !_cts.IsCancellationRequested;

    /// <summary>
    /// 收到数据时触发
    /// </summary>
    public event Action<TResult>? OnData;

    /// <summary>
    /// 发生错误时触发
    /// </summary>
    public event Action<Exception>? OnError;

    /// <summary>
    /// 创建轮询服务（推荐使用 IMonionService.CreatePolling 扩展方法）
    /// </summary>
    public PollingService(IMonionService monion, Func<TService, Task<TResult>> fetchFunc, TimeSpan interval)
    {
        _monion = monion;
        _fetchFunc = fetchFunc;
        _interval = interval;
    }

    /// <summary>
    /// 开始轮询
    /// </summary>
    /// <param name="immediate">是否立即执行一次</param>
    public void Start(bool immediate = true)
    {
        if (IsRunning) return;

        _cts = new CancellationTokenSource();
        _ = PollingLoopAsync(immediate, _cts.Token);
    }

    /// <summary>
    /// 停止轮询
    /// </summary>
    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    /// <summary>
    /// 手动执行一次
    /// </summary>
    public async Task FetchOnceAsync()
    {
        try
        {
            var service = _monion.Create<TService>();
            var result = await _fetchFunc(service);
            OnData?.Invoke(result);
        }
        catch (Exception ex)
        {
            OnError?.Invoke(ex);
        }
    }

    private async Task PollingLoopAsync(bool immediate, CancellationToken ct)
    {
        if (immediate)
        {
            await FetchOnceAsync();
        }

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, ct);
                await FetchOnceAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Stop();
        GC.SuppressFinalize(this);
    }
}
