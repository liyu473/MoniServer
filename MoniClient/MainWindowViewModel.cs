using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LyuMonionCore.Client;
using LyuMonionCore.Client.Handlers;
using MoniShared.SharedDto;
using MoniShared.SharedIService;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace MoniClient;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IMonionService _monion;
    private readonly NotificationClient _notificationClient;
    private AutoReconnectHandler? _reconnectHandler;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _connectionStatus = "未连接";

    public MainWindowViewModel(IMonionService monion, NotificationClient notificationClient)
    {
        _monion = monion;
        _notificationClient = notificationClient;

        // 监听连接状态变化
        _notificationClient.OnConnectionStateChanged(OnConnectionStateChanged);
    }

    private void OnConnectionStateChanged(bool connected)
    {
        IsConnected = connected;
        ConnectionStatus = connected ? "已连接" : "未连接";
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
            maxRetries: 3, // 无限重试
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
