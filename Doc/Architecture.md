# LocalWinAI Architecture

This document describes the current implemented architecture in `Src/`.

## Solution and Project Structure

```
Src/
  LocalWinAI.Domain/          — Core abstractions and domain types (net10.0-windows10.0.19041.0)
  LocalWinAI.Application/     — Business logic, ViewModels, DI registration (net10.0-windows10.0.19041.0)
  LocalWinAI.Infrastructure/  — Windows AI integration and named pipe server (net10.0-windows10.0.19041.0)
  LocalWinAI.Mcp/             — MCP tool implementations (net10.0-windows10.0.19041.0)
  LocalWinAI.McpHost/         — MCP stdio host executable; bridges AI agents to the pipe server (net10.0)
  App/                        — WinUI 3 views and composition root (net10.0-windows10.0.19041.0)
  LocalWinAI.Tests/           — xUnit unit tests (net10.0-windows10.0.19041.0)
```

The solution file is `LocalWinAI.slnx` at the repository root.

## Layered Architecture

```
┌──────────────────────────────────┐   ┌──────────────────────────────────┐
│  App  (WinUI 3)                  │   │  LocalWinAI.McpHost              │
│  GUI + named pipe server         │   │  MCP stdio host                  │
│                                  │   │  PipeLanguageModelService        │
└──────────────┬────────────────────┘   └──────────────┬───────────────────┘
               │       named pipe IPC                   │
               │◄───────────────────────────────────────┘
               │                                        │
               ▼                              ▼ (hosts LocalWinAI.Mcp tools)
┌──────────────────────────────────────────────────────────────────────┐
│   Application                                                        │
│  IChatService / ChatService          IUsageAggregateService          │
│  IChatSessionManager / ChatSessionManager                            │
│  ChatPageViewModel                   StatisticsPageViewModel         │
│  ObservableChatMessage               ChatSessionSummaryViewModel     │
│  UsageAggregates / ToolStats         DayStats / ToolStatRow          │
└──────────────────────────────┬───────────────────────────────────────┘
                               │
                               ▼
               ┌──────────────────────────────────────┐
               │          Infrastructure               │
               │  WindowsLanguageModelService          │
               │  NamedPipeInferenceServer             │
               │  ClaudeCodeSettingsService            │
               │  ChatSessionRepository (→ sessions/)  │
               │  UsageTracker (→ usage.ndjson)        │
               │  UsageAggregateService (FileWatcher)  │
               └──────────────┬───────────────────────┘
                              │
                              ▼
               ┌────────────────────────────────┐
               │           Domain               │
               │  ChatMessageSender             │
               │  ChatSession / ChatMessage     │
               │  IChatSessionRepository        │
               │  ILanguageModelService         │
               │  IUsageTracker / UsageEvent    │
               └────────────────────────────────┘
```

**Dependency rule:** arrows flow inward only. `App` and `LocalWinAI.McpHost` are driving adapters at the same level — they depend on Application/Infrastructure/Domain but never on each other. `LocalWinAI.Mcp` contains the MCP tool implementations and is loaded by `McpHost`.

## Startup Modes

There are two executables:

| Executable | Mode |
|---|---|
| `LocalWinAI.exe` | WinUI 3 GUI — launches the chat window and starts the named pipe inference server |
| `LocalWinAI.McpHost.exe --mcp` | MCP host — stdio transport, no UI; forwards inference requests to LocalWinAI.exe via named pipe |

`LocalWinAI.exe` always starts in GUI mode. On startup it initializes `NamedPipeInferenceServer` as a background service on the named pipe `LocalWinAI-Inference`. This pipe server must be reachable for any MCP tool call to succeed — the app must be running.

`LocalWinAI.McpHost.exe` is a separate, non-packaged executable. It registers `PipeLanguageModelService` as `ILanguageModelService`, which sends each inference or embedding request to the running `LocalWinAI.exe` over the named pipe and returns the result. Both executables register `IUsageTracker`. Every tool call and chat turn appends a record to the shared log at `%LOCALAPPDATA%\LocalWinAI\usage.ndjson`, so the GUI's Statistics page captures usage from both sources.

