/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

using aunit;
using AUnit;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config =>
{
    config
        .SetApplicationName("au")
        .SetApplicationVersion(Globals.Version)
        .TrimTrailingPeriods(trimTrailingPeriods: false)
        .CaseSensitivity(CaseSensitivity.None)
        .ValidateExamples()
        ;

    config
        .AddCommand<RunCommand>("run")
        .WithDescription("Run tests.")
        .WithExample(["run"])
        .WithExample(["run", "MySolution.sln"])
        .WithExample(["run", "SomeDll.dll"])
        .WithExample(["run", "a/path/to/search/for/dlls"])
        ;

    config
        .AddCommand<ListCommand>("list")
        .WithAlias("ls")
        .WithDescription("List tests.")
        .WithExample(["list"])
        .WithExample(["list", "MySolution.sln"])
        .WithExample(["list", "SomeDll.dll"])
        .WithExample(["list", "a/path/to/search/for/dlls"])
        ;
});

return app.Run(args);
