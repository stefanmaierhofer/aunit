/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

using System.Text.Json;
using System.Xml.Linq;

namespace AUnit;

/// <summary>
/// Project file info (.csproj, .fsproj).
/// </summary>
public record ProjectInfo(
    string ProjectFilePath,
    string ProjectFolder,
    string AssemblyName,
    ProjectAssets? ProjectAssets,
    IReadOnlyList<string> Configurations,
    IReadOnlyList<string> TargetFrameworks
    )
{
    public static ProjectInfo FromProjectFile(string filename)
    {
        filename = Path.GetFullPath(filename);

        if (!File.Exists(filename)) throw new Exception(
            $"File \"{filename}\" does not exist. Error e20fa332-0cba-4e62-8f35-b6fbb6c52dbc."
            );

        var projectFolder = Path.GetDirectoryName(filename)!;

        // project.assets.json
        var projectAssetsPath = Path.Combine(projectFolder, "obj", "project.assets.json");
        ProjectAssets? projectAssets = null;
        if (File.Exists(projectAssetsPath))
        {
            projectAssets = JsonSerializer.Deserialize<ProjectAssets>(
                File.ReadAllText(projectAssetsPath),
                Utils.JsonOptions
                );
        }

        // configurations
        var configs = Directory
            .EnumerateDirectories(Path.Combine(projectFolder, "bin"))
            .Select(x => Path.GetFileName(x) ?? throw new Exception(
                $"Failed to extract config from path \"{x}\". Error 54ca5009-ee70-4834-b8d3-a0a443bf334e."
                ))
            .ToList()
            ;

        // target frameworks
        IReadOnlyList<string> targetFrameworks = [];
        {
            var xmlDoc = XDocument.Load(filename);
            var propertyGroups = xmlDoc.Root!.Elements("PropertyGroup");
            foreach (var propertyGroup in propertyGroups)
            {
                var tfw = propertyGroup.Element("TargetFrameworks")?.Value;
                if (tfw != null)
                {
                    targetFrameworks = tfw.Split(';') ?? [];
                    break; // found the property group with 'TargetFrameworks' entry -> done
                }
            }
        }

        var assemblyName = Path.GetFileNameWithoutExtension(filename);
        return new(
            ProjectFilePath: filename,
            ProjectFolder: projectFolder,
            AssemblyName: assemblyName,
            ProjectAssets: projectAssets,
            Configurations: configs,
            TargetFrameworks: targetFrameworks
            );
    }
}
