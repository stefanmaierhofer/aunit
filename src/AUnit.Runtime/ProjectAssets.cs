/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

using System.Text.Json;

namespace AUnit;

public record ProjectAssets(
    int Version,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, ProjectAssets.Target>> Targets,
    JsonElement Libraries,
    JsonElement ProjectFileDependencyGroups,
    JsonElement PackageFolders,
    JsonElement Project
    )
{
    public record Target(
        string Type,
        IReadOnlyDictionary<string, string> Dependencies,
        IReadOnlyDictionary<string, JsonElement> Compile,
        IReadOnlyDictionary<string, JsonElement> Runtime
        );

    private IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>? _target2name2path = null;

    private void InitTarget2Name2Path()
    {
        if (_target2name2path != null) return;

        var target2name2path = new Dictionary<string, Dictionary<string, string>>();
        _target2name2path = (IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>)target2name2path;

        foreach (var target in Targets)
        {
            var name2path = new Dictionary<string, string>();

            //Console.WriteLine($"target: {target.Key}");
            foreach (var x in target.Value)
            {
                if (x.Value.Type == "package")
                {
                    var p = Path.Combine(Utils.NugetPackageFolder, x.Key.ToLower());

                    //Console.WriteLine($"    prefix: {p}");

                    if (x.Value.Runtime != null)
                    {
                        foreach (var y in x.Value.Runtime.Keys)
                        {
                            var packageName = Path.GetFileNameWithoutExtension(y);
                            var assemblyPath = Path.GetFullPath(Path.Combine(p, y));
                            //Console.WriteLine($"    {packageName,-60} {assemblyPath}");
                            if (!File.Exists(assemblyPath)) throw new Exception();

                            if (packageName != "_")
                            {
                                name2path.Add(packageName, assemblyPath);
                            }
                        }
                    }
                }
            }

            var targetKey = target.Key;
            if (Utils.TfmOld2New.TryGetValue(targetKey, out var newTargetKey)) targetKey = newTargetKey;
            target2name2path.Add(targetKey, name2path);
        }
    }

    public bool TryGetNugetAssemblyPath(string target, string assemblyName, out string? path)
    {
        if (_target2name2path == null) InitTarget2Name2Path();
        if (_target2name2path == null) throw new Exception("Error 684872fb-4056-4926-9a2b-bebcd10b6856.");

        // try target ...
        if (!_target2name2path.TryGetValue(target, out var name2path))
        {
            // check if target is an old TFM and we can map it to the new one
            if (Utils.TfmOld2New.TryGetValue(target, out var newTarget))
            {
                _target2name2path.TryGetValue(newTarget, out name2path);
            }
        }

        if (name2path != null && name2path.TryGetValue(assemblyName, out path))
        {
            return true;
        }

        path = null;
        return false;
    }
}
