namespace LyuMonion.JwtAuth.Client;

/// <summary>
/// GrpcChannel 构建器
/// </summary>
public class GrpcChannelBuilder
{
    internal readonly string ServerAddress;
    internal Func<string?>? TokenProvider;
    internal bool UseSecureFlag;

    private GrpcChannelBuilder(string serverAddress)
    {
        ServerAddress = serverAddress;
    }

    /// <summary>
    /// 创建构建器
    /// </summary>
    /// <example>
    /// var channel = GrpcChannelBuilder.Create("http://localhost:5000")
    ///     .WithJwtAuth(tokenStore)
    ///     .Build();
    /// </example>
    public static GrpcChannelBuilder Create(string serverAddress) => new(serverAddress);
}
