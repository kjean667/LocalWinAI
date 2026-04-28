using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalWinAI.Application.Sessions;

/// <summary>Observable list-item model for the session sidebar.</summary>
public partial class ChatSessionSummaryViewModel : ObservableObject
{
    public Guid Id { get; init; }

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    public DateTimeOffset LastUsedAt { get; init; }

    public string RelativeDateText
    {
        get
        {
            var today = DateTime.UtcNow.Date;
            var lastUsed = LastUsedAt.UtcDateTime.Date;
            return (today - lastUsed).Days switch
            {
                0 => "Today",
                1 => "Yesterday",
                var d => $"{d} days ago"
            };
        }
    }
}
