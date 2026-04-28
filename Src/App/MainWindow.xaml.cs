using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LocalWinAI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        NavView.SelectedItem = NavChat;
        MainFrame.Navigate(typeof(ChatPage));
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            var pageType = item.Tag switch
            {
                "Settings" => typeof(SettingsPage),
                "Statistics" => typeof(StatisticsPage),
                _ => typeof(ChatPage)
            };
            MainFrame.Navigate(pageType);
        }
    }
}
