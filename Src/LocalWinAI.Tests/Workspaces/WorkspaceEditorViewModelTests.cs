using FluentAssertions;
using LocalWinAI.Application.Workspaces;
using LocalWinAI.Domain.Workspaces;
using LocalWinAI.Tests.Fakes;

namespace LocalWinAI.Tests.Workspaces;

public class WorkspaceEditorViewModelTests
{
    private static (WorkspaceEditorViewModel editor, FakeWorkspaceRepository repo) CreateEditor(Workspace workspace)
    {
        var repo = new FakeWorkspaceRepository();
        repo.SaveAsync(workspace).Wait();
        var manager = new WorkspaceManager(repo);
        return (new WorkspaceEditorViewModel(workspace, manager), repo);
    }

    [Fact]
    public void Constructor_InitializesFieldsFromWorkspace()
    {
        var workspace = new Workspace
        {
            Name = "Alpha",
            Description = "Desc",
            SystemPrompt = "Prompt",
            AccentColorHex = "#FF0000",
            IconGlyph = ""
        };
        var (editor, _) = CreateEditor(workspace);

        editor.Name.Should().Be("Alpha");
        editor.Description.Should().Be("Desc");
        editor.SystemPrompt.Should().Be("Prompt");
        editor.AccentColorHex.Should().Be("#FF0000");
        editor.IconGlyph.Should().Be("");
        editor.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void Constructor_LoadsFoldersFromWorkspace()
    {
        var workspace = new Workspace();
        workspace.Folders.Add(new WorkspaceFolder(@"C:\Projects\Alpha", "Alpha"));
        workspace.Folders.Add(new WorkspaceFolder(@"C:\Projects\Beta"));
        var (editor, _) = CreateEditor(workspace);

        editor.Folders.Should().HaveCount(2);
        editor.Folders[0].Path.Should().Be(@"C:\Projects\Alpha");
        editor.Folders[0].Alias.Should().Be("Alpha");
        editor.Folders[1].Path.Should().Be(@"C:\Projects\Beta");
        editor.Folders[1].Alias.Should().BeNull();
        editor.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void Name_Changed_SetsDirtyTrue()
    {
        var workspace = new Workspace { Name = "Alpha" };
        var (editor, _) = CreateEditor(workspace);

        editor.Name = "Beta";

        editor.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void Description_Changed_SetsDirtyTrue()
    {
        var workspace = new Workspace { Description = "Old" };
        var (editor, _) = CreateEditor(workspace);

        editor.Description = "New";

        editor.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void SystemPrompt_Changed_SetsDirtyTrue()
    {
        var workspace = new Workspace { SystemPrompt = "Old" };
        var (editor, _) = CreateEditor(workspace);

        editor.SystemPrompt = "New";

        editor.IsDirty.Should().BeTrue();
    }

    [Fact]
    public async Task SaveCommand_PersistsViaManagerAndClearsDirty()
    {
        var workspace = new Workspace { Name = "Alpha" };
        var (editor, repo) = CreateEditor(workspace);
        editor.Name = "Updated";
        editor.SystemPrompt = "My prompt";

        await editor.SaveCommand.ExecuteAsync(null);

        editor.IsDirty.Should().BeFalse();
        var saved = repo[workspace.Id]!;
        saved.Name.Should().Be("Updated");
        saved.SystemPrompt.Should().Be("My prompt");
    }

    [Fact]
    public async Task SaveCommand_PersistsFoldersToWorkspace()
    {
        var workspace = new Workspace();
        var (editor, repo) = CreateEditor(workspace);
        editor.Folders.Add(new WorkspaceFolderViewModel { Path = @"C:\Src", Alias = "src" });
        editor.IsDirty = true;

        await editor.SaveCommand.ExecuteAsync(null);

        var saved = repo[workspace.Id]!;
        saved.Folders.Should().HaveCount(1);
        saved.Folders[0].Path.Should().Be(@"C:\Src");
        saved.Folders[0].Alias.Should().Be("src");
    }

    [Fact]
    public async Task AddFolderCommand_WithRequestFolderStub_AddsFolderAndSetsDirty()
    {
        var workspace = new Workspace();
        var (editor, _) = CreateEditor(workspace);
        editor.RequestFolderAsync = () => Task.FromResult<string?>(@"C:\Projects\MyApp");

        await editor.AddFolderCommand.ExecuteAsync(null);

        editor.Folders.Should().HaveCount(1);
        editor.Folders[0].Path.Should().Be(@"C:\Projects\MyApp");
        editor.IsDirty.Should().BeTrue();
    }

    [Fact]
    public async Task AddFolderCommand_WhenRequestFolderReturnsNull_DoesNotAddFolder()
    {
        var workspace = new Workspace();
        var (editor, _) = CreateEditor(workspace);
        editor.RequestFolderAsync = () => Task.FromResult<string?>(null);

        await editor.AddFolderCommand.ExecuteAsync(null);

        editor.Folders.Should().BeEmpty();
        editor.IsDirty.Should().BeFalse();
    }

    [Fact]
    public async Task AddFolderCommand_WhenNoRequestDelegate_DoesNothing()
    {
        var workspace = new Workspace();
        var (editor, _) = CreateEditor(workspace);

        await editor.AddFolderCommand.ExecuteAsync(null);

        editor.Folders.Should().BeEmpty();
        editor.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void RemoveFolderCommand_RemovesFolderAndSetsDirty()
    {
        var workspace = new Workspace();
        workspace.Folders.Add(new WorkspaceFolder(@"C:\Projects"));
        var (editor, _) = CreateEditor(workspace);
        var folder = editor.Folders[0];

        editor.RemoveFolderCommand.Execute(folder);

        editor.Folders.Should().BeEmpty();
        editor.IsDirty.Should().BeTrue();
    }
}
