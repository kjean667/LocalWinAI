# Coding Standards

These standards apply to all C# code in the project.
They complement `.editorconfig` (which handles formatting mechanically).

## General Principles

- Write code for the reader, not the compiler.
- Prefer clarity over cleverness.
- Keep methods short and focused on a single responsibility.
- Avoid premature abstraction — extract only when the pattern repeats or complexity demands it.

## Naming

| Element | Convention | Example |
|---|---|---|
| Classes, interfaces, records | PascalCase | `IssueService`, `IIssueRepository` |
| Methods, properties | PascalCase | `GetByIdAsync`, `Title` |
| Private fields | `_camelCase` | `_dbContext` |
| Local variables, parameters | camelCase | `issueId`, `createdAt` |
| Constants | PascalCase | `MaxClaimDurationDays` |
| Interfaces | `I` prefix | `IIssueRepository` |
| Async methods | `Async` suffix | `CreateIssueAsync` |

## C# Language Usage

- Use `record` types for immutable value objects and DTOs.
- Use `sealed` on classes that are not designed for inheritance.
- Prefer `required` properties and primary constructors over mutable setters.
- Use `CancellationToken` in all async method signatures.
- Prefer `IReadOnlyList<T>` / `IReadOnlyCollection<T>` for return types instead of `List<T>`.
- Never use `dynamic`. Avoid `object` except at infrastructure boundaries.
- Do not use `Thread.Sleep` — use `Task.Delay` with a `CancellationToken`.

## Nullability

- Nullable reference types are enabled globally. Treat every compiler warning as an error.
- Never suppress nullability warnings with `!` unless there is a documented reason.
- Prefer `if (x is null)` over `if (x == null)`.

## Error Handling

- Use domain-specific exceptions for expected failure paths (e.g., `IssueNotFoundException`).
- Do not catch `Exception` except in the global exception handler middleware.
- Log errors with context at the point where the exception is caught.
- Never swallow exceptions silently.

## Dependency Injection

- Register services in extension methods (`AddDomainServices`, `AddInfrastructure`, etc.).
- Prefer constructor injection. Avoid service locator pattern (`IServiceProvider` injection).
- Use `Scoped` lifetime for EF Core contexts and repository implementations.

## Testing

- Use xUnit for all tests. Use FluentAssertions for assertions.
- Name tests: `MethodUnderTest_Scenario_ExpectedBehavior`.
- Unit tests live in `App.Tests` and `App.Tests`.
- Do not mock what you own — use real implementations or in-memory fakes for domain logic.

## Documentation

- Update `Doc/Architecture.md` and `Doc/Features.md` when adding or removing features.
- Public APIs (controllers, service interfaces) require XML doc comments.
- Internal implementation details do not need comments unless the logic is non-obvious.

## Line endings

- All files in the repository MUST use CRLF line endings (Windows-style). This repository standard
  is enforced by `.editorconfig` and `.gitattributes` and must be followed by all contributors.
- Editors and CI should normalize files to CRLF on checkout/commit. When adding or modifying files,
  ensure the editor is configured to use `end_of_line = crlf` and `insert_final_newline = true`.

Notes:
- Use `.editorconfig` to declare `end_of_line = crlf` for all file types.
- Add a repository `.gitattributes` file with `* text=auto eol=crlf` to ensure consistent checkouts.
- If you encounter mojibake sequences (e.g., "â€”", "â†’", "â”€") after editing, re-save the file as UTF-8
  and verify the correct Unicode characters (`—`, `→`, `–`) are present.
