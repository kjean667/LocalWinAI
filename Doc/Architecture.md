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
└───────────��─┬────────────────────┘   └──────────────┬───────────────────┘
              │                                        │
              ▼                                        ▼
┌─────────────────────┐             ┌──────────────────────────┐
│   Application       │             │      Infrastructure       │
│  IChatService       │             │  WindowsLanguageModel-    │
│  ChatService        │             │  Service (Windows AI SDK) │
│  ChatPageViewModel  │             └──────────────┬────────────┘
│  ObservableChatMsg  │                            │
└──────────┬──────────┘                            │
           │                                        │
           └────────────────┬───────────────────────┘
                            ▼
               ┌────────────────────────┐
               │         Domain         │
               │  ChatMessageSender     │
               │  ILanguageModelService │
               └────────────────────────┘
```

**Dependency rule:** arrows flow inward only. Both `App` and `LocalWinAI.Mcp` are driving adapters at the same level — they depend on Application/Infrastructure/Domain but never on each other.

## Startup Modes

`Program.cs` in the `App` project is the single entry point. It selects the startup mode from the command line:

| Invocation | Mode |
|---|---|
| `LocalWinAI.exe` | WinUI 3 GUI — launches the chat window |
| `LocalWinAI.exe --mcp` | MCP server — stdio transport, no UI |

In MCP mode, the WinUI stack is never initialized. The generic host starts with `Infrastructure` and `Mcp` services only, then drives the MCP stdio read/write loop until stdin is closed.

## MCP Server

`LocalWinAI.Mcp` embeds an MCP server using the official `ModelContextProtocol` 1.x .NET SDK. It exposes four tools:

| Tool | Method | Description |
|---|---|---|
| `local_infer` | `LocalInferTool.InferAsync` | Run a prompt through the local model |
| `local_summarize` | `LocalSummarizeTool.SummarizeAsync` | Summarize text |
| `local_classify` | `LocalClassifyTool.ClassifyAsync` | Classify text into provided categories |
| `local_embed` | `LocalEmbedTool.EmbedAsync` | Not yet supported — throws `NotSupportedException` |

All tools inject `ILanguageModelService` from DI and delegate to the same NPU pipeline used by the chat UI.

## Domain Model

| Type | Layer | Purpose |
|---|---|---|
| `ChatMessageSender` | Domain | Enum: User or AI |
| `ILanguageModelService` | Domain | Interface for on-device text generation |
| `IChatService` | Application | Interface for conversation management |
| `ChatService` | Application | Manages history, builds prompts, delegates to model |
| `ObservableChatMessage` | Application | UI-bindable message with mutable Text and IsWaiting |
| `ChatPageViewModel` | Application | MVVM ViewModel for the chat page |
| `WindowsLanguageModelService` | Infrastructure | Windows Copilot Runtime implementation |
| `LocalInferTool` | Mcp | MCP tool: raw inference |
| `LocalSummarizeTool` | Mcp | MCP tool: summarization via prompted inference |
| `LocalClassifyTool` | Mcp | MCP tool: text classification via prompted inference |
| `LocalEmbedTool` | Mcp | MCP tool stub: embeddings (not yet supported) |

## Development Setup

Requirements:
- Windows 11 24H2 (Build 26100) or later with Copilot+ feature enabled
- Visual Studio 2026 with WinUI / Windows App SDK workload
- .NET 10 SDK

The `Microsoft.WindowsAppSDK` package is used for building the WinUI 3 application and the NPU integration.

## Dependency Boundary Rules

- **Domain** has no external project references.
- **Application** references Domain only. It must not reference Infrastructure or WinUI.
- **Infrastructure** references Domain only. It must not reference Application.
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
