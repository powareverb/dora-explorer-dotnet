# Overview

## Local Testing

All instructions below assume using the current directory (src/dora-explorer-dotnet-tool/dora-explorer-dotnet-tool).
To test locally:

### Install package

```sh
# Install package
dotnet tool list -g
dotnet build
dotnet pack

# Pack'd tool nupkg will be under folder:
# src/dora-explorer-dotnet-tool/dora-explorer-dotnet-tool/bin/Pack
# or relative to project:
# ./bin/Pack/
dotnet tool install --global --prerelease --add-source ./bin/Pack/ dora-explorer-dotnet
```

### Test package

```sh
# Test package
dora-explorer-dotnet
```

### Uninstall package

```sh
# Install package
dotnet tool list -g
dotnet tool uninstall dora-explorer-dotnet --global --prerelease
```
