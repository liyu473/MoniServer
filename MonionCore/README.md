# LyuMonionCore

基于 MagicOnion 的实时通信框架，提供服务调用、实时通知、自动重连、轮询等功能。

## 安装

```bash
dotnet add package LyuMonionCore

dotnet add package LyuMonion.JwtAuth //需要权鉴的话
```

## 服务端配置

```csharp
var builder = WebApplication.CreateBuilder(args);

// 配置 Kestrel 使用 HTTP/2（gRPC 必需）
builder.WebHost.ConfigureKestrel(options =>
{
    options.Configure(builder.Configuration.GetSection("Kestrel"));
});

builder.Services.AddMagicOnion();
// 如果需要权鉴
.AddJwtAuth(options =>
    {
        options.SecretKey = "YourSuperSecretKeyAtLeast32Characters!";  // 必填，至少32字符
        options.Issuer = "MyServer";           // 可选
        options.Audience = "MyClient";         // 可选
        options.ExpiresInMinutes = 60;         // 可选，默认60分钟（无时钟偏差）
        options.ExcludeServices("IAuthService"); // 排除不需要认证的服务
    });

builder.Services.AddMonionNotification();  // 添加通知推送服务（需要服务器向客户端推送时添加，为长连接区别于Unary）

var app = builder.Build();
app.MapMagicOnionService();
app.Run();
```

服务端 `appsettings.json`：
```json
{
  "Kestrel": {
    "Endpoints": {
      "gRPC": {
        "Url": "http://localhost:5000",
        "Protocols": "Http2"
      }
    }
  }
}
```

### 创建通知 Hub

```csharp
public class NotificationHub : NotificationHubBase
{
    // 可选：重写客户端加入/断开回调
    protected override void OnClientJoined(string clientName, Guid connectionId)
    {
        Console.WriteLine($"客户端 [{clientName}] 已加入");
    }

    protected override void OnClientDisconnected(string clientName)
    {
        Console.WriteLine($"客户端 [{clientName}] 已断开");
    }
}
```

### 推送消息

```csharp
public class Yourcalss(INotificationPushService pushService) 
{
    public async Task SendMessageFor(string clientName)
    {
        //泛型参数，此处演示string
        await pushService.PushToAll("Hello everyone!");//推送每个连接客户端

        await pushService.PushToClient(clientName, "Hello!");//推送给特点客户端
    }
}
```

## 客户端配置

```csharp
// 方式1：直接传地址（自动创建 GrpcChannel）
services.AddMonionClient("http://localhost:5000");

// 方式2：传入已有的 GrpcChannel（配合 JWT 认证时使用）
var channel = GrpcChannelBuilder
    .Create("http://localhost:5000")
    .WithJwtAuth(tokenStore)
    .Build();
services.AddMonionClient(channel); //此时channel已经注入成单例，GrpcChannel切勿频繁new
//附加IMonionService和NotificationClient一并注入成单例，直接使用
```

### 调用服务

```csharp
var client = monion.Create<IHelloService>(); //monion是从容器拿的IMonionService
var result = await client.SayHello("World");
```

### 接收实时通知

```csharp
// 注册消息处理器（服务器推送时触发）notificationClient（NotificationClient）从DI拿，没有DI就new
notificationClient
    .On<string>(msg => Console.WriteLine($"收到: {msg}"))
    .On<Person>(person => Console.WriteLine($"收到: {person.Name}"));

// 连接到服务器
await notificationClient.ConnectAsync("Client1");

// 断开连接
await notificationClient.DisconnectAsync();
```

### 监听连接状态

```csharp
notificationClient.OnConnectionStateChanged(connected =>
{
    Console.WriteLine(connected ? "已连接" : "已断开");
});
```

### 自动重连

```csharp
var reconnectHandler = notificationClient.EnableAutoReconnect(
    maxRetries: -1,  // -1 表示无限重试
    retryInterval: TimeSpan.FromSeconds(3),
    onReconnectAttempt: (current, max) => Console.WriteLine($"重连中 ({current})..."),
    onReconnectFailed: () => Console.WriteLine("重连失败")
);

// 停止自动重连
reconnectHandler.Dispose();
```

### 轮询服务

```csharp
var polling = monion.EnablePolling<IHelloService, string>(
    service => service.SayHello("heartbeat"),
    interval: TimeSpan.FromSeconds(3),
    onData: result => Console.WriteLine($"心跳: {result}"),
    onError: ex => Console.WriteLine($"错误: {ex.Message}")
);

// 暂停/恢复
polling.Pause();
polling.Resume();

// 停止
polling.Dispose();
```



## 配合 JWT 认证

参考 [LyuMonion.JwtAuth](../LyuMonion.JwtAuth/README.md) 项目。
