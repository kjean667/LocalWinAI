using Microsoft.UI.Xaml;

namespace LocalWinAI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainFrame.Navigate(typeof(ChatPage));
    }
}
