using System.Text.Json;
using System.Text.Json.Serialization;

namespace LocalWinAI.Application.Settings;

public class ClaudeCodeSettings
{
    [JsonPropertyName("mcpServers")]
    public Dictionary<string, McpServerEntry> McpServers { get; set; } = [];

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public class McpServerEntry
{
    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;

    [JsonPropertyName("args")]
    public List<string> Args { get; set; } = [];

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
