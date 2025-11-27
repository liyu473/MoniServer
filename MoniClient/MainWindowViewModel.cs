using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grpc.Net.Client;
using MagicOnion.Client;
using MoniClient.Extension;
using MoniClient.Service;
using MoniShared.Notification;
using MoniShared.SharedIService;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace MoniClient;

public partial class MainWindowViewModel(
    IMonionService monion,
    GrpcChannel channel,
    INotificationReceiver receiver
) : ObservableObject
{
    private INotificationHub? hub;

    [RelayCommand]
    private async Task Calculator()
    {
        var client = monion.Create<ICalculator>();
        var result = await client.SumAsync(1, 2);
        MessageBox.Show($"收到结果{result}");
    }

    [RelayCommand]
    private async Task GetPerson()
    {
        var client = monion.Create<IPersonService>();
        var result = await client.GetPerson();
        MessageBox.Show($"收到结果{result}");
    }

    [RelayCommand]
    private async Task SayHello()
    {
        var client = monion.Create<IHelloService>();
        var result = await client.SayHello("小明");
        MessageBox.Show(result);
    }

    [RelayCommand]
    private async Task JoinRoom()
    {
        receiver.GetReceiver().MessageReceived += OnMessage;

        hub = await StreamingHubClient.ConnectAsync<INotificationHub, INotificationReceiver>(
            channel,
            receiver
        );

        await hub.JoinAsync("Client1");
    }

    [RelayCommand]
    private async Task LeaveRoom()
    {
        if (hub is not null)
        {
            receiver.GetReceiver().MessageReceived -= OnMessage;
            await hub.DisposeAsync();
        }
    }

    [RelayCommand]
    private async Task ServerSendForAll()
    {
        var client = monion.Create<INotification>();
        await client.SendMessageFor("");
    }

    [RelayCommand]
    private async Task ServerSendForSingle()
    {
        var client = monion.Create<INotification>();
        await client.SendMessageFor("Client1");
    }

    private void OnMessage(string msg) => MessageBox.Show(msg);
}
