using Grpc.Core;
using Grpc.Net.Client;

namespace LyuMonion.JwtAuth.Client;

/// <summary>
/// 客户端 JWT 扩展方法
/// </summary>
public static class ClientExtensions
{
    /// <summary>
    /// 创建带 JWT 认证的 GrpcChannel
    /// </summary>
    /// <param name="serverAddress">服务器地址</param>
    /// <param name="tokenProvider">Token 提供器</param>
    /// <example>
    /// var channel = GrpcChannelFactory.CreateWithJwtAuth(
    ///     "http://localhost:5000",
    ///     () => tokenStore.Token
    /// );
    /// </example>
    public static GrpcChannel CreateWithJwtAuth(string serverAddress, Func<string?> tokenProvider)
    {
        var credentials = CallCredentials.FromInterceptor((context, metadata) =>
        {
            var token = tokenProvider();
            if (!string.IsNullOrEmpty(token))
            {
                metadata.Add("Authorization", $"Bearer {token}");
            }
            return Task.CompletedTask;
        });

        return GrpcChannel.ForAddress(serverAddress, new GrpcChannelOptions
        {
            Credentials = ChannelCredentials.Create(ChannelCredentials.Insecure, credentials),
            UnsafeUseInsecureChannelCallCredentials = true
        });
    }

    /// <summary>
    /// 创建带 JWT 认证的 GrpcChannel（使用 TokenStore）
    /// </summary>
    public static GrpcChannel CreateWithJwtAuth(string serverAddress, TokenStore tokenStore)
    {
        return CreateWithJwtAuth(serverAddress, () => tokenStore.Token);
    }

    /// <summary>
    /// 创建带 JWT 认证的 GrpcChannel（HTTPS）
    /// </summary>
    public static GrpcChannel CreateWithJwtAuthSecure(string serverAddress, Func<string?> tokenProvider)
    {
        var credentials = CallCredentials.FromInterceptor((context, metadata) =>
        {
            var token = tokenProvider();
            if (!string.IsNullOrEmpty(token))
            {
                metadata.Add("Authorization", $"Bearer {token}");
            }
            return Task.CompletedTask;
        });

        return GrpcChannel.ForAddress(serverAddress, new GrpcChannelOptions
        {
            Credentials = ChannelCredentials.Create(ChannelCredentials.SecureSsl, credentials)
        });
    }
}
