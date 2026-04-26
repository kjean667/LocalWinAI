namespace LocalWinAI.Application;

/// <summary>Manages a chat conversation with the local language model.</summary>
public interface IChatService
{
    /// <summary>Sends a user message and returns the AI-generated response.</summary>
    Task<string> SendMessageAsync(string userMessage, CancellationToken cancellationToken = default);

    /// <summary>Clears all conversation history.</summary>
    void ClearConversation();
}
