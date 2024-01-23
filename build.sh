#!/bin/bash

dotnet tool restore
dotnet paket restore
dotnet build src/aunit.sln -c Release
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained -o publish/au src/au