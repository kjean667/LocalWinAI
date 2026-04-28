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
LocalWinAI exposes four MCP tools so that AI agents can delegate small tasks to the local NPU instead of consuming cloud API tokens. Two executables work together:

- **`LocalWinAI.exe`** must be running. It serves inference and embedding requests over a named pipe.
- **`LocalWinAI.McpHost.exe`** is started by AI Agents as the MCP stdio server. It forwards each tool call to the running app via the pipe.

**Register via the Settings page (recommended):**
Open the app, navigate to **Settings**, enable the *LocalWinAI* toggle, and click **Save**. The app writes the correct entry to `~/.mcp.json` automatically.

**Or add manually** (`~/.mcp.json`):
```json
{
  "mcpServers": {
    "localwinai": {
      "command": "C:\\Path\\To\\LocalWinAI.McpHost.exe",
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
| `local_embed` | Generate a semantic embedding vector using the local NPU |

### Settings Page
A dedicated **Settings** page in the navigation lets you manage Claude Code integration without editing JSON by hand. Toggle the *LocalWinAI* switch on or off and click **Save** — the app reads and writes `~/.mcp.json`, adding or removing the `localwinai` MCP server entry. The path to the settings file is shown on the page for reference.

## Planned features

- Workspace-aware file reasoning.
- Markdown rendering in AI message bubbles.
- Persistent conversation history across app restarts.
