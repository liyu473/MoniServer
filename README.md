# MoniServer

基于 [MagicOnion](https://github.com/Cysharp/MagicOnion) 框架的 .NET 10 gRPC 服务示例项目，包含服务端、客户端（仅限于.Net客户端）（以WPF为例）和共享库。

MagicOnion 是一个 **基于 gRPC 的 .NET 高性能实时通信框架**，主要用于 **客户端与服务器之间的 RPC 通信**

实时通信，类似于SignalR，传输速率快于WebApi

无须写控制器，无需写Http代码或Client代码，只需要定义接口调用函数即可

## 项目结构

```
MoniServer/
├── MoniServer/        # ASP.NET Core gRPC 服务端
├── MoniClient/        # WPF 桌面客户端
└── MoniShared/        # 共享接口与 DTO
```

### 

## 配置

### 服务端配置 (`appsettings.json`)

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

## 开发说明

### 添加新服务

1. 在 `MoniShared/SharedIService/` 中定义接口（接口即服务）：
   ```csharp
   public interface IMyService : IService<IMyService>
   {
       UnaryResult<string> MyMethod();
   }
   ```

2. 在 `MoniServer/Services/` 中实现服务：
   ```csharp
   public class MyService : ServiceBase<IMyService>, IMyService
   {
       public async UnaryResult<string> MyMethod()
       {
           return "Hello";
       }
   }
   ```

### 添加 DTO

在 `MoniShared/SharedDto/` 中定义，使用 MessagePack 特性：

```csharp
[MessagePackObject]
public class MyDto
{
    [Key(0)]
    public int Id { get; set; }
}
```

客户端（WPF）里定义了服务，可以通过IMonionService（已注入容器）

~~~C#
IMonionService monion
var client = monion.Create<ICalculator>();
var result = await client.SumAsync(1,2);
~~~

