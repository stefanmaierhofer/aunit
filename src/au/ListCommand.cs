/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

using au;
using AUnit;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0079
#pragma warning disable IL3000

namespace aunit;

public class ListCommandSettings : CommandSettings
{
}

public class ListCommand : Command<ListCommand.Settings>
{
    public class Settings : RunCommandSettings
    {
        [Description("Specific directories, solution files, or assemblies to search tests in. ")]
        [CommandArgument(0, "[PATH*]")]
        public string[] Paths { get; set; } = [];
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        // if user specifies no paths then we use current directory ...
        var paths = settings.Paths;
        if (paths.Length == 0) paths = ["."];

        // discover assemblies ...
        var assembliesToTest = Discovery.EnumerateAssemblyFiles(paths);

        //// load each assembly into its own isolated AssemblyLoadContext ...
        //var assemblies = Discovery.LoadAssemblies(
        //    assembliesToTest,
        //    onSuccess: (path, a) => { },
        //    onFail: (path, e) => { }
        //    );


        var data = assembliesToTest
            .Select(assembly =>
            {
                try
                {
                    var a = assembly.GetAssembly();
                    return new
                    {
                        Assembly = a,
                        Tests = TestRunner.DiscoverTests(a).ToArray()
                    };
                }
                catch
                {
                    return null;
                }
            })
            .Where(x => x != null)
            .Select(x =>
            {
                var name = x!.Assembly.GetName();
                return new
                {
                    x.Assembly,
                    Name = name.Name ?? name.FullName,
                    Version = x.Assembly.GetAssemblyVersion() ?? "???",
                    Target = x.Assembly.GetAssemblyTarget() ?? "???",
                    Config = x.Assembly.GetAssemblyConfiguration() ?? "???",
                    x.Tests
                };
            })
            .Where(x => x.Tests.Length > 0)
            .OrderBy(x => x.Name + ";" + x.Version + ";" + x.Config + ";" + x.Target)
            .ToArray()
            ;

        var table = new Table()
                .AddColumn("#", x => x.Alignment = Justify.Right)
                .AddColumn("Assembly")
                .AddColumn("Version")
                .AddColumn("Config")
                .AddColumn("Target")
                .AddColumn("Test Classes", x => x.Alignment = Justify.Right)
                .AddColumn("Test Methods", x => x.Alignment = Justify.Right)
                .AddColumn("Path")
                ;

        var i = 0;
        var sumTestClasses = 0L;
        var sumTestMethods = 0L;
        foreach (var x in data)
        {
            try
            {
                var countTestClasses = x.Tests.Length;
                sumTestClasses += countTestClasses;

                var countTestMethods = x.Tests.Sum(x => x.methods.Length);
                sumTestMethods += countTestMethods;

                var row = new List<IRenderable> {
                    new Text($"{++i}"),
                    new Text(x.Name),
                    new Text(x.Version),
                    new Text(x.Config),
                    new Text(x.Target),
                    new Text($"{countTestClasses:N0}"),
                    new Text($"{countTestMethods:N0}"),
                    new Text($"{x.Assembly.Location}"),
                    };

                table.AddRow(row);
            }
            catch /*(Exception e)*/
            {
                //Console.WriteLine($"[ERROR] test discovery failed for {assembly.FullName}");
                //Console.WriteLine(e);
                //Console.ReadLine();
            }
        }

        {
            // summary row
            var summary = new List<IRenderable> {
                new Text(""),
                new Text("Total"),
                new Text(""),
                new Text(""),
                new Text(""),
                new Text($"{sumTestClasses:N0}"),
                new Text($"{sumTestMethods:N0}"),
                new Text(""),
                };

            table.AddEmptyRow();
            table.AddRow(summary);
        }

        AnsiConsole.Write(table);

        return 0;
    }
}
