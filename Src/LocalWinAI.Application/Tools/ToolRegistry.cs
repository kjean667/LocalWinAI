namespace LocalWinAI.Application.Tools;

/// <summary>
/// Singleton registry built from all <see cref="IFileTool"/> instances collected by DI.
/// Throws at construction time if any two tools share the same name.
/// </summary>
public sealed class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, IFileTool> _tools;

    public ToolRegistry(IEnumerable<IFileTool> tools)
    {
        _tools = new Dictionary<string, IFileTool>(StringComparer.Ordinal);
        foreach (var tool in tools)
        {
            if (!_tools.TryAdd(tool.Name, tool))
                throw new InvalidOperationException($"Duplicate tool name registered: '{tool.Name}'.");
        }
    }

    public bool TryGetTool(string name, out IFileTool tool) =>
        _tools.TryGetValue(name, out tool!);

    public IEnumerable<IFileTool> GetAll() => _tools.Values;
}
