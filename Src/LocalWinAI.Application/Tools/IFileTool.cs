namespace LocalWinAI.Application.Tools;

public interface IFileTool
{
    string Name { get; }
    string Description { get; }
    Task<string> ExecuteAsync(IFileToolContext context, string argumentsJson, CancellationToken cancellationToken);
}
