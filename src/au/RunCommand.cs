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
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace aunit;

public class RunCommandSettings : CommandSettings
{
}

public class RunCommand : AsyncCommand<RunCommand.Settings>
{
    public class Settings : RunCommandSettings
    {
        [Description("Specific directories, solution files, or assemblies to search tests in. ")]
        [CommandArgument(0, "[PATH*]")]
        public string[] Paths { get; set; } = [];

        [Description("Only tests assemblies with this build configuration, e.g. Debug or Release. ")]
        [CommandOption("-c|--config <CONFIG>")]
        public string[] Configurations { get; set; } = [];

        [Description("Saves test results to FILE. ")]
        [CommandOption("-o|--output <FILE>")]
        public string? Output { get; set; }
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        // if user specifies no paths then we use current directory ...
        var paths = settings.Paths;
        if (paths.Length == 0) paths = ["."];

        // discover assemblies ...
        var assembliesToTest = Discovery.EnumerateAssemblyFiles(paths);

        // optionally filter by config (if specified) ...
        if (settings.Configurations.Length > 0)
        {
            var configs = new HashSet<string>(settings.Configurations);
            assembliesToTest = assembliesToTest
                .Where(x =>
                {
                    try
                    {
                        var a = x.GetAssembly();
                        var c = a.GetAssemblyConfiguration();
                        if (c == null) return false;
                        var result = configs.Contains(c);
                        return result;
                    }
                    catch
                    {
                        return false;
                    }
                })
                .ToImmutableList()
                ;
        }

        // run tests ...
        var protocol = await CreateWithLivePrintAsync(assembliesToTest.Select(x => x.GetAssembly()));

        var now = DateTimeOffset.UtcNow;

        // save output to file ...
        {
            var logFileName =
                settings.Output
                ?? $"./testlog_{now.Year:0000}-{now.Month:00}-{now.Day:00}_{now.Hour:00}{now.Minute:00}{now.Second}_{now.ToUnixTimeMilliseconds()}.json";
            ;
            if (!Path.GetExtension(logFileName).Equals(".json", StringComparison.CurrentCultureIgnoreCase))
            {
                logFileName += ".json";
            }

            var json = JsonSerializer.Serialize(protocol, _jsonOptions);
            File.WriteAllText(logFileName, json);
        }

        return 0;
    }

    public static async Task<TestProtocol> CreateWithLivePrintAsync(IEnumerable<Assembly> assemblies, CancellationToken ct = default)
    {
        var tests = new List<(Type type, MethodInfo[] methods)>();

        foreach (var assembly in assemblies)
        {
            try
            {
                var xs = TestRunner.DiscoverTests(assembly).ToArray();
                tests.AddRange(xs);
            }
            catch
            {
            }
        }

        var countTypes = tests.Count;
        var countTests = tests.Sum(x => x.methods.Length);
        if (countTypes == 0) return TestProtocol.Empty;

        var allResults = Concat(tests.Select(x => TestRunner.RunAsync(x.type, ct)), ct);
        return await CreateWithLivePrintAsync(allResults, ct);

        static async IAsyncEnumerable<TestResult> Concat(IEnumerable<IAsyncEnumerable<TestResult>> xss, [EnumeratorCancellation] CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            foreach (var xs in xss)
            {
                ct.ThrowIfCancellationRequested();
                await foreach (var x in xs)
                {
                    ct.ThrowIfCancellationRequested();
                    yield return x;
                }
            }
        }
    }

