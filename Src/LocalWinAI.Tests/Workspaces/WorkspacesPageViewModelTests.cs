using FluentAssertions;
using LocalWinAI.Application.Workspaces;
using LocalWinAI.Tests.Fakes;

namespace LocalWinAI.Tests.Workspaces;

public class WorkspacesPageViewModelTests
{
    private static (WorkspacesPageViewModel vm, WorkspaceManager manager) CreateVm()
    {
        // Null out the SynchronizationContext so WorkspacesChanged fires RebuildWorkspaces
        // synchronously and async operations on already-completed tasks run inline.
        var prev = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(null);
        try
        {
            var repo = new FakeWorkspaceRepository();
            var manager = new WorkspaceManager(repo);
            return (new WorkspacesPageViewModel(manager), manager);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(prev);
        }
    }

    [Fact]
    public void Constructor_WhenNoWorkspaces_IsEmptyTrue()
    {
        var (vm, _) = CreateVm();

        vm.Workspaces.Should().BeEmpty();
        vm.IsEmpty.Should().BeTrue();
        vm.SelectedWorkspace.Should().BeNull();
        vm.Editor.Should().BeNull();
    }

    [Fact]
    public async Task NewWorkspaceCommand_CreatesWorkspaceAndSelectsIt()
    {
        var (vm, _) = CreateVm();

        await vm.NewWorkspaceCommand.ExecuteAsync(null);

        vm.Workspaces.Should().HaveCount(1);
        vm.SelectedWorkspace.Should().NotBeNull();
        vm.Editor.Should().NotBeNull();
        vm.Editor!.Name.Should().Be("Untitled workspace");
        vm.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public async Task NewWorkspaceCommand_EditorStartsClean()
    {
        var (vm, _) = CreateVm();

        await vm.NewWorkspaceCommand.ExecuteAsync(null);

        vm.Editor!.IsDirty.Should().BeFalse();
    }

    [Fact]
    public async Task NewWorkspaceCommand_MultipleWorkspaces_EachAppearsInList()
    {
        var (vm, _) = CreateVm();

        await vm.NewWorkspaceCommand.ExecuteAsync(null);
        await vm.NewWorkspaceCommand.ExecuteAsync(null);

        vm.Workspaces.Should().HaveCount(2);
        vm.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public async Task WorkspacesChanged_ExternalCreate_RebuildsWorkspacesList()
    {
        var (vm, manager) = CreateVm();

        await manager.CreateAsync("External");

        vm.Workspaces.Should().HaveCount(1);
        vm.Workspaces[0].Name.Should().Be("External");
        vm.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteWorkspaceCommand_RemovesWorkspaceFromList()
    {
        var (vm, _) = CreateVm();
        await vm.NewWorkspaceCommand.ExecuteAsync(null);
        var id = vm.SelectedWorkspace!.Id;

        await vm.DeleteWorkspaceCommand.ExecuteAsync(id);

        vm.Workspaces.Should().BeEmpty();
        vm.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteWorkspaceCommand_WhenSelectedIsDeleted_ClearsEditor()
    {
        var (vm, _) = CreateVm();
        await vm.NewWorkspaceCommand.ExecuteAsync(null);
        var id = vm.SelectedWorkspace!.Id;

        await vm.DeleteWorkspaceCommand.ExecuteAsync(id);

        vm.SelectedWorkspace.Should().BeNull();
        vm.Editor.Should().BeNull();
    }

    [Fact]
    public async Task DeleteWorkspaceCommand_WhenNonSelectedIsDeleted_KeepsEditor()
    {
        var (vm, manager) = CreateVm();
        await vm.NewWorkspaceCommand.ExecuteAsync(null);
        var selectedId = vm.SelectedWorkspace!.Id;
        var otherWorkspace = await manager.CreateAsync("Other");

        await vm.DeleteWorkspaceCommand.ExecuteAsync(otherWorkspace.Id);

        vm.SelectedWorkspace.Should().NotBeNull();
        vm.SelectedWorkspace!.Id.Should().Be(selectedId);
        vm.Editor.Should().NotBeNull();
    }

    [Fact]
    public async Task SelectedWorkspace_Set_LoadsEditor()
    {
        var (vm, manager) = CreateVm();
        var workspace = await manager.CreateAsync("Alpha");

        vm.SelectedWorkspace = vm.Workspaces.First(w => w.Id == workspace.Id);

        vm.Editor.Should().NotBeNull();
        vm.Editor!.Name.Should().Be("Alpha");
    }

    [Fact]
    public async Task SelectedWorkspace_SetToNull_ClearsEditor()
    {
        var (vm, _) = CreateVm();
        await vm.NewWorkspaceCommand.ExecuteAsync(null);

        vm.SelectedWorkspace = null;

        vm.Editor.Should().BeNull();
    }

    [Fact]
    public async Task Dispose_UnsubscribesFromWorkspacesChanged()
    {
        var (vm, manager) = CreateVm();
        vm.Dispose();

        await manager.CreateAsync("After dispose");

        // No exception and list is still empty (event no longer handled).
        vm.Workspaces.Should().BeEmpty();
    }
}
