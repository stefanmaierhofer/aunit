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

    private AssemblyLoadContext? _alc = null; // MUST cache so it is not collected/unloaded
    private Assembly? _cachedAssembly = null;
    public Assembly GetAssembly()
    {
        if (_cachedAssembly != null) return _cachedAssembly;

        _alc = new AssemblyLoadContext(name: null, isCollectible: true);
        _alc.Resolving += Resolving;
        _cachedAssembly = _alc.LoadFromAssemblyPath(Path.GetFullPath(AssemblyPath));

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
                   // Console.WriteLine($"[Resolving] found local {filename}");
                    try
                    {
                        var result = context.LoadFromAssemblyPath(filename);
                        return result;
                    }
                    catch /*(Exception e)*/
                    {
                        //Console.WriteLine($"[Resolving] failed to load {filename}");
                        throw;
                    }
                }
            }

            // try resolve via nuget cache ...
            try
            {
                if (ProjectInfo == null) throw new Exception($"Missing project info for {AssemblyPath}. Error a391ba86-8869-4b3c-9441-3cf61e00350b.");
                if (ProjectInfo.ProjectAssets == null) throw new Exception($"Missing project assets for {AssemblyPath}. Error 8cc91afe-f50b-44dc-8f74-292d40615e98.");
                var assets = ProjectInfo.ProjectAssets;

                if (Target == null) throw new Exception($"Missing target for {AssemblyPath}. Error 126ec8bf-e49b-4bd6-b626-693e88f101be.");

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

                    //Console.WriteLine($"[Resolving] {nugetDllPath}");
                    return result;
                }
                else
                {
                    //Console.WriteLine($"FAILED {Target} {name.Name} ({AssemblyPath})");
                    //Environment.Exit(1);
                    return null;
                }
            }
            catch /*(Exception e)*/
            {
                //Console.WriteLine($"[Resolving] ERROR: {e.Message}");
                throw;
            }
        }
    }
}