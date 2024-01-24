@echo off
dotnet tool restore
dotnet paket restore
dotnet build src\AUnit.sln -c Release
dotnet build src\AUnit.sln -c Debug
dotnet pack src\au\au.csproj -c Release -o bin\pack
dotnet run --project src/getversion/getversion.fsproj
for /f "delims=" %%x in (.version) do set VERSION=%%x
echo "VERSION: %VERSION%"
dotnet paket pack bin/pack --version %VERSION%
