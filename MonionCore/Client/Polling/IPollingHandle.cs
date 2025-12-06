namespace LyuMonionCore.Client.Polling;

/// <summary>
/// 轮询控制句柄
/// </summary>
public interface IPollingHandle : IDisposable
{
    /// <summary>
    /// 是否正在运行
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 停止轮询
    /// </summary>
    void Stop();

    /// <summary>
    /// 手动执行一次
    /// </summary>
    Task FetchOnceAsync();
}