## MCP Server

`LocalWinAI.Mcp` contains MCP tool implementations using the official `ModelContextProtocol` 1.x .NET SDK. These tools are hosted by `LocalWinAI.McpHost.exe`, which acts as the stdio MCP server process for MCP clients such as Claude Code.

| Tool | Method | Description |
|---|---|---|
| `local_infer` | `LocalInferTool.InferAsync` | Run a prompt through the local model |
| `local_summarize` | `LocalSummarizeTool.SummarizeAsync` | Summarize text |
| `local_classify` | `LocalClassifyTool.ClassifyAsync` | Classify text into provided categories |
| `local_embed` | `LocalEmbedTool.EmbedAsync` | Generate a semantic embedding vector |

All tools inject `ILanguageModelService` and `IUsageTracker` from DI. In `McpHost`, `ILanguageModelService` is satisfied by `PipeLanguageModelService`, which forwards calls over the named pipe to the running `LocalWinAI.exe`. Every successful invocation records a `UsageEvent` to the shared log.

### Named Pipe Protocol

`PipeLanguageModelService` opens a new `NamedPipeClientStream` connection per request to the pipe `LocalWinAI-Inference`. Messages are newline-delimited JSON using camelCase property names.

| `PipeRequest.Method` | Payload | Response |
|---|---|---|
| `ping` | — | `PipeResponse.Result = "pong"` |
| `infer` | `Prompt` | `PipeResponse.Result` = generated text |
| `embed` | `Prompt` | `PipeResponse.Embedding` = `float[]` vector |

Timeouts: connect = 5 s, inference/embed = 120 s.

## Domain Model

| Type | Layer | Purpose |
|---|---|---|
| `ChatMessageSender` | Domain | Enum: User or AI |
| `ChatMessage` | Domain | Immutable record of a single message: Sender, Text, Timestamp |
| `ChatSession` | Domain | Aggregate root: Id, Title, CreatedAt, LastUsedAt, Messages |
| `IChatSessionRepository` | Domain | Persistence contract for chat sessions |
| `ILanguageModelService` | Domain | Interface for on-device text generation |
| `IUsageTracker` | Domain | Write interface: append a `UsageEvent` to the shared log |
| `UsageEvent` | Domain | Per-call record: source, tool, estimated tokens, duration |
| `ToolCallParser` | Application | Static parser: extracts `ToolCall` records from model output containing `<tool_call>` blocks |
| `ToolResultFormatter` | Application | Static formatter: wraps tool results in `<tool_result>` blocks for model consumption |
| `ToolCall` | Application | Immutable record: Name and ArgumentsJson extracted from a single tool-call block |
| `IChatService` | Application | Interface for conversation management |
| `ChatService` | Application | Builds prompts from session history, delegates to model, persists after each exchange |
| `IChatSessionManager` | Application | Orchestrates active session: create, switch, delete, persist, title generation |
| `ChatSessionManager` | Application | Implements `IChatSessionManager`; fires background title generation after first exchange |
| `ObservableChatMessage` | Application | UI-bindable message with mutable Text and IsWaiting |
| `ChatSessionSummaryViewModel` | Application | Observable sidebar item: Id, Title (observable), RelativeDateText |
| `ChatPageViewModel` | Application | MVVM ViewModel for the chat page including session sidebar |
| `IUsageAggregateService` | Application | Read interface: aggregated stats + `AggregatesChanged` event |
| `UsageAggregates` | Application | Computed totals, per-tool breakdown, 7-day activity, cost estimate |
| `StatisticsPageViewModel` | Application | MVVM ViewModel for the statistics page |
| `WindowsLanguageModelService` | Infrastructure | Windows Copilot Runtime implementation of `ILanguageModelService` |
| `NamedPipeInferenceServer` | Infrastructure | `IHostedService` that listens on the `LocalWinAI-Inference` named pipe |
| `ChatSessionRepository` | Infrastructure | File-based `IChatSessionRepository`; stores sessions under `%LOCALAPPDATA%\LocalWinAI\sessions\` |
| `PipeConstants` | Infrastructure | Shared pipe name and timeout constants |
| `PipeRequest` / `PipeResponse` | Infrastructure | JSON record types for the pipe protocol |
| `UsageTracker` | Infrastructure | Appends NDJSON events to `%LOCALAPPDATA%\LocalWinAI\usage.ndjson` |
| `UsageAggregateService` | Infrastructure | Reads and aggregates the log; watches for changes; 90-day compaction |
| `LocalInferTool` | Mcp | MCP tool: raw inference |
| `LocalSummarizeTool` | Mcp | MCP tool: summarization via prompted inference |
| `LocalClassifyTool` | Mcp | MCP tool: text classification via prompted inference |
| `LocalEmbedTool` | Mcp | MCP tool: generate a semantic embedding vector |
| `PipeLanguageModelService` | McpHost | `ILanguageModelService` implementation that forwards requests over the named pipe |

## Session Persistence Layout

```
%LocalAppData%\LocalWinAI\sessions\
  index.json           ← [{Id, Title, LastUsedAt, CreatedAt}, ...]  (sidebar metadata)
  {guid1}.json         ← Full ChatSession with all messages
  {guid2}.json
  ...
