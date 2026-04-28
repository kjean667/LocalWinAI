namespace LocalWinAI.Application.Settings;

public interface IClaudeCodeSettingsService
{
    string SettingsFilePath { get; }
    Task<ClaudeCodeSettings> LoadAsync();
    Task SaveAsync(ClaudeCodeSettings settings);
}
