using FluentAssertions;
using LocalWinAI.Application.Sessions;
using LocalWinAI.Domain.Sessions;
using LocalWinAI.Tests.Fakes;

namespace LocalWinAI.Tests.Sessions;

public class ChatSessionManagerTests
{
    private static (ChatSessionManager manager, FakeChatSessionRepository repo, FakeLanguageModelService lm) Create()
    {
        var repo = new FakeChatSessionRepository();
        var lm = new FakeLanguageModelService();
        return (new ChatSessionManager(repo, lm), repo, lm);
    }

    [Fact]
    public async Task InitializeAsync_NoExistingSessions_CreatesNewSession()
    {
        var (manager, repo, _) = Create();

        await manager.InitializeAsync();

        manager.ActiveSession.Should().NotBeNull();
        repo.Count.Should().Be(1);
    }

    [Fact]
    public async Task InitializeAsync_ExistingSessions_LoadsMostRecent()
    {
        var (manager, repo, _) = Create();
        var old = new ChatSession { LastUsedAt = DateTimeOffset.UtcNow.AddDays(-1) };
        var recent = new ChatSession { LastUsedAt = DateTimeOffset.UtcNow };
        old.Messages.Add(new ChatMessage(Domain.ChatMessageSender.User, "old", DateTimeOffset.UtcNow));
        recent.Messages.Add(new ChatMessage(Domain.ChatMessageSender.User, "recent", DateTimeOffset.UtcNow));
        await repo.SaveAsync(old);
        await repo.SaveAsync(recent);

        await manager.InitializeAsync();

        manager.ActiveSession.Id.Should().Be(recent.Id);
        manager.ActiveSession.Messages.Should().HaveCount(1);
        manager.ActiveSession.Messages[0].Text.Should().Be("recent");
    }

    [Fact]
    public async Task CreateSessionAsync_AddsNewSessionToRepository()
    {
        var (manager, repo, _) = Create();
        await manager.InitializeAsync();

        await manager.CreateSessionAsync();

        repo.Count.Should().Be(2);
        manager.ActiveSession.Title.Should().Be("New conversation");
    }

    [Fact]
    public async Task SwitchToSessionAsync_ChangesActiveSession()
    {
        var (manager, repo, _) = Create();
        await manager.InitializeAsync();
        var firstId = manager.ActiveSession.Id;

        var second = new ChatSession();
        second.Messages.Add(new ChatMessage(Domain.ChatMessageSender.User, "hello", DateTimeOffset.UtcNow));
        await repo.SaveAsync(second);

        await manager.SwitchToSessionAsync(second.Id);

        manager.ActiveSession.Id.Should().Be(second.Id);
        manager.ActiveSession.Messages.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteSessionAsync_ActiveSession_SwitchesToRemaining()
    {
        var (manager, repo, _) = Create();
        await manager.InitializeAsync();
        var first = manager.ActiveSession;

        await manager.CreateSessionAsync();
        var secondId = manager.ActiveSession.Id;

        await manager.DeleteSessionAsync(secondId);

        manager.ActiveSession.Id.Should().Be(first.Id);
        repo.Count.Should().Be(1);
    }

    [Fact]
    public async Task DeleteSessionAsync_LastSession_CreatesNewSession()
    {
        var (manager, repo, _) = Create();
        await manager.InitializeAsync();
        var id = manager.ActiveSession.Id;

        await manager.DeleteSessionAsync(id);

        manager.ActiveSession.Should().NotBeNull();
        manager.ActiveSession.Id.Should().NotBe(id);
        repo.Count.Should().Be(1);
    }

    [Fact]
    public async Task PersistActiveSessionAsync_UpdatesLastUsedAt()
    {
        var (manager, repo, _) = Create();
        await manager.InitializeAsync();
        var before = manager.ActiveSession.LastUsedAt;
        await Task.Delay(10);

        await manager.PersistActiveSessionAsync();

        var saved = repo[manager.ActiveSession.Id];
        saved!.LastUsedAt.Should().BeAfter(before);
    }

    [Fact]
    public async Task GetSessionsAsync_ReturnsAllSessionMetadata()
    {
        var (manager, _, _) = Create();
        await manager.InitializeAsync();
        await manager.CreateSessionAsync();

        var sessions = await manager.GetSessionsAsync();

        sessions.Should().HaveCount(2);
    }
}
