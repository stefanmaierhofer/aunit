/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

using AUnit;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Versioning;

namespace au;

internal static class Discovery
{
    /// <summary>
    /// Enumerates all assembly files starting at 1 or more roots.
    /// A root is either a solution file (.sln), an assembly (.dll), or a directory that will be searched recursively for .sln files.
    /// If no .sln files are found, then the directory will be recursively searched for .dll files.
    /// </summary>
    public static IReadOnlyList<AssemblyContext> EnumerateAssemblyFiles(IEnumerable<string> roots)
    {
        var dlls = new List<AssemblyContext>();

        foreach (var root in roots)
        {
            var path = Path.GetFullPath(root);
            var ext = Path.GetExtension(path).ToLower();

            switch (ext)
            {
                case ".dll":
                    {
                        if (!File.Exists(path)) throw new Exception(
                            $"File \"{path}\" does not exist. Error 61d57ad0-aa94-4dbb-ab9a-e049abc932eb."
                            );

                        dlls.Add(AssemblyContext.FromDll(path));

                        break;
                    }

                case ".sln":
                    {
                        var infos = GetAssemblyInfosFromSolutionFile(path);
                        dlls.AddRange(infos);
                        break;
                    }

                default:
                    {
                        if (!Directory.Exists(path)) throw new Exception(
                            $"Directory \"{path}\" does not exist. Error 144727ce-319a-4b63-b628-7b5f3ad7e95d."
                            );

                        // first, check for .sln files ...
                        var slnFound = false;
                        var slns = Directory.EnumerateFiles(path, "*.sln", SearchOption.AllDirectories);
                        foreach (var sln in slns)
                        {
                            var infos = GetAssemblyInfosFromSolutionFile(sln);
                            dlls.AddRange(infos);
                            slnFound = true;
                        }

                        // if no .sln files have been found, then brute force enumerate all .dll files
                        if (!slnFound)
                        {
                            var infos = GetAssemblyInfosFromDirectory(path);
                            dlls.AddRange(infos);
                        }

                        break;
                    }
            }
        }

        var result = dlls.OrderBy(x => Path.GetFileName(x.AssemblyPath)).ToImmutableList();
        return result;

        static IEnumerable<AssemblyContext> GetAssemblyInfosFromSolutionFile(string solutionFile)
        {
            var slnFolder = Path.GetDirectoryName(solutionFile)!;

            var lines = File.ReadAllLines(solutionFile);

            foreach (var line in lines)
            {
                var s = line.Trim();
                if (!s.StartsWith("Project(")) continue;

                var ts = s.Split(",", StringSplitOptions.TrimEntries);
                var relProjPath = ts[1][1..^1];

                var absProjPath = Path.Combine(slnFolder, relProjPath);
                if (!File.Exists(absProjPath)) continue;

                var info = ProjectInfo.FromProjectFile(absProjPath);
                var xs = AssemblyContext.FromProjectInfo(info);
                foreach (var x in xs) yield return x;
            }
        }

        static IEnumerable<AssemblyContext> GetAssemblyInfosFromDirectory(string dir)
        {
            var fs = Directory.EnumerateFiles(dir, "*.dll", SearchOption.AllDirectories);
            foreach (var f in fs) yield return AssemblyContext.FromDll(f);
        }
    }

    /// <summary>
    /// Enumerates all assembly files starting at 1 or more roots.
    /// A root is either a solution file (.sln), an assembly (.dll), or a directory that will be searched recursively for .sln files.
    /// If no .sln files are found, then the directory will be recursively searched for .dll files.
    /// </summary>
    public static IReadOnlyList<AssemblyContext> EnumerateAssemblyFiles(params string[] roots)
        => EnumerateAssemblyFiles((IEnumerable<string>)roots);

    //public static IReadOnlyList<Assembly> LoadAssemblies(IEnumerable<AssemblyContext> dllPaths, Action<AssemblyContext, Assembly> onSuccess, Action<AssemblyContext, Exception> onFail)
    //{
    //    var assemblies = ImmutableList<Assembly>.Empty;

    //    foreach (var context in dllPaths)
    //    {
    //        try
    //        {
    //            //AnsiConsole.MarkupLine($"[yellow]{context.ProjectInfo?.ProjectFilePath} {context.Target} {context.AssemblyPath}[/]");
    //            var assembly = context.GetAssembly();
    //            assemblies = assemblies.Add(assembly);

    //            onSuccess.Invoke(context, assembly);
    //        }
    //        catch (Exception e)
    //        {
    //            onFail.Invoke(context, e);
    //        }
    //    }
    //    return assemblies;
    //}

    public static string? GetAssemblyConfiguration(this Assembly assembly)
    {
        return assembly.CustomAttributes
            ?.SingleOrDefault(x => x.AttributeType == typeof(AssemblyConfigurationAttribute))
            ?.ConstructorArguments.SingleOrDefault().Value?.ToString()
            ;
    }

    public static string? GetAssemblyTarget(this Assembly assembly)
    {
        return assembly.CustomAttributes
            ?.SingleOrDefault(x => x.AttributeType == typeof(TargetFrameworkAttribute))
            ?.NamedArguments[0].TypedValue.Value?.ToString()
            ;
    }

    public static string? GetAssemblyVersion(this Assembly assembly)
    {
        return assembly.CustomAttributes
            ?.SingleOrDefault(x => x.AttributeType == typeof(AssemblyFileVersionAttribute))
            ?.ConstructorArguments.SingleOrDefault().Value?.ToString()
            ;
    }
}
