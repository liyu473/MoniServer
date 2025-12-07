using Grpc.Core;
using Grpc.Net.Client;

namespace LyuMonion.JwtAuth.Client;

/// <summary>
/// GrpcChannelBuilder 扩展方法
/// </summary>
public static class GrpcChannelBuilderExtensions
{
    /// <summary>
    /// 添加 JWT 认证（使用 TokenStore）
    /// </summary>
    public static GrpcChannelBuilder WithJwtAuth(this GrpcChannelBuilder builder, TokenStore tokenStore)
    {
        builder.TokenProvider = () => tokenStore.Token;
        return builder;
    }

    /// <summary>
    /// 添加 JWT 认证（使用自定义 Token 提供器）
    /// </summary>
    public static GrpcChannelBuilder WithJwtAuth(this GrpcChannelBuilder builder, Func<string?> tokenProvider)
    {
        builder.TokenProvider = tokenProvider;
        return builder;
    }

    /// <summary>
    /// 使用 HTTPS（默认 HTTP）
    /// </summary>
    public static GrpcChannelBuilder UseSecure(this GrpcChannelBuilder builder)
    {
        builder.UseSecureFlag = true;
        return builder;
    }

    /// <summary>
    /// 构建 GrpcChannel
    /// </summary>
    public static GrpcChannel Build(this GrpcChannelBuilder builder)
    {
        // 无认证
        if (builder.TokenProvider is null)
        {
            return GrpcChannel.ForAddress(builder.ServerAddress);
        }

        // 有认证
        var credentials = CallCredentials.FromInterceptor((context, metadata) =>
        {
            var token = builder.TokenProvider();
            if (!string.IsNullOrEmpty(token))
            {
                metadata.Add("Authorization", $"Bearer {token}");
            }
            return Task.CompletedTask;
        });

        if (builder.UseSecureFlag)
        {
            return GrpcChannel.ForAddress(builder.ServerAddress, new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(ChannelCredentials.SecureSsl, credentials)
            });
        }

        return GrpcChannel.ForAddress(builder.ServerAddress, new GrpcChannelOptions
        {
            Credentials = ChannelCredentials.Create(ChannelCredentials.Insecure, credentials),
            UnsafeUseInsecureChannelCallCredentials = true
        });
    }
}
