using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalWinAI.Application.Settings;

public partial class McpProviderState : ObservableObject
{
    public IMcpProviderDescriptor Descriptor { get; }

    [ObservableProperty]
    public partial bool IsRegistered { get; set; }

    public McpProviderState(IMcpProviderDescriptor descriptor, bool isRegistered)
    {
        Descriptor = descriptor;
        IsRegistered = isRegistered;
    }
}
