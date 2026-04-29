namespace LocalWinAI.Application.Tools;

public interface IFileTool
{
    string Name { get; }
    string Description { get; }
    Task<string> ExecuteAsync(string workspaceRoot, string argumentsJson, CancellationToken cancellationToken);
}
