using MoniClient.Service;
using System.Windows;

namespace MoniClient;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static JabService Service { get; } = new();
}
