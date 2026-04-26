# LocalWinAI Architecture

This document describes the current implemented architecture in `Src/`.

## Solution and Project Structure

```
Src/
  LocalWinAI.sln
  LocalWinAI.Domain/          — Core abstractions and domain types (net10.0-windows10.0.19041.0)
  LocalWinAI.Application/     — Business logic, ViewModels, DI registration (net10.0-windows10.0.19041.0)
  LocalWinAI.Infrastructure/  — Windows AI integration (net10.0-windows10.0.19041.0)
  App/                        — WinUI 3 views and composition root (net10.0-windows10.0.19041.0)
  LocalWinAI.Tests/           — xUnit unit tests (net10.0-windows10.0.19041.0)
```

## Layered Architecture

```
┌─────────────────────────────────────────────────────┐
│  App  (WinUI 3 Views, App.xaml.cs composition root) │
└────────────┬──────────────────────────┬─────────────┘
             │                          │
             ▼                          ▼
┌────────────────────┐    ┌──────────────────────────┐
│   Application      │    │      Infrastructure       │
│  IChatService      │    │  WindowsLanguageModel-    │
│  ChatService       │    │  Service (Windows AI SDK) │
│  ChatPageViewModel │    └──────────────┬────────────┘
│  ObservableChat-   │                   │
│  Message           │                   │
└────────────┬───────┘                   │
             │                           │
             └──────────┬────────────────┘
                        ▼
             ┌─────────────────────┐
             │       Domain        │
             │  ChatMessageSender  │
             │  ILanguageModel-    │
             │  Service            │
             └─────────────────────┘
```

**Dependency rule:** arrows flow inward only. App and Infrastructure both depend on Application and Domain. Application depends on Domain. Domain has no project dependencies.

## Current Implemented Behavior

### Chat Flow

1. User types a message and clicks **Ask** (or presses Enter).
2. `ChatPageViewModel.GenerateResponseAsync` adds a user message bubble and a waiting AI bubble to the observable collection.
3. `ChatService.SendMessageAsync` appends the user message to internal conversation history, calls `ILanguageModelService.EnsureReadyAsync`, then calls `GenerateResponseAsync` with the full history joined as a prompt.
4. `WindowsLanguageModelService` delegates to the Windows Copilot Runtime (`Microsoft.Windows.AI.Text.LanguageModel`), which runs inference on the on-device NPU.
5. The AI response is stored in conversation history and returned to the ViewModel, which updates the AI bubble text.

### Conversation Management

- `ChatService` maintains the conversation history as an ordered list of prompt lines.
- `ChatPageViewModel` maintains an `ObservableCollection<ObservableChatMessage>` for UI data binding.
- Clearing the conversation resets both the ViewModel collection and the `ChatService` history.

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
- **App** references Application and Infrastructure. It is the composition root.
- **Tests** references Application and Domain only. It must not reference Infrastructure or App.

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
