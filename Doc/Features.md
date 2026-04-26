# Features

## Finished features available right now

### On-device Chat
- Send text messages to the local Windows Runtime language model.
- Conversation history is maintained across turns within a session (history is passed as a single joined prompt).
- Streaming-style UX: a spinner appears in the AI message bubble while inference runs.
- **Clear** button resets the conversation and clears all message bubbles.
- Enter key submits the current input message.
- Chat log auto-scrolls to the latest message.

## Planned features

- Streaming token output (update AI bubble text incrementally as tokens arrive).
- Markdown rendering in AI message bubbles.
- Persistent conversation history across app restarts.
- MCP (Model Context Protocol) integration to allow Claude Code to delegate tasks to the local model.
- Support for additional local model capabilities: summarization, classification, embeddings.
