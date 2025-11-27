using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MoniClient.Service;
using MoniShared.SharedIService;


namespace MoniClient;

public partial class MainWindowViewModel(IMonionService monion) : ObservableObject
{
    [RelayCommand]
    private async Task Calculator()
    {
        var client = monion.Create<ICalculator>();
        var result = await client.SumAsync(1,2);
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
}

