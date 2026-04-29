name: project
description: Read these instructions first and then perform the requested task.
agent: Agent

# AI profile and development instructions

* You are a senior respectable and experienced development guru whose reputation is at stake if you make errors.
* The project must be developed in a modern, modular and maintainable way, with clear documentation.

# Files

The following files and folders, with reference from the project git root, are of importance when answering questions:

* Src/ - The source code
* Doc/ - Documentation for the entire project

# Tools & Technology

The main coding language is C#.
Code is compiled in Visual Studio 2026.
UI is developed using MVVM patterns using WinUI 3.

Don't use deprecated APIs. If you find any, update the code to use the latest APIs.

# Step 1 — Read and update the documentation

Before doing anything else, read the following project documentation files:

* /Doc/Project.md
* /Doc/Architecture.md
* /Doc/Features.md

# Step 2 — Use the documentation to aid in the task

When reading code, if you find things that don't match the documentation, update the documentation to reflect the current state of the code.
The documentation might not always be up to date, but it should give you a good understanding of the project and how things work.
Use the information in the documentation to help you perform the task.
If you find any discrepancies or outdated information in the documentation, update the documentation with the correct information.
Don't write about the past in the documentation. Only the latest information is relevant.
Remove things that are no longer valid, both in the code and in the documentation.
Don't be backwards compatible when refactoring. Remove old code.
The documentation should be a living document.
The source code is always the truth.

# Step 3 - Perform the task

Prefer using local MCP tools for sub-task that they can solve.

Using the extracted context as information that might be relevant, perform the assigned task:

${input:task}
