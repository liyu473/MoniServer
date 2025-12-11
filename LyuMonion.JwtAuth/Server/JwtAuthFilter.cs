using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;

namespace LyuMonion.JwtAuth.Server;

/// <summary>
/// Unary 服务 Token 验证过滤器
/// </summary>
internal class JwtAuthFilter(IJwtAuthService jwtService, HashSet<string> excludedServices)
    : MagicOnionFilterAttribute
{
    public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
    {
        // 检查是否排除此服务
        if (IsExcluded(context.ServiceType))
        {
            await next(context);
            return;
        }

        var token = ExtractToken(context.CallContext.RequestHeaders);
        if (string.IsNullOrEmpty(token))
        {
            throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Missing token");
        }

        var serviceProvider = context.ServiceProvider;
        var validationResult = await jwtService.ValidateTokenAsync(token, serviceProvider);

        if (validationResult is null)
        {
            throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Invalid token");
        }

        if (validationResult.IsFailed)
        {
            throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, validationResult.FailureMessage ?? "Token validation failed");
        }

        await next(context);
    }

    private bool IsExcluded(Type serviceType)
    {
        // 检查实现类名
        if (excludedServices.Contains(serviceType.Name))
            return true;

        // 检查接口名
        var interfaceName = serviceType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IService<>))
            ?.GetGenericArguments().FirstOrDefault()?.Name;

        return interfaceName is not null && excludedServices.Contains(interfaceName);
    }

    private static string? ExtractToken(Grpc.Core.Metadata headers)
    {
        var authorization = headers
            .FirstOrDefault(h => h.Key.Equals("authorization", StringComparison.OrdinalIgnoreCase))?.Value;

        if (string.IsNullOrEmpty(authorization))
            return null;

        return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization["Bearer ".Length..]
            : authorization;
    }
}

/// <summary>
/// StreamingHub Token 验证过滤器
/// </summary>
internal class JwtAuthStreamingHubFilter(IJwtAuthService jwtService) : StreamingHubFilterAttribute
{
    public override async ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
    {
        var authorization = context.ServiceContext.CallContext.RequestHeaders
            .FirstOrDefault(h => h.Key.Equals("authorization", StringComparison.OrdinalIgnoreCase))?.Value;

        var token = string.IsNullOrEmpty(authorization) ? null
            : authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authorization["Bearer ".Length..]
                : authorization;

        if (string.IsNullOrEmpty(token))
        {
            throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Missing token");
        }

        var serviceProvider = context.ServiceContext.ServiceProvider;
        var validationResult = await jwtService.ValidateTokenAsync(token, serviceProvider);

        if (validationResult is null)
        {
            throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Invalid token");
        }

        if (validationResult.IsFailed)
        {
            throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, validationResult.FailureMessage ?? "Token validation failed");
        }

        await next(context);
    }
}
