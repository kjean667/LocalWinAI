namespace LocalWinAI.Application.Settings;

public interface IMcpProviderDescriptor
{
    string ServerKey { get; }
    string DisplayName { get; }
    string Description { get; }
    string Command { get; }
    string[] Args { get; }
}
