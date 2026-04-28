using LocalWinAI.Application.Statistics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace LocalWinAI;

public sealed partial class StatisticsPage : Page
{
    public StatisticsPageViewModel ViewModel { get; }

    public StatisticsPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<StatisticsPageViewModel>();
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.Refresh();
    }
}
