using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MoniClient.Service;
using MoniShared.SharedIService;


namespace MoniClient;

public partial class MainWindowViewModel(IMonionService monion) : ObservableObject
{
    [RelayCommand]
    private async Task Get()
    {
        var client = monion.Create<ICalculator>();
        var result = await client.SumAsync(1,2);
        MessageBox.Show($"收到结果{result}");
    }
}

