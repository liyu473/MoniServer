using LyuMonion.JwtAuth.Server;
using MagicOnion;
using MagicOnion.Server;
using MoniShared.SharedIService;

namespace MoniServer.Services;

public class AuthService(IJwtAuthService jwtService) : ServiceBase<IAuthService>, IAuthService
{
    public UnaryResult<string?> Login(string username, string password)
    {
        // 示例：简单验证，实际应该查数据库
        if (username == "admin" && password == "123456")
        {
            var token = jwtService.GenerateToken(userId: "1", userName: username);
            return new UnaryResult<string?>(token);
        }

        return new UnaryResult<string?>(null as string);
    }
}