```

`GetAllAsync` reads only `index.json` (fast sidebar load). `GetAsync(id)` reads the full `{id}.json` file. `SaveAsync` updates both the session file and `index.json`.

## MCP Host Architecture Rationale

Two platform constraints force the two-process design:

- **McpHost cannot access the local model**: `LocalWinAI.McpHost.exe` is a plain .NET console app, not an AppX package. Windows Copilot Runtime (`ILanguageModel`) requires the Limited Access Feature (LAF), which is only granted to packaged AppX applications. The McpHost therefore cannot call the model directly.

- **The main app cannot act as an MCP host**: `LocalWinAI.exe` is a WinUI 3 AppX package. Packaged apps do not have access to `stdin`, so the stdio transport required by the MCP protocol is unavailable.

The named pipe bridges the gap: `LocalWinAI.exe` runs a `NamedPipeInferenceServer` in the background at all times. `LocalWinAI.McpHost.exe` connects to the pipe per request, sends a JSON command (`infer` or `embed`), and returns the result to the MCP client via stdio.

## Development Setup

Requirements:
- Windows 11 24H2 (Build 26100) or later with Copilot+ feature enabled
- Visual Studio 2026 with WinUI / Windows App SDK workload
- .NET 10 SDK

The `Microsoft.WindowsAppSDK` package is used for building the WinUI 3 application and the NPU integration.

## Dependency Boundary Rules

- **Domain** has no external project references.
- **Application** references Domain only. It must not reference Infrastructure or WinUI.
- **Infrastructure** references Domain and Application. It must not reference WinUI, Mcp, or McpHost.
- **LocalWinAI.Mcp** references Application and Domain only. It must not reference Infrastructure or App.
- **LocalWinAI.McpHost** references Infrastructure, Mcp, Application, and Domain. It is the composition root for the MCP process.
- **App** references Application, Infrastructure, and Mcp. It is the composition root for the GUI process.
- **Tests** references Application, Domain, and Mcp. It must not reference Infrastructure or App.

## Build and Test

From `Src/`:

- Build all projects:
  ```
  dotnet build
  ```
- Run all unit tests:
  ```
  dotnet test LocalWinAI.Tests/LocalWinAI.Tests.csproj
  ```
- Build the WinUI app (requires a runtime identifier):
  ```
  dotnet build App/LocalWinAI.csproj -r win-x64
  ```
