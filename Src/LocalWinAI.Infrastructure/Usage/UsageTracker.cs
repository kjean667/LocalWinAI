using System.Text;
using System.Text.Json;
using LocalWinAI.Domain.Usage;

namespace LocalWinAI.Infrastructure.Usage;

public sealed class UsageTracker : IUsageTracker
{
    internal static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LocalWinAI",
        "usage.ndjson");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task RecordAsync(UsageEvent evt)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

        var json = JsonSerializer.Serialize(evt, JsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json + "\n");

        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await using var stream = new FileStream(
                    FilePath, FileMode.Append, FileAccess.Write,
                    FileShare.ReadWrite, bufferSize: 4096, useAsync: true);
                await stream.WriteAsync(bytes);
                return;
            }
            catch (IOException) when (attempt < 2)
            {
                await Task.Delay(50);
            }
            catch (IOException) { }
        }
    }
}
