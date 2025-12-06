using MagicOnion;

namespace LyuMonionCore.Client;

/// <summary>
/// MagicOnion 服务工厂接口
/// </summary>
public interface IMonionService
{
    /// <summary>
    /// 创建 MagicOnion 服务客户端
    /// </summary>
    /// <typeparam name="T">服务接口类型</typeparam>
    T Create<T>() where T : IService<T>;
}
