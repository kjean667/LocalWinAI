using FluentAssertions;
using LocalWinAI.Application.Workspaces;
using LocalWinAI.Domain.Workspaces;
using LocalWinAI.Tests.Fakes;

namespace LocalWinAI.Tests.Workspaces;

public class WorkspaceManagerTests
{
    private static (WorkspaceManager manager, FakeWorkspaceRepository repo) Create()
    {
        var repo = new FakeWorkspaceRepository();
        return (new WorkspaceManager(repo), repo);
    }

    [Fact]
    public async Task LoadAsync_PopulatesSummariesFromRepository()
    {
        var (manager, repo) = Create();
        var ws = new Workspace { Name = "Alpha" };
        await repo.SaveAsync(ws);

        await manager.LoadAsync();

        manager.Summaries.Should().HaveCount(1);
        manager.Summaries[0].Id.Should().Be(ws.Id);
        manager.Summaries[0].Name.Should().Be("Alpha");
    }

    [Fact]
    public async Task CreateAsync_AddsSummaryPersistsAndRaisesEventOnce()
    {
        var (manager, repo) = Create();
        int eventCount = 0;
        manager.WorkspacesChanged += (_, _) => eventCount++;

        var workspace = await manager.CreateAsync("Beta");

        manager.Summaries.Should().HaveCount(1);
        manager.Summaries[0].Id.Should().Be(workspace.Id);
        repo.Count.Should().Be(1);
        eventCount.Should().Be(1);
    }

    [Fact]
    public async Task SaveAsync_ExistingWorkspace_UpdatesSummaryAndRaisesEventOnce()
    {
        var (manager, _) = Create();
        var workspace = await manager.CreateAsync("Gamma");
        int eventCount = 0;
        manager.WorkspacesChanged += (_, _) => eventCount++;

        workspace.Name = "Gamma Updated";
        await manager.SaveAsync(workspace);

        manager.Summaries.Should().HaveCount(1);
        manager.Summaries[0].Name.Should().Be("Gamma Updated");
        eventCount.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_RemovesSummaryAndRaisesEventOnce()
    {
        var (manager, repo) = Create();
        var workspace = await manager.CreateAsync("Delta");
        int eventCount = 0;
        manager.WorkspacesChanged += (_, _) => eventCount++;

        await manager.DeleteAsync(workspace.Id);

        manager.Summaries.Should().BeEmpty();
        repo.Count.Should().Be(0);
        eventCount.Should().Be(1);
    }

    [Fact]
    public async Task Summaries_OrderedByUpdatedAtDescending()
    {
        var (manager, repo) = Create();
        var older = new Workspace { Name = "Old", UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1) };
        var newer = new Workspace { Name = "New", UpdatedAt = DateTimeOffset.UtcNow };
        await repo.SaveAsync(older);
        await repo.SaveAsync(newer);

        await manager.LoadAsync();

        manager.Summaries[0].Name.Should().Be("New");
        manager.Summaries[1].Name.Should().Be("Old");
    }

    [Fact]
    public async Task SaveAsync_BumpsUpdatedAtSoNewestAppearsFirst()
    {
        var (manager, _) = Create();
        var first = await manager.CreateAsync("First");
        await Task.Delay(5);
        var second = await manager.CreateAsync("Second");
        await Task.Delay(5);

        first.Name = "First Updated";
        await manager.SaveAsync(first);

        manager.Summaries[0].Id.Should().Be(first.Id);
    }

    [Fact]
    public async Task GetAsync_DelegatesToRepository()
    {
        var (manager, repo) = Create();
        var workspace = new Workspace { Name = "Epsilon" };
        await repo.SaveAsync(workspace);

        var result = await manager.GetAsync(workspace.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(workspace.Id);
    }

    [Fact]
    public async Task GetAsync_UnknownId_ReturnsNull()
    {
        var (manager, _) = Create();

        var result = await manager.GetAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task InMemoryList_MirrorsRepositoryAfterEachOperation()
    {
        var (manager, repo) = Create();

        var ws = await manager.CreateAsync("Zeta");
        manager.Summaries.Should().HaveCount(1);
        repo.Count.Should().Be(1);

        ws.Name = "Zeta Renamed";
        await manager.SaveAsync(ws);
        manager.Summaries.Should().HaveCount(1);
        manager.Summaries[0].Name.Should().Be("Zeta Renamed");
        repo.Count.Should().Be(1);

        await manager.DeleteAsync(ws.Id);
        manager.Summaries.Should().BeEmpty();
        repo.Count.Should().Be(0);
    }
}
