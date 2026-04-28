using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LocalWinAI.Application.Settings;

public partial class SettingsPageViewModel : ObservableObject
{
    private readonly IClaudeCodeSettingsService _settingsService;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public string SettingsFilePath => _settingsService.SettingsFilePath;
    public ObservableCollection<McpProviderState> Providers { get; } = [];

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }

    public SettingsPageViewModel(IClaudeCodeSettingsService settingsService, IEnumerable<IMcpProviderDescriptor> providers)
    {
        _settingsService = settingsService;

        foreach (var p in providers)
            Providers.Add(new McpProviderState(p, false));

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var settings = await _settingsService.LoadAsync();
            foreach (var p in Providers)
                p.IsRegistered = settings.McpServers.ContainsKey(p.Descriptor.ServerKey);
            StatusMessage = string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load settings: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            var settings = await _settingsService.LoadAsync();

            foreach (var p in Providers)
            {
                if (p.IsRegistered)
                    settings.McpServers[p.Descriptor.ServerKey] = new McpServerEntry
                    {
                        Command = p.Descriptor.Command,
                        Args = [.. p.Descriptor.Args]
                    };
                else
                    settings.McpServers.Remove(p.Descriptor.ServerKey);
            }

            await _settingsService.SaveAsync(settings);
            StatusMessage = "Settings saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to save settings: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
