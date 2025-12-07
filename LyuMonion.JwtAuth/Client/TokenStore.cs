namespace LyuMonion.JwtAuth.Client;

/// <summary>
/// Token 存储（线程安全）
/// </summary>
public class TokenStore
{
    private string? _token;
    private readonly object _lock = new();

    /// <summary>
    /// 当前 Token
    /// </summary>
    public string? Token
    {
        get { lock (_lock) return _token; }
        set { lock (_lock) _token = value; }
    }

    /// <summary>
    /// 是否已登录
    /// </summary>
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    /// <summary>
    /// 清除 Token
    /// </summary>
    public void Clear() => Token = null;
}
