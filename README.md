# LyuMonionCore

基于 [MagicOnion](https://github.com/Cysharp/MagicOnion) 框架的 .NET 10 gRPC 服务示例项目，包含服务端、客户端（仅限于.Net客户端）（以WPF为例），共享库（双方定义的服务接口和DTO）和包核心库（内置MagicOnion和部分通用接口以简化使用）。

MagicOnion 是一个 **基于 gRPC 的 .NET 高性能实时通信框架**，主要用于 **客户端与服务器之间的 RPC 通信**

实时通信，类似于SignalR，传输速率快于WebApi

无须写控制器，无需写Http代码或Client代码，只需要定义接口调用函数即可

## 项目结构

```
MoniServer/
├── MoniServer/        # 服务端
├── MoniClient/        # WPF 客户端
├── MoniShared/        # 共享接口与 DTO
└── MonionCore/        # 通用通知库
```

## 快速开始

### 1. 定义服务接口（MoniShared）

```csharp
public interface ICalculator : IService<ICalculator>
{
    UnaryResult<int> SumAsync(int x, int y);
}
```

### 2. 实现服务（MoniServer）

```csharp
public class CalculatorService : ServiceBase<ICalculator>, ICalculator
{
    public async UnaryResult<int> SumAsync(int x, int y)
    {
        return x + y;
    }
}
```

### 3. 客户端调用

```csharp
var client = monion.Create<ICalculator>();
var result = await client.SumAsync(1, 2);


//这里的monion可简单打包成服务类，如下

public interface IMonionService
{
    T Create<T>() where T : IService<T>;
}

//这里的channel为单例管道。勿瞬态或频繁创建
public class MonionSerrvice(GrpcChannel channel) : IMonionService
{
    public T Create<T>()
        where T : IService<T>
    {
        return MagicOnionClient.Create<T>(channel);
    }
}

//channel注册如下，以jab容器为例
[ServiceProvider]
[Transient<IMonionService, MonionSerrvice>]
[Singleton<INotificationReceiver, NotificationReceiver>]
[Singleton(typeof(IConfiguration), Factory = nameof(BuildConfig))]
[Singleton(typeof(GrpcChannel), Factory = nameof(BuildChannel))]
public partial class JabService
{
    //服务器地址放在配置文件
    private static IConfiguration BuildConfig() =>
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

    //从配置读取地址
    private static GrpcChannel BuildChannel(IConfiguration config) =>
        GrpcChannel.ForAddress(config["MagicOnion:ServerUrl"] ?? "http://localhost:5000");
}
```

配置文件

```json
{
  "MagicOnion": {
    "ServerUrl": "http://localhost:17721"
  }
}
```



## 服务器推送（MonionCore）

### 服务端

1. 继承 `NotificationHubBase`：
```csharp
public class NotificationHub : NotificationHubBase 
{
    //1对多的情况，1对1可忽略
    protected override void OnClientJoined(string clientName, Guid connectionId)
    {
        
    }

    protected override void OnClientDisconnected(string clientName)
    {
       
    }
}
```

2. 注册推送服务：
```csharp
builder.Services.AddSingleton<INotificationPushService, NotificationPushService>();

builder.Services.AddMagicOnion();

app.MapMagicOnionService();
```

3. 推送消息：
```csharp
//泛型数据类型，加上MessagePackObject即可
// 推送给所有客户端
pushService.PushToAll("消息内容");
pushService.PushToAll(new Person { Name = "张三" });

// 推送给指定客户端
pushService.PushToClient("Client1", "只给你的消息");
```

### 客户端

1. 继承 `NotificationReceiverBase`：
```csharp
public class NotificationReceiver : NotificationReceiverBase//这里实现了接口
{
    public event Action<string>? StringReceived;
    public event Action<Person>? PersonReceived;

    public NotificationReceiver()
    {
        RegisterHandler<string>(s => StringReceived?.Invoke(s));
        RegisterHandler<Person>(p => PersonReceived?.Invoke(p));
    }

    //wpf之类的客户端的UI操作需要重写
    protected override void InvokeOnMainThread(Action action)
    {
        Application.Current.Dispatcher.Invoke(action);
    }
    
    //也可以不重写在Action里面加上UI线程操作
    //服务器推送是单向，暂不支持Func，需要返回值客户端直接请求就行
}
```

2. 连接并订阅：
```csharp
var receiver = new NotificationReceiver();
receiver.StringReceived += msg => MessageBox.Show(msg);

var hub = await StreamingHubClient.ConnectAsync<INotificationHub, INotificationReceiver>(channel, receiver);
await hub.JoinAsync("Client1");
```

## DTO 定义

```csharp
[MessagePackObject(true)]
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

## 配置

服务端 `appsettings.json`：
```json
{
  "Kestrel": {
    "Endpoints": {
      "gRPC": {
        "Url": "http://localhost:17721",
        "Protocols": "Http2"
      }
    }
  }
}
```
