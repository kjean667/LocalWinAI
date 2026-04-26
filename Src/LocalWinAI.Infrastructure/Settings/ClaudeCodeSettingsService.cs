using LocalWinAI.Application.Settings;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LocalWinAI.Infrastructure.Settings;

public class ClaudeCodeSettingsService : IClaudeCodeSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string SettingsFilePath { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude", "settings.json");

    public async Task<ClaudeCodeSettings> LoadAsync()
    {
        if (!File.Exists(SettingsFilePath))
            return new ClaudeCodeSettings();

        var json = await File.ReadAllTextAsync(SettingsFilePath);
        if (string.IsNullOrWhiteSpace(json))
            return new ClaudeCodeSettings();

        return JsonSerializer.Deserialize<ClaudeCodeSettings>(json, JsonOptions) ?? new ClaudeCodeSettings();
    }

    public async Task SaveAsync(ClaudeCodeSettings settings)
    {
        var directory = Path.GetDirectoryName(SettingsFilePath)!;
        Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        await File.WriteAllTextAsync(SettingsFilePath, json);
    }
}
