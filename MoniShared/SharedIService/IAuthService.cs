using MagicOnion;

namespace MoniShared.SharedIService;

public interface IAuthService : IService<IAuthService>
{
    /// <summary>
    /// 登录获取 Token
    /// </summary>
    UnaryResult<string?> Login(string username, string password);
}
