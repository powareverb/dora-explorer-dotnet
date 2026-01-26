# Dora Explorer - AI Coding Agent Instructions

## Project Overview
Dora Explorer is a .NET 9/10 DORA metrics calculator that ingests issue data from Jira and generates DORA metrics (Deployment Frequency, Lead Time, Change Failure Rate, MTTR). It outputs metrics to CSV, Markdown, and Confluence.

## Architecture
The project is organized as a **multi-project solution** under `src/`:
- **dora-explorer-core**: Shared library with core business logic (targets net9.0, net10.0)
- **dora-explorer-dotnet-tool**: CLI tool packaged as a global .NET tool (targets net9.0, net10.0)
- **dora-explorer-dotnet-tool.MSTest**: MSTest-based functional and integration tests

Key design: ProjectReferences in tool project are auto-transformed to PackageReferences during pack, requiring centralized version management via `PackageMetadata.props`.

## Planning
- Features are specified in `docs/features/` with detailed requirements. Follow these specs closely when implementing features.
- Follow general guidelines for documenting and implementing features in `docs/features/0-README.md`.

## Build and Pack Workflow
```sh
# Navigate to tool directory
cd src/dora-explorer-dotnet-tool/dora-explorer-dotnet-tool

# Local development
dotnet build
dotnet pack  # Creates .nupkg in bin/Release/

# Install locally for testing
dotnet tool install --global --prerelease --add-source ./bin/Pack/ dora-explorer-dotnet

# Test the tool
dora-explorer-dotnet

# Uninstall
dotnet tool uninstall dora-explorer-dotnet --global
```

**Critical**: Pack operation automatically transforms ProjectReferences to PackageReferences with cascading versions from `VersionPrefix` property. Verify `PackageMetadata.props` before packing.

## C# Code Standards
- **Framework**: Use C# 14 features, target both net9.0 and net10.0
- **Nullability**: `Nullable>disable` in tool, `Nullable>enable` in tests. Use `is null`/`is not null`
- **Namespacing**: File-scoped namespaces (e.g., `namespace DoraExplorer.Core;`)
- **Testing**: MSTest framework with TestInitialize/TestMethod attributes. Don't use Arrange/Act/Assert comments. Mock console I/O with `Console.SetOut/SetIn` for functional tests
- **Formatting**: Follow `.editorconfig` rules; insert newline before opening braces

## Key Project Files
- [Directory.build.props](src/dora-explorer-dotnet-tool/dora-explorer-dotnet-tool/Directory.build.props) - Empty; versioning in PackageMetadata.props
- [PackageMetadata.props](src/dora-explorer-dotnet-tool/dora-explorer-dotnet-tool/Package/Config/PackageMetadata.props) - Version control and NuGet metadata (VersionMajor/Minor/Build/Revision)
- [PackageFileMappings.props](src/dora-explorer-dotnet-tool/dora-explorer-dotnet-tool/Package/Config/PackageFileMappings.props) - File inclusion rules for package
- [Program.cs](src/dora-explorer-dotnet-tool/dora-explorer-dotnet-tool/Program.cs) - CLI entry point
- [FunctionalTests.cs](src/dora-explorer-dotnet-tool/dora-explorer-dotnet-tool.MSTest/FunctionalTests.cs) - Console I/O tests

## Testing Pattern Example
Functional tests capture console I/O redirection:
```csharp
StringWriter @out = new StringWriter();
Console.SetOut(@out);
Console.SetIn(new StringReader("input\nlines"));
Program.Main(Array.Empty<string>());
Assert.AreEqual(output, expectedOutput);
```

## External Guidelines
- Refer to [General Development Guidelines](.github/instructions/general.instructions.md) for cross-language best practices on code quality, documentation, and testing.
- Refer to [C# Development Guidelines](.github/instructions/csharp.instructions.md) for comprehensive standards on logging, data access, validation, and deployment patterns
- Refer to [Markdown Documentation Guidelines](.github/instructions/markdown.instructions.md) for writing and formatting project documentation.
