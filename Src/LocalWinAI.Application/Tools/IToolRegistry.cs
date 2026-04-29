namespace LocalWinAI.Application.Tools;

/// <summary>Provides name-based lookup and enumeration of all registered file tools.</summary>
public interface IToolRegistry
{
    /// <summary>Looks up a tool by its <see cref="IFileTool.Name"/>.</summary>
    bool TryGetTool(string name, out IFileTool tool);

    /// <summary>Returns all registered tools.</summary>
    IEnumerable<IFileTool> GetAll();
}
