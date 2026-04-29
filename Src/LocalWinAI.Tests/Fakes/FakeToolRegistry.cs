using LocalWinAI.Application.Tools;

namespace LocalWinAI.Tests.Fakes;

public sealed class FakeToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, IFileTool> _tools = new(StringComparer.Ordinal);

    public void Register(IFileTool tool) => _tools[tool.Name] = tool;

    public bool TryGetTool(string name, out IFileTool tool) =>
        _tools.TryGetValue(name, out tool!);

    public IEnumerable<IFileTool> GetAll() => _tools.Values;
}
