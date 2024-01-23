/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

using System.Reflection;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0079
#pragma warning disable IL3000

namespace AUnit;

public record Counts(
    long Total,
    long Ok,
    long Failed,
    long Ignored,
    long Inconclusive,
    long Slow
    )
{
    public static readonly Counts Zero = new(0L, 0L, 0L, 0L, 0L, 0L);

    public Counts Add(TestStatus status)
    {
        if ((status & TestStatus.Expected) == TestStatus.Expected)
        {
            return this with { Total = Total + 1, Ok = Ok + 1 };
        }
        else
        {
            return status switch
            {
                TestStatus.Failed => this with { Total = Total + 1, Failed = Failed + 1 },
                TestStatus.Ignored => this with { Total = Total + 1, Ignored = Ignored + 1 },
                TestStatus.Inconclusive => this with { Total = Total + 1, Inconclusive = Inconclusive + 1 },
                TestStatus.Ok => this with { Total = Total + 1, Ok = Ok + 1 },
                TestStatus.Slow => this with { Total = Total + 1, Slow = Slow + 1 },
                _ => throw new Exception($"Unknown status \"\". Error 2bec80bf-8904-4c6b-94dc-f1433fc842ef.")
            };
        }
    }
}

public partial record TestProtocol(
    DateTimeOffset Started,
    DateTimeOffset Finished,
    TimeSpan TotalDuration,
    Counts Counts,
    IReadOnlyList<TestResultDto> Tests
    )
{

    public static readonly TestProtocol Empty = Create();

    public static TestProtocol Create() => new(
        Started: DateTimeOffset.UtcNow,
        Finished: DateTimeOffset.MinValue,
        TotalDuration: TimeSpan.Zero,
        Counts: Counts.Zero,
        Tests: []
        );

    public TestProtocol Add(TestResult result)
    {
        var newTests = new List<TestResultDto>(Tests)
        {
            result.ToDto()
        };
        return this with { Tests = newTests, Counts = Counts.Add(result.Status) };
    }

    public TestProtocol Finish()
    {
        var now = DateTimeOffset.UtcNow;
        return this with { Finished = now, TotalDuration = now - Started };
    }
}

public partial record TestProtocol
{
    public static Task<TestProtocol> CreateAsync(Type type,  CancellationToken ct = default)
    {
        var xs = TestRunner.DiscoverTests(type);
        var countTypes = xs.Length;
        var countTests = xs.Sum(x => x.methods.Length);
        
        var results = TestRunner.RunAsync(type, ct);
        return CreateAsync(results, ct);
    }

    public static Task<TestProtocol> CreateAsync(Assembly assembly, CancellationToken ct = default)
    {
        var xs = TestRunner.DiscoverTests(assembly).ToArray();
        var countTypes = xs.Length;
        var countTests = xs.Sum(x => x.methods.Length);
        if (countTypes == 0) return Task.FromResult(Empty);

        var results = TestRunner.RunAsync(assembly, ct);
        return CreateAsync(results, ct);
    }

    public static Task<TestProtocol> CreateAsync(IEnumerable<AssemblyContext> assemblies, CancellationToken ct = default)
        => CreateAsync(assemblies.Select(x => x.GetAssembly()), ct);

    public static async Task<TestProtocol> CreateAsync(IEnumerable<Assembly> assemblies, CancellationToken ct = default)
    {
        var tests = new List<(Type type, MethodInfo[] methods)>();

        foreach (var assembly in assemblies)
        {
            try
            {
                var xs = TestRunner.DiscoverTests(assembly).ToArray();
                if (xs.Length > 0)
                {
                    Console.WriteLine($"[TestProtocol.CreateAsync] found {xs.Length} tests ({assembly.Location})");
                    //foreach (var x in xs)
                    //{
                    //    foreach (var m in x.methods)
                    //    {
                    //        Console.WriteLine($"[TestProtocol.CreateAsync]     {x.type.AssemblyQualifiedName}   {m.Name}");
                    //    }
                    //}
                }
                tests.AddRange(xs);
            }
            catch /*(Exception e)*/
            {
                //Console.WriteLine($"[ERROR] test discovery failed for {assembly.FullName}");
                //Console.WriteLine(e);
                //Console.ReadLine();
            }
        }

        var countTypes = tests.Count;
        var countTests = tests.Sum(x => x.methods.Length);
        if (countTypes == 0) return Empty;

        var allResults = Concat(tests.Select(x => TestRunner.RunAsync(x.type, ct)), ct);
        return await CreateAsync(allResults, ct);

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

    public static async Task<TestProtocol> CreateAsync(IAsyncEnumerable<TestResult> results, CancellationToken ct = default)
    {
        var protocol = Create();

        await foreach (var _result in results)
        {
            ct.ThrowIfCancellationRequested();
            var result = (_result.IsSlow) ? _result with { Status = TestStatus.Slow } : _result;
            protocol = protocol.Add(result);
        }

        protocol = protocol.Finish();

        return protocol;
    }
}