# Features

## Finished features available right now

### Usage Statistics
- A dedicated **Statistics** page in the navigation shows how much work the local NPU has done.
- Tracks every chat turn and every MCP tool call: token estimates, call counts, and duration.
- Displays a **hero card** with cumulative token count and an estimated cost saving vs. cloud APIs (calculated at approximate reference rates — $3 / 1M input tokens, $15 / 1M output tokens).
- Shows a **tool breakdown list** (Chat UI, Infer, Summarize, Classify) with all-time and today call counts.
- Shows a **7-day activity bar chart**.
- Shows a **streak badge** when the local model has been used on consecutive days.
- Usage is persisted to `%LOCALAPPDATA%\LocalWinAI\usage.ndjson` — an append-only newline-delimited JSON log shared between the GUI process and any MCP server processes.
- The statistics page **updates in near real-time** when external tools (e.g. Claude Code via MCP) use the local model: a `FileSystemWatcher` detects new data within ~200 ms, with a 5-second polling fallback. A **Refresh** button is also available.
- The log is automatically compacted on startup — events older than 90 days are dropped.

### On-device Chat
- Send text messages to the local Windows Runtime language model.
- Conversation history is maintained across turns within a session (history is passed as a single joined prompt).
- Streaming-style UX: a spinner appears in the AI message bubble while inference runs.
- **Clear** button resets the conversation and clears all message bubbles.
- Enter key submits the current input message.
- Chat log auto-scrolls to the latest message.

### MCP Server (Claude Code Integration)
LocalWinAI can run as a local MCP tool provider for Claude Code. Launch it with the `--mcp` flag and Claude Code will delegate small tasks to the local NPU instead of consuming cloud API tokens.

**Start in MCP mode:**
```
LocalWinAI.exe --mcp
```

**Add to your Claude Code settings** (`~/.claude/settings.json`):
```json
{
  "mcpServers": {
    "localwinai": {
      "command": "C:\\Path\\To\\LocalWinAI.exe",
      "args": ["--mcp"]
    }
  }
}
```

**Available MCP tools:**

| Tool | Description |
|---|---|
| `local_infer` | Run a prompt through the local NPU-accelerated model |
| `local_summarize` | Summarize text locally |
| `local_classify` | Classify text into provided categories |
| `local_embed` | Generate embeddings — not yet supported |

## Planned features

- Streaming token output (update AI bubble text incrementally as tokens arrive).
- Markdown rendering in AI message bubbles.
- Persistent conversation history across app restarts.
- Embedding support for the `local_embed` MCP tool (requires a dedicated embedding model).
