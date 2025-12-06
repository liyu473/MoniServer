using MagicOnion;

namespace LyuMonionCore.Client.Polling;

/// <summary>
/// 轮询服务 - 定时调用服务获取数据
/// </summary>
internal class PollingService<TService, TResult> : IPollingHandle
    where TService : IService<TService>
{
    private readonly IMonionService _monion;
    private readonly Func<TService, UnaryResult<TResult>> _fetchFunc;
    private readonly Action<TResult>? _onData;
    private readonly Func<TResult, Task>? _onDataAsync;
    private readonly Action<Exception>? _onError;
    private readonly Func<Exception, Task>? _onErrorAsync;
    private readonly TimeSpan _interval;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    public bool IsRunning => _cts is not null && !_cts.IsCancellationRequested;

    internal PollingService(
        IMonionService monion,
        Func<TService, UnaryResult<TResult>> fetchFunc,
        TimeSpan interval,
        Action<TResult>? onData,
        Func<TResult, Task>? onDataAsync,
        Action<Exception>? onError,
        Func<Exception, Task>? onErrorAsync)
    {
        _monion = monion;
        _fetchFunc = fetchFunc;
        _interval = interval;
        _onData = onData;
        _onDataAsync = onDataAsync;
        _onError = onError;
        _onErrorAsync = onErrorAsync;
    }

    internal void Start(bool immediate = true)
    {
        if (IsRunning) return;

        _cts = new CancellationTokenSource();
        _ = PollingLoopAsync(immediate, _cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    public async Task FetchOnceAsync()
    {
        try
        {
            var service = _monion.Create<TService>();
            var result = await _fetchFunc(service);
            
            _onData?.Invoke(result);
            if (_onDataAsync is not null)
                await _onDataAsync(result);
        }
        catch (Exception ex)
        {
            _onError?.Invoke(ex);
            if (_onErrorAsync is not null)
                await _onErrorAsync(ex);
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
