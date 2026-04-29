using FluentAssertions;
using LocalWinAI.Infrastructure.Tools;

namespace LocalWinAI.Tests.Tools;

public sealed class FileToolTests : IDisposable
{
    private readonly string _workspace = Path.Combine(Path.GetTempPath(), $"LocalWinAI_Test_{Guid.NewGuid():N}");

    public FileToolTests()
    {
        Directory.CreateDirectory(_workspace);
        Directory.CreateDirectory(Path.Combine(_workspace, "subdir"));
        File.WriteAllText(Path.Combine(_workspace, "hello.txt"), "Hello, world!\nLine two.\nHello again.");
        File.WriteAllText(Path.Combine(_workspace, "subdir", "nested.txt"), "nested content");
    }

    public void Dispose() => Directory.Delete(_workspace, recursive: true);

    // ── ListDirectoryTool ──────────────────────────────────────────────────────

    [Fact]
    public async Task ListDirectory_RootDot_ListsFilesAndDirs()
    {
        var tool = new ListDirectoryTool();

        var result = await tool.ExecuteAsync(_workspace, """{"path":"."}""", default);

        result.Should().Contain("[dir]  subdir");
        result.Should().Contain("[file] hello.txt");
    }

    [Fact]
    public async Task ListDirectory_EmptyArgs_DefaultsToRoot()
    {
        var tool = new ListDirectoryTool();

        var result = await tool.ExecuteAsync(_workspace, "", default);

        result.Should().Contain("[file] hello.txt");
    }

    [Fact]
    public async Task ListDirectory_Subdirectory_ListsNestedFile()
    {
        var tool = new ListDirectoryTool();

        var result = await tool.ExecuteAsync(_workspace, """{"path":"subdir"}""", default);

        result.Should().Contain("[file] nested.txt");
        result.Should().NotContain("hello.txt");
    }

    [Fact]
    public async Task ListDirectory_PathEscapesWorkspace_ReturnsError()
    {
        var tool = new ListDirectoryTool();

        var result = await tool.ExecuteAsync(_workspace, """{"path":"../.."}""", default);

        result.Should().Contain("escapes the workspace root");
    }

    // ── ReadFileTool ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ReadFile_ExistingFile_ReturnsContent()
    {
        var tool = new ReadFileTool();

        var result = await tool.ExecuteAsync(_workspace, """{"path":"hello.txt"}""", default);

        result.Should().Contain("Hello, world!");
        result.Should().Contain("Line two.");
    }

    [Fact]
    public async Task ReadFile_MissingFile_ReturnsNotFound()
    {
        var tool = new ReadFileTool();

        var result = await tool.ExecuteAsync(_workspace, """{"path":"missing.txt"}""", default);

        result.Should().Contain("File not found");
    }

    [Fact]
    public async Task ReadFile_LargeFile_TruncatesWithNotice()
    {
        var bigFile = Path.Combine(_workspace, "big.txt");
        File.WriteAllText(bigFile, new string('x', 40_000));
        var tool = new ReadFileTool();

        var result = await tool.ExecuteAsync(_workspace, """{"path":"big.txt"}""", default);

        result.Should().EndWith("[truncated]");
        result.Length.Should().BeLessThan(40_000);
    }

    [Fact]
    public async Task ReadFile_PathEscapesWorkspace_ReturnsError()
    {
        var tool = new ReadFileTool();

        var result = await tool.ExecuteAsync(_workspace, """{"path":"../../secret.txt"}""", default);

        result.Should().Contain("escapes the workspace root");
    }

    // ── SearchInFileTool ───────────────────────────────────────────────────────

    [Fact]
    public async Task SearchInFile_MatchingQuery_ReturnsMatchingLines()
    {
        var tool = new SearchInFileTool();

        var result = await tool.ExecuteAsync(_workspace, """{"path":"hello.txt","query":"hello"}""", default);

        result.Should().Contain("1: Hello, world!");
        result.Should().Contain("3: Hello again.");
        result.Should().NotContain("Line two.");
    }

    [Fact]
    public async Task SearchInFile_CaseInsensitive_FindsMatch()
    {
        var tool = new SearchInFileTool();

        var result = await tool.ExecuteAsync(_workspace, """{"path":"hello.txt","query":"HELLO"}""", default);

        result.Should().Contain("1: Hello, world!");
    }

    [Fact]
    public async Task SearchInFile_NoMatches_ReturnsNoMatchesMessage()
    {
        var tool = new SearchInFileTool();

        var result = await tool.ExecuteAsync(_workspace, """{"path":"hello.txt","query":"zzznomatch"}""", default);

        result.Should().Contain("No matches found");
    }

    [Fact]
    public async Task SearchInFile_MissingQuery_ReturnsError()
    {
        var tool = new SearchInFileTool();

        var result = await tool.ExecuteAsync(_workspace, """{"path":"hello.txt"}""", default);

        result.Should().Contain("Missing required argument: query");
    }

    [Fact]
    public async Task SearchInFile_PathEscapesWorkspace_ReturnsError()
    {
        var tool = new SearchInFileTool();

        var result = await tool.ExecuteAsync(_workspace, """{"path":"../outside.txt","query":"x"}""", default);

        result.Should().Contain("escapes the workspace root");
    }

    [Fact]
    public async Task SearchInFile_MissingPath_ReturnsError()
    {
        var tool = new SearchInFileTool();

        var result = await tool.ExecuteAsync(_workspace, """{"query":"hello"}""", default);

        result.Should().Contain("Missing required argument: path");
    }
}
