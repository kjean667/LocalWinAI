# LocalWinAI

LocalWinAI is a WinUI 3 desktop application that provides an on-device AI chat experience powered by the Windows Copilot Runtime (Intel NPU). No cloud connectivity is required; all inference runs locally.

## Goals

- Fast, private, on-device AI chat using the Windows Copilot Runtime language model.
- A clean, layered codebase that is easy to extend as the local AI ecosystem matures.
- MCP integration so that tools like Claude Code can delegate lightweight tasks (summarization, classification, inference) to the local NPU instead of consuming cloud API tokens.
- Usage statistics that surface how many tokens have been processed locally and estimate the cloud cost saved, updated in near real-time across both the GUI and MCP processes.

## Repository Layout

```
Doc/          — Living documentation (Architecture, Features, Project)
Src/          — All source code and the solution file
  App/                        WinUI 3 views and composition root; runs the named pipe inference server
  LocalWinAI.Application/     Business logic and ViewModels
  LocalWinAI.Domain/          Core interfaces and domain types
  LocalWinAI.Infrastructure/  Windows AI SDK integration and named pipe server
  LocalWinAI.Mcp/             MCP tool implementations
  LocalWinAI.McpHost/         MCP stdio host executable; bridges MCP clients to the named pipe
  LocalWinAI.Tests/           xUnit unit tests
```

## Technology Stack

| Concern | Choice |
|---|---|
| Language | C# (.NET 10) |
| UI framework | WinUI 3 (Windows App SDK 2.0-experimental) |
| MVVM toolkit | CommunityToolkit.Mvvm 8.4 |
| Local AI runtime | Windows Copilot Runtime (`Microsoft.Windows.AI.Text.LanguageModel`) |
| DI container | Microsoft.Extensions.DependencyInjection |
| Test framework | xUnit + FluentAssertions |
| Build tooling | Visual Studio 2026 / dotnet CLI |

## Requirements

- Windows 11 24H2 (Build 26100) or later with Copilot+ feature enabled
- Copilot+ PC with NPU (or compatible hardware for local model inference)
- `systemAIModels` capability declared in the app manifest
