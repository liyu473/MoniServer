using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LyuMonionCore.Client;
using MoniClient.Service;
using MoniShared.SharedDto;
using MoniShared.SharedIService;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace MoniClient;

public partial class MainWindowViewModel(IMonionService monion, NotificationClient notificationClient)
    : ObservableObject
{
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
        notificationClient
            .On<string>(OnStringReceived)
            .On<Person>(OnPersonReceived);

        await notificationClient.ConnectAsync("Client1");
    }

    [RelayCommand]
    private async Task LeaveRoom()
    {
        await notificationClient.DisconnectAsync();
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

    private void OnStringReceived(string msg) => MessageBox.Show($"[字符串] {msg}");
    private void OnPersonReceived(Person person) => MessageBox.Show($"[Person] {person}");

    [RelayCommand]
    private async Task SendPerson()
    {
        var client = monion.Create<INotification>();
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
