# LyuMonionCore / LyuMonion.JwtAuth

基于 [MagicOnion](https://github.com/Cysharp/MagicOnion) 框架的 gRPC 服务示例项目，包含服务端、客户端（仅限于.Net客户端）（以WPF为例），共享库（双方定义的服务接口和DTO）和包核心库，以及权鉴项目。

MagicOnion 是一个 **基于 gRPC 的 .NET 高性能实时通信框架**，主要用于 **客户端与服务器之间的 RPC 通信**

在开始使用前，请按照如下项目结构创建你的项目：

## 项目结构

```
MoniServer/
├── MoniServer/        # 服务端
├── MoniClient/        # WPF 客户端
└── MoniShared/        # 共享接口与 DTO

其中MoniShared依赖于LyuMonionCore，需要权鉴则需要依赖 LyuMonion.JwtAuth
MoniServer和MoniClient均引用MoniShared

MoniShared定义服务接口，服务器实现，客户端调用
DTO类需要附加[MessagePackObject(true)]特性
```

## 快速开始

详细使用说明请参考：

- [LyuMonionCore 文档](MonionCore/README.md) - 核心通信框架
- [LyuMonion.JwtAuth 文档](LyuMonion.JwtAuth/README.md) - JWT 认证扩展
