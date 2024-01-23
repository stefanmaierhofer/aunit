@echo off
SETLOCAL
PUSHD %~dp0

dotnet tool restore
dotnet paket restore
dotnet build src/AUnit.sln -c Release
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained -o publish/au src/au