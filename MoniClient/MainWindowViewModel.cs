using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LyuMonionCore.Client;
using LyuMonionCore.Client.Handlers;
using LyuMonionCore.Client.Polling;
using MoniShared.SharedDto;
using MoniShared.SharedIService;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace MoniClient;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IMonionService _monion;
    private readonly NotificationClient _notificationClient;
    private AutoReconnectHandler? _reconnectHandler;
    private IPollingHandle? _heartbeatPolling;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isHeartbeatFlashing;

    [ObservableProperty]
    private string _connectionStatus = "未连接";

    public MainWindowViewModel(IMonionService monion, NotificationClient notificationClient)
    {
        _monion = monion;
        _notificationClient = notificationClient;

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
            TimeSpan.FromSeconds(0.3),
            onData: _ => IsHeartbeatFlashing = !IsHeartbeatFlashing,
            onError: ex => System.Diagnostics.Debug.WriteLine($"心跳失败: {ex.Message}")
        );

        //add other polly works here if needed
    }

    [RelayCommand]
    private async Task Calculator()
    {
        var client = _monion.Create<ICalculator>();
        var result = await client.SumAsync(1, 2);
        MessageBox.Show($"收到结果{result}");
    }

    [RelayCommand]
    private async Task GetPerson()
    {
        var client = _monion.Create<IPersonService>();
        var result = await client.GetPerson();
        MessageBox.Show($"收到结果{result}");
    }

    [RelayCommand]
    private async Task SayHello()
    {
        var client = _monion.Create<IHelloService>();
        var result = await client.SayHello("小明");
        MessageBox.Show(result);
    }

    [RelayCommand]
    private async Task JoinRoom()
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
    }

    [RelayCommand]
    private async Task LeaveRoom()
    {
        _reconnectHandler?.Dispose();

        await _notificationClient.DisconnectAsync();
    }

    [RelayCommand]
    private async Task ServerSendForAll()
    {
        var client = _monion.Create<INotification>();
        await client.SendMessageFor("");
    }

    [RelayCommand]
    private async Task ServerSendForSingle()
    {
        var client = _monion.Create<INotification>();
        await client.SendMessageFor("Client1");
    }

    private void OnStringReceived(string msg) => MessageBox.Show($"[字符串] {msg}");
    private void OnPersonReceived(Person person) => MessageBox.Show($"[Person] {person}");

    [RelayCommand]
    private async Task SendPerson()
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
    }
}
