#!/usr/bin/env bash
set -euo pipefail

# Run the API locally with the same env/urls as VS Code launch
PROJECT="TaskMaster.csproj"
CONFIG="Debug"

export ASPNETCORE_ENVIRONMENT="Development"
export ASPNETCORE_URLS="https://localhost:5001;http://localhost:5000"

dotnet run --project "$PROJECT" --configuration "$CONFIG"