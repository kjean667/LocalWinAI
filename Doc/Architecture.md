# LocalWinAI Architecture

This document describes the current implemented architecture in `Src/`.

## Solution and Project Structure

```
Src/
  LocalWinAI.Domain/          — Core abstractions and domain types (net10.0-windows10.0.19041.0)
  LocalWinAI.Application/     — Business logic, ViewModels, DI registration (net10.0-windows10.0.19041.0)
  LocalWinAI.Infrastructure/  — Windows AI integration (net10.0-windows10.0.19041.0)
  LocalWinAI.Mcp/             — MCP server adapter: stdio transport and tool implementations (net10.0-windows10.0.19041.0)
  App/                        — WinUI 3 views and composition root (net10.0-windows10.0.19041.0)
  LocalWinAI.Tests/           — xUnit unit tests (net10.0-windows10.0.19041.0)
```

The solution file is `LocalWinAI.slnx` at the repository root.

## Layered Architecture

```
┌──────────────────────────────────┐   ┌──────────────────────────────────┐
│  App  (WinUI 3, Program.cs)      │   │  LocalWinAI.Mcp                  │
│  Driving adapter — GUI mode      │   │  Driving adapter — MCP stdio mode │
└──────────────┬────────────────────┘   └──────────────┬───────────────────┘
               │                                        │
               ▼                                        ▼
┌──────────────────────────────────────────────────────────────────────┐
│   Application                                                        │
│  IChatService / ChatService          IUsageAggregateService          │
│  ChatPageViewModel                   StatisticsPageViewModel         │
│  ObservableChatMessage               UsageAggregates / ToolStats     │
│  SettingsPageViewModel               DayStats / ToolStatRow          │
└──────────────────────────────┬───────────────────────────────────────┘
                               │
                               ▼
               ┌──────────────────────────────────────┐
               │          Infrastructure               │
               │  WindowsLanguageModelService          │
               │  ClaudeCodeSettingsService            │
               │  UsageTracker (→ usage.ndjson)        │
               │  UsageAggregateService (FileWatcher)  │
               └──────────────┬───────────────────────┘
                              │
                              ▼
               ┌────────────────────────────────┐
               │           Domain               │
               │  ChatMessageSender             │
               │  ILanguageModelService         │
               │  IUsageTracker / UsageEvent    │
               └────────────────────────────────┘
```

**Dependency rule:** arrows flow inward only. Both `App` and `LocalWinAI.Mcp` are driving adapters at the same level — they depend on Application/Infrastructure/Domain but never on each other.

## Startup Modes

`Program.cs` in the `App` project is the single entry point. It selects the startup mode from the command line:

| Invocation | Mode |
|---|---|
| `LocalWinAI.exe` | WinUI 3 GUI — launches the chat window |
| `LocalWinAI.exe --mcp` | MCP server — stdio transport, no UI |

In MCP mode, the WinUI stack is never initialized. The generic host starts with `Infrastructure` and `Mcp` services only, then drives the MCP stdio read/write loop until stdin is closed.

Both startup modes register `IUsageTracker`. Every tool call and chat turn appends a record to the shared log at `%LOCALAPPDATA%\LocalWinAI\usage.ndjson`, so the GUI's Statistics page captures usage from both sources.

## MCP Server

`LocalWinAI.Mcp` embeds an MCP server using the official `ModelContextProtocol` 1.x .NET SDK. It exposes four tools:

| Tool | Method | Description |
|---|---|---|
| `local_infer` | `LocalInferTool.InferAsync` | Run a prompt through the local model |
| `local_summarize` | `LocalSummarizeTool.SummarizeAsync` | Summarize text |
| `local_classify` | `LocalClassifyTool.ClassifyAsync` | Classify text into provided categories |
| `local_embed` | `LocalEmbedTool.EmbedAsync` | Not yet supported — throws `NotSupportedException` |

All tools inject `ILanguageModelService` and `IUsageTracker` from DI. Every successful invocation records a `UsageEvent` to the shared log.

## Domain Model

| Type | Layer | Purpose |
|---|---|---|
| `ChatMessageSender` | Domain | Enum: User or AI |
| `ILanguageModelService` | Domain | Interface for on-device text generation |
| `IUsageTracker` | Domain | Write interface: append a `UsageEvent` to the shared log |
| `UsageEvent` | Domain | Per-call record: source, tool, estimated tokens, duration |
| `IChatService` | Application | Interface for conversation management |
| `ChatService` | Application | Manages history, builds prompts, records usage, delegates to model |
| `ObservableChatMessage` | Application | UI-bindable message with mutable Text and IsWaiting |
| `ChatPageViewModel` | Application | MVVM ViewModel for the chat page |
| `IUsageAggregateService` | Application | Read interface: aggregated stats + `AggregatesChanged` event |
| `UsageAggregates` | Application | Computed totals, per-tool breakdown, 7-day activity, cost estimate |
| `StatisticsPageViewModel` | Application | MVVM ViewModel for the statistics page |
| `WindowsLanguageModelService` | Infrastructure | Windows Copilot Runtime implementation |
| `UsageTracker` | Infrastructure | Appends NDJSON events to `%LOCALAPPDATA%\LocalWinAI\usage.ndjson` |
| `UsageAggregateService` | Infrastructure | Reads and aggregates the log; watches for changes; 90-day compaction |
| `LocalInferTool` | Mcp | MCP tool: raw inference |
| `LocalSummarizeTool` | Mcp | MCP tool: summarization via prompted inference |
| `LocalClassifyTool` | Mcp | MCP tool: text classification via prompted inference |
| `LocalEmbedTool` | Mcp | MCP tool stub: embeddings (not yet supported) |

## MCP Host Architecture Rationale

The solution includes a separate `LocalWinAI.McpHost` project to handle MCP server functionality through inter-process communication with the main `LocalWinAI` application. This division is necessary due to platform constraints:

- **MCP Host Limitations**: The MCP host executable (`LocalWinAI.McpHost.exe`) cannot directly access the local language model because it is not packaged as an AppX application and lacks access to the Limited Access Feature (LAF) required for Windows Copilot Runtime integration.

- **Main App Limitations**: The main `LocalWinAI` application, being a WinUI 3 AppX package, cannot function as an MCP host because it lacks stdin access in packaged applications.

To bridge this gap, `LocalWinAI.McpHost` communicates with the main `LocalWinAI` application through a named pipe. The MCP host sends language model commands via the pipe, and the main application executes them using its Windows Runtime access, then returns results back through the pipe. This architecture allows MCP clients to leverage local AI capabilities.

## Development Setup

Requirements:
- Windows 11 24H2 (Build 26100) or later with Copilot+ feature enabled
- Visual Studio 2026 with WinUI / Windows App SDK workload
- .NET 10 SDK

The `Microsoft.WindowsAppSDK` package is used for building the WinUI 3 application and the NPU integration.

## Dependency Boundary Rules

- **Domain** has no external project references.
- **Application** references Domain only. It must not reference Infrastructure or WinUI.
- **Infrastructure** references Domain and Application. It must not reference WinUI or Mcp.
- **LocalWinAI.Mcp** references Application and Domain only. It must not reference Infrastructure or App.
- **App** references Application, Infrastructure, and Mcp. It is the composition root.
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
