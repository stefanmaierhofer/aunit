/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

using System.Reflection;
using System.Runtime.Loader;

namespace AUnit;

public record AssemblyContext(
    string AssemblyPath,
    ProjectInfo? ProjectInfo,
    string? Configuration,
    string? Target
    )
{
    public static IEnumerable<AssemblyContext> FromProjectInfo(ProjectInfo info)
    {
        foreach (var config in info.Configurations)
        {
            var targetDirs = Directory.GetDirectories(Path.Combine(info.ProjectFolder, "bin", config));
            foreach (var targetDir in targetDirs)
            {
                var target = Path.GetFileName(targetDir);
                var assemblyPath = Path.Combine(targetDir, info.AssemblyName + ".dll");
                if (!File.Exists(assemblyPath))
                {
                    assemblyPath = Path.Combine(targetDir, info.AssemblyName + ".exe");
                    if (!File.Exists(assemblyPath))
                    {
                        // might be empty folder, not yet built, etc...
                        continue;
                    }
                }

                yield return new(
                    AssemblyPath: assemblyPath,
                    ProjectInfo: info,
                    Configuration: config,
                    Target: target
                    );
            }
        }
    }

    public static AssemblyContext FromDll(string dllPath)
    {
        return new(
            AssemblyPath: dllPath,
            ProjectInfo: null,
            Configuration: null,
            Target: null
            );
    }

    private Assembly? _cachedAssembly = null;
    public Assembly GetAssembly()
    {
        if (_cachedAssembly != null) return _cachedAssembly;

        var alc = new AssemblyLoadContext(name: null, isCollectible: true);
        alc.Resolving += Resolving;
        _cachedAssembly = alc.LoadFromAssemblyPath(Path.GetFullPath(AssemblyPath));

        return _cachedAssembly;

        Assembly? Resolving(AssemblyLoadContext context, AssemblyName name)
        {
            //Console.WriteLine($"[Resolving] target={Target} name={name.Name} fullname={name.FullName} ({AssemblyPath})");

            // try local dir ...
            {
                var localDir = Path.GetDirectoryName(AssemblyPath)!;
                var filename = Path.Combine(localDir, name.Name + ".dll");
                if (File.Exists(filename))
                {
                    //Console.WriteLine($"[Resolving] found local {filename}");
                    var result = context.LoadFromAssemblyPath(filename);
                    return result;
                }
            }

            // try resolve via nuget cache ...
            {
                if (ProjectInfo == null) throw new Exception($"Missing project info for {AssemblyPath}. Error a391ba86-8869-4b3c-9441-3cf61e00350b.");
                if (ProjectInfo.ProjectAssets == null) throw new Exception($"Missing project assets for {AssemblyPath}. Error 8cc91afe-f50b-44dc-8f74-292d40615e98.");
                var assets = ProjectInfo.ProjectAssets;

                if (Target == null) throw new Exception($"Missing target for {AssemblyPath}. Error 126ec8bf-e49b-4bd6-b626-693e88f101be.");

                //if (!assets.TryGetValue(Target, out var targetInfo))
                //{
                //    if (Target == "netstandard2.0")
                //    {
                //        if (!assets.TryGetValue(".NETStandard,Version=v2.0", out targetInfo))
                //        {
                //            throw new Exception($"Failed to find target \"{Target}\" in [{string.Join(';', assets.Keys)}]. Error a9c9baeb-703d-4b11-8d6b-19c1ba6b2188.");
                //        }
                //    }
                //    else
                //    {
                //        throw new Exception($"Failed to find target \"{Target}\" in [{string.Join(';', assets.Keys)}]. Error e6fe6b39-f03c-48b1-9e33-dc086a07e986.");
                //    }
                //}


                //var mapping = targetInfo.Keys.ToImmutableDictionary(x => x.Split('/')[0]);
                //var found = targetInfo[mapping[name.Name!]];

                ////Console.WriteLine($"[Resolving] mapping {mapping[name.Name!]}");

                //var nugetBase = Path.Combine("C:\\Users\\sm\\.nuget\\packages", mapping[name.Name!].ToLower());
                //var nugetPath = found.Runtime.Keys.Single();
                //var nugetDll = Path.Combine(nugetBase, nugetPath);

                //Console.WriteLine($"[Resolving] trying to load {nugetDll}");

                if (assets.TryGetNugetAssemblyPath(Target, name.Name!, out var nugetDllPath))
                {
                    if (nugetDllPath == null) throw new Exception($"Error 9be3c032-6e49-4ffc-8452-f5679b7c0252.");

                    if (!File.Exists(nugetDllPath))
                    {
                        Console.WriteLine($"[Resolving] file not found {nugetDllPath}");
                    }

                    var result = context.LoadFromAssemblyPath(nugetDllPath) ?? throw new NotImplementedException(
                        $"TODO a98d32c1-846d-4728-aba0-7e0a52ea5922. name={name.Name} path={nugetDllPath} fullname={name.FullName} version={name.Version} - choose from {string.Join(";", assets.Targets.Keys)}"
                        );

                    //Console.WriteLine($"[Resolving] {dllPath}");
                    return result;
                }
                else
                {
                    //AnsiConsole.MarkupLine($"[red]FAILED {Target} {name.Name} ({AssemblyPath})[/]");
                    //Environment.Exit(1);
                    return null;
                }
            }
        }
    }
}