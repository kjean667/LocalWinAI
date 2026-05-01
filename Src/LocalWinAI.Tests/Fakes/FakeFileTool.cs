using LocalWinAI.Application.Tools;

namespace LocalWinAI.Tests.Fakes;

public sealed class FakeFileTool : IFileTool
{
    public string Name { get; }
    public string Description => "Fake tool";
    public string Result { get; set; } = "tool result";
    public int CallCount { get; private set; }

    public FakeFileTool(string name) => Name = name;

    public Task<string> ExecuteAsync(IFileToolContext context, string argumentsJson, CancellationToken cancellationToken)
    {
        CallCount++;
        return Task.FromResult(Result);
    }
}