    public static async Task<TestProtocol> CreateWithLivePrintAsync(IAsyncEnumerable<TestResult> results, CancellationToken ct = default)
    {
        var protocol = TestProtocol.Create();

        var table = new Table()
            .AddColumn("#", x => x.Alignment = Justify.Right)
            .AddColumn("Assembly")
            .AddColumn("Version")
            .AddColumn("Config")
            .AddColumn("Framework")
            .AddColumn("Class")
            .AddColumn("Test")
            .AddColumn("Data")
            .AddColumn("Elapsed [[ms]]", x => x.Alignment = Justify.Right)
            .AddColumn("Status")
            .AddColumn("Message")
            ;

        await AnsiConsole
            .Live(table)
            .StartAsync(async ctx =>
            {
                var count = 0;
                var lastTestType = "";
                var lastTestMethod = "";

                await foreach (var _result in results)
                {
                    ct.ThrowIfCancellationRequested();

                    var result = (_result.IsSlow) ? _result with { Status = TestStatus.Slow } : _result;

                    protocol = protocol.Add(result);

                    var testType = result.TestType?.FullName;
                    if (testType == lastTestType) testType = ""; else lastTestType = testType;

                    var testMethod = result.TestMethod.Name;
                    if (testMethod == lastTestMethod) testMethod = ""; else lastTestMethod = testMethod;

                    var isExpected = (result.Status & TestStatus.Expected) == TestStatus.Expected;
                    var statusText = (result.Status & ~TestStatus.Expected, isExpected) switch
                    {
                        (TestStatus.Ok, _) => "[green]ok[/]",
                        (TestStatus.Failed, false) => "[red]failed[/]",
                        (TestStatus.Ignored, false) => "[cyan]ignored[/]",
                        (TestStatus.Inconclusive, false) => "[yellow]inconcl[/]",
                        (TestStatus.Slow, false) => "[orange1]slow[/]",

                        (TestStatus.Failed, true) => "[green]failed (expected)[/]",
                        (TestStatus.Ignored, true) => "[green]ignored (expected)[/]",
                        (TestStatus.Inconclusive, true) => "[green]inconcl (expected)[/]",
                        (TestStatus.Slow, true) => "[green]slow (expected)[/]",

                        _ => $"[red]unknown status {result.Status}[/]"
                    };

                    if (
                        result.Exception != null &&
                        result.Exception is not FailException &&
                        result.Exception is not PassException &&
                        result.Exception is not IgnoredException &&
                        result.Exception is not InconclusiveException
                        && !result.Status.IsExpected()
                        )
                    {
                        statusText += $"\n\n[red]{Markup.Escape(result.Exception.ToString())}[/]";

                        if (!string.IsNullOrWhiteSpace(result.ConsoleOutput))
                        {
                            statusText += $"\n\n[gray]{Markup.Escape(result.ConsoleOutput.ToString())}[/]";
                        }
                    }

                    var messages = new List<IRenderable>();
                    if (result.Message != null) messages.Add(new Text(result.Message));

                    foreach (var x in result.Env.Perfs)
                    {
                        string unit;
                        Func<double, double> scale;

                        if (x.Avg < 0.000001) { unit = "[[ns]]"; scale = sec => Math.Round(sec * 1000000000.0, 3); }
                        else if (x.Avg < 0.001) { unit = "[[µs]]"; scale = sec => Math.Round(sec * 1000000.0, 3); }
                        else if (x.Avg < 1.0) { unit = "[[ms]]"; scale = sec => Math.Round(sec * 1000.0, 3); }
                        else { unit = "[[s]] "; scale = sec => Math.Round(sec, 3); }

                        var label = $"[blue]{unit}[/] {x.TimingRounds} rounds ({x.WarmupRounds} warmup)";
                        var chart = new BarChart()
                            .AddItem("min", scale(x.Min))
                            .AddItem("avg", scale(x.Avg))
                            .AddItem("max", scale(x.Max))
                            .AddItem("σ", scale(x.StdDev))
                            ;

                        messages.Add(new Markup(label));
                        messages.Add(chart);
                    }

                    table.AddRow(
                        new Text($"{++count}"),
                        new Text(result.TestAssembly?.GetName()?.Name ?? "-"),
                        new Text(result.AssemblyVersion),
                        new Text(result.Configuration),
                        new Text(result.TargetFramework),
                        new Text(testType ?? "-"),
                        new Text(testMethod ?? "-"),
                        new Text(result.SourceData ?? ""),
                        new Text($"{result.Elapsed.TotalMilliseconds:N0}"),
                        new Markup(statusText),
                        messages.Count > 0 ? new Rows(messages) : new Text("")
                        );

                    ctx.Refresh();
                }
            });

        protocol = protocol.Finish();

        AnsiConsole.WriteLine($"started : {protocol.Started.ToLocalTime():G}");
        AnsiConsole.WriteLine($"finished: {protocol.Finished.ToLocalTime():G}");
        AnsiConsole.WriteLine($"elapsed : {protocol.TotalDuration}");
        AnsiConsole.WriteLine();

        int padLength = "Inconclusive".Length;
        var barChart = new BarChart()
            .Width(78)
            .AddItem("Total".PadRight(padLength), protocol.Counts.Total)
            .AddItem("Ok".PadRight(padLength), protocol.Counts.Ok, Color.Green)
            .AddItem("Ok (slow!)".PadRight(padLength), protocol.Counts.Slow, Color.Orange1)
            .AddItem("Failed".PadRight(padLength), protocol.Counts.Failed, Color.Red)
            .AddItem("Inconclusive".PadRight(padLength), protocol.Counts.Inconclusive, Color.Yellow)
            .AddItem("Ignored".PadRight(padLength), protocol.Counts.Ignored, Color.Cyan3)
            ;

        AnsiConsole.Write(barChart);

        return protocol;
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(),
        },
    };
}
