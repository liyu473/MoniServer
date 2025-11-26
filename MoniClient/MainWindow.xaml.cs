using System.Windows;

namespace MoniClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _vm;
        public MainWindow()
        {
            InitializeComponent();
            _vm = App.Service.GetService<MainWindowViewModel>();
            DataContext = _vm;
        }
    }
}