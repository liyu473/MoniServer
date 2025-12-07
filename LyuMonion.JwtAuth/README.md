# LyuMonion.JwtAuth

为 LyuMonionCore 提供 JWT 认证的扩展库。

## 安装

```bash
dotnet add package LyuMonion.JwtAuth
```

## 服务端配置

```csharp
builder.Services.AddMagicOnion()
    .AddJwtAuth(options =>
    {
        options.SecretKey = "YourSuperSecretKeyAtLeast32Characters!";  // 必填，至少32字符
        options.Issuer = "MyServer";           // 可选
        options.Audience = "MyClient";         // 可选
        options.ExpiresInMinutes = 60;         // 可选，默认60分钟
        options.ExcludeServices("IAuthService"); // 排除不需要认证的服务
    });
```

### 创建认证服务

```csharp
public class AuthService(IJwtAuthService jwtService) : ServiceBase<IAuthService>, IAuthService
{
    public UnaryResult<string?> Login(string username, string password)
    {
        if (username == "admin" && password == "123456")
        {
            // 方式1：传 userId 和 userName
            var token = jwtService.GenerateToken("1", username);
            
            // 方式2：自定义 Claims
            var token = jwtService.GenerateToken(
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin")
            );
            
            return new UnaryResult<string?>(token);
        }
        return new UnaryResult<string?>(null as string);
    }
}
```

## 客户端配置

```csharp
// 创建 TokenStore
var tokenStore = new TokenStore();
services.AddSingleton(tokenStore);

// 创建带 JWT 认证的 GrpcChannel
var channel = GrpcChannelBuilder
    .Create("http://localhost:5000")
    .WithJwtAuth(tokenStore)
    .Build();

services.AddMonionClient(channel);
```

### 登录获取 Token

```csharp
var authService = monion.Create<IAuthService>();
var token = await authService.Login("admin", "123456");
if (token != null)
{
    tokenStore.Token = token;  // 保存 Token，后续请求自动携带
}
```

### 登出

```csharp
tokenStore.Clear();
```

## 配置选项

| 选项 | 类型 | 必填 | 默认值 | 说明 |
|------|------|------|--------|------|
| SecretKey | string | ✓ | - | 密钥，至少32字符 |
| Issuer | string | - | null | Token 签发者 |
| Audience | string | - | null | Token 接收者 |
| ExpiresInMinutes | int | - | 60 | Token 过期时间（分钟） |
