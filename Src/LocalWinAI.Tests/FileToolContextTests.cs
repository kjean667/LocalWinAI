using FluentAssertions;
using LocalWinAI.Application.Tools;
using LocalWinAI.Domain.Workspaces;

namespace LocalWinAI.Tests;

public class FileToolContextTests
{
    private static readonly string Root = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);

    // ── Single-folder ──────────────────────────────────────────────────────────

    [Fact]
    public void TryResolveAbsolute_SingleFolder_NullAlias_ResolvesToFolder()
    {
        var ctx = new FileToolContext([new WorkspaceFolder(Root)]);

        var ok = ctx.TryResolveAbsolute(null, ".", out var abs, out var err);

        ok.Should().BeTrue();
        abs.Should().Be(Path.GetFullPath(Root));
        err.Should().BeNull();
    }

    [Fact]
    public void TryResolveAbsolute_SingleFolder_EmptyAlias_ResolvesToFolder()
    {
        var ctx = new FileToolContext([new WorkspaceFolder(Root)]);

        var ok = ctx.TryResolveAbsolute(string.Empty, ".", out var abs, out _);

        ok.Should().BeTrue();
        abs.Should().Be(Path.GetFullPath(Root));
    }

    [Fact]
    public void TryResolveAbsolute_SingleFolder_MatchesByAlias()
    {
        var ctx = new FileToolContext([new WorkspaceFolder(Root, "src")]);

        var ok = ctx.TryResolveAbsolute("src", ".", out var abs, out _);

        ok.Should().BeTrue();
        abs.Should().Be(Path.GetFullPath(Root));
    }

    [Fact]
    public void TryResolveAbsolute_SingleFolder_MatchesByFolderName()
    {
        var folderName = Path.GetFileName(Root);
        var ctx = new FileToolContext([new WorkspaceFolder(Root)]);

        var ok = ctx.TryResolveAbsolute(folderName, ".", out var abs, out _);

        ok.Should().BeTrue();
        abs.Should().Be(Path.GetFullPath(Root));
    }

    // ── Multi-folder ───────────────────────────────────────────────────────────

    [Fact]
    public void TryResolveAbsolute_MultiFolderNullAlias_ReturnsError()
    {
        var ctx = new FileToolContext([new WorkspaceFolder(Root, "src"), new WorkspaceFolder(Root, "docs")]);

        var ok = ctx.TryResolveAbsolute(null, ".", out _, out var err);

        ok.Should().BeFalse();
        err.Should().Contain("missing 'folder'");
        err.Should().Contain("src");
        err.Should().Contain("docs");
    }

    [Fact]
    public void TryResolveAbsolute_MultiFolderUnknownAlias_ReturnsError()
    {
        var ctx = new FileToolContext([new WorkspaceFolder(Root, "src"), new WorkspaceFolder(Root, "docs")]);

        var ok = ctx.TryResolveAbsolute("tests", ".", out _, out var err);

        ok.Should().BeFalse();
        err.Should().Contain("unknown folder 'tests'");
        err.Should().Contain("src");
        err.Should().Contain("docs");
    }

    [Fact]
    public void TryResolveAbsolute_MultiFolderAliasTakesPrecedenceOverFolderName()
    {
        // Two folders: one with alias "other", one whose basename equals "other"
        var root2 = Path.Combine(Root, "other");
        var ctx = new FileToolContext([
            new WorkspaceFolder(Root, "other"),   // alias wins
            new WorkspaceFolder(root2)             // basename matches but alias check comes first
        ]);

        var ok = ctx.TryResolveAbsolute("other", ".", out var abs, out _);

        ok.Should().BeTrue();
        abs.Should().Be(Path.GetFullPath(Root)); // resolved to the first folder (by alias)
    }

    // ── Sandbox enforcement ────────────────────────────────────────────────────

    [Fact]
    public void TryResolveAbsolute_PathTraversal_ReturnsError()
    {
        var ctx = new FileToolContext([new WorkspaceFolder(Root)]);

        var ok = ctx.TryResolveAbsolute(null, "../escape", out _, out var err);

        ok.Should().BeFalse();
        err.Should().Contain("escapes the folder root");
    }

    [Fact]
    public void TryResolveAbsolute_ValidRelativePath_Resolves()
    {
        var ctx = new FileToolContext([new WorkspaceFolder(Root)]);

        var ok = ctx.TryResolveAbsolute(null, "subdir/file.txt", out var abs, out _);

        ok.Should().BeTrue();
        abs.Should().Be(Path.GetFullPath(Path.Combine(Root, "subdir", "file.txt")));
    }

    // ── Constructor guard ──────────────────────────────────────────────────────

    [Fact]
    public void Constructor_EmptyFolders_Throws()
    {
        var act = () => new FileToolContext([]);

        act.Should().Throw<ArgumentException>();
    }
}
