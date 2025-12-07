using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grpc.Core;
using LogExtension;
using LyuMonion.JwtAuth.Client;
using LyuMonionCore.Client;
using LyuMonionCore.Client.Handlers;
using LyuMonionCore.Client.Polling;
using MoniShared.SharedDto;
using MoniShared.SharedIService;
using ZLogger;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace MoniClient;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IMonionService _monion;
    private readonly NotificationClient _notificationClient;
    private readonly TokenStore _tokenStore;
    private AutoReconnectHandler? _reconnectHandler;
    private IPollingHandle? _heartbeatPolling;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isHeartbeatFlashing;

    [ObservableProperty]
    private string _connectionStatus = "未连接";

    [ObservableProperty]
    private string _tokenStatus = "未登录";

    public MainWindowViewModel(IMonionService monion, NotificationClient notificationClient, TokenStore tokenStore)
    {
        _monion = monion;
        _notificationClient = notificationClient;
        _tokenStore = tokenStore;

        // 监听连接状态变化
        _notificationClient.OnConnectionStateChanged(OnConnectionStateChanged);

        // 轮询使用 Unary 服务，独立于 StreamingHub 房间连接，应用启动即开始
        StartHeartbeatPolling();
    }

    private void OnConnectionStateChanged(bool connected)
    {
        IsConnected = connected;
        ConnectionStatus = connected ? "已连接" : "未连接";
    }

    /// <summary>
    /// 轮询走的是 Unary 服务，不依赖于通知连接
    /// </summary>
    private void StartHeartbeatPolling()
    {
        _heartbeatPolling = _monion.EnablePolling<IHelloService, string>(
            service => service.SayHello("heartbeat"),
            TimeSpan.FromSeconds(3),
            onData: _ => IsHeartbeatFlashing = !IsHeartbeatFlashing,
            onError: ex => ZLogFactory.Get<MainWindowViewModel>().ZLogError($"心跳失败:{ex}")
        );
    }

    /// <summary>
    /// 登录获取 Token
    /// </summary>
    [RelayCommand]
    private async Task Login()
    {
        try
        {
            var authService = _monion.Create<IAuthService>();
            var token = await authService.Login("admin", "123456");

            if (token is not null)
            {
                _tokenStore.Token = token;
                TokenStatus = "已登录 ✓";
                MessageBox.Show("登录成功！Token 已保存");
            }
            else
            {
                MessageBox.Show("登录失败");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"登录异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 登出清除 Token
    /// </summary>
    [RelayCommand]
    private async Task Logout()
    {
        // 断开 Hub 连接
        _reconnectHandler?.Dispose();
        _reconnectHandler = null;
        await _notificationClient.DisconnectAsync();

        // 清除 Token
        _tokenStore.Clear();
        TokenStatus = "未登录";
        MessageBox.Show("已登出");
    }

    /// <summary>
    /// 通用调用包装，处理认证失败等错误
    /// </summary>
    private async Task SafeCallAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unauthenticated)
        {
            MessageBox.Show("认证失败，请先登录！");
        }
        catch (RpcException ex)
        {
            MessageBox.Show($"RPC 错误: {ex.Status.Detail}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"错误: {ex.Message}");
        }
    }

    [RelayCommand]
    private Task Calculator() => SafeCallAsync(async () =>
    {
        var client = _monion.Create<ICalculator>();
        var result = await client.SumAsync(1, 2);
        MessageBox.Show($"收到结果{result}");
    });

    [RelayCommand]
    private Task GetPerson() => SafeCallAsync(async () =>
    {
        var client = _monion.Create<IPersonService>();
        var result = await client.GetPerson();
        MessageBox.Show($"收到结果{result}");
    });

    [RelayCommand]
    private Task SayHello() => SafeCallAsync(async () =>
    {
        var client = _monion.Create<IHelloService>();
        var result = await client.SayHello("小明");
        MessageBox.Show(result);
    });

    [RelayCommand]
    private Task JoinRoom() => SafeCallAsync(async () =>
    {
        _notificationClient
            .On<string>(OnStringReceived)
            .On<Person>(OnPersonReceived);

        // 启用自动重连
        _reconnectHandler = _notificationClient.EnableAutoReconnect(
            maxRetries: -1,
            retryInterval: TimeSpan.FromSeconds(3),
            onReconnectAttempt: (current, max) => ConnectionStatus = $"重连中 ({current})...",
            onReconnectFailed: () => ConnectionStatus = "重连失败"
        );

        await _notificationClient.ConnectAsync("Client1");
    });

    [RelayCommand]
    private async Task LeaveRoom()
    {
        _reconnectHandler?.Dispose();
        await _notificationClient.DisconnectAsync();
    }

    [RelayCommand]
    private Task ServerSendForAll() => SafeCallAsync(async () =>
    {
        var client = _monion.Create<INotification>();
        await client.SendMessageFor("");
    });

    [RelayCommand]
    private Task ServerSendForSingle() => SafeCallAsync(async () =>
    {
        var client = _monion.Create<INotification>();
        await client.SendMessageFor("Client1");
    });

    private void OnStringReceived(string msg) => MessageBox.Show($"[字符串] {msg}");
    private void OnPersonReceived(Person person) => MessageBox.Show($"[Person] {person}");

    [RelayCommand]
    private Task SendPerson() => SafeCallAsync(async () =>
    {
        var client = _monion.Create<INotification>();
        await client.SendPersonFor(
            new Person()
            {
                Id = 2,
                Name = "小红",
                Age = 20,
            }
        );
    });
}
