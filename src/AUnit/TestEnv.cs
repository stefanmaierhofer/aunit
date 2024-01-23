/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AUnit;

public class TestEnv
{
    public static TestEnv Empty => new();

    private readonly List<TestPerfInfo> _perfs = [];
    public IReadOnlyList<TestPerfInfo> Perfs => _perfs;

    public TestPerfInfo Time(int warmupRounds, int timingRounds, Action f)
        => Time(null, warmupRounds, timingRounds, avgAlert: -1.0, f);

    public TestPerfInfo Time(string displayName, int warmupRounds, int timingRounds, Action f)
        => Time(displayName, warmupRounds, timingRounds, avgAlert: -1.0, f);

    public TestPerfInfo Time(int warmupRounds, int timingRounds, double avgAlert, Action f)
        => Time(null, warmupRounds, timingRounds, avgAlert, f);

    public TestPerfInfo Time(string? displayName, int warmupRounds, int timingRounds, double avgAlert, Action f)
    {
        for (var i = 0; i < warmupRounds; i++) f();

        var times = new double[timingRounds];
        var sw = new Stopwatch();
        for (var i = 0; i < timingRounds; i++)
        {
            sw.Restart();
            f();
            sw.Stop();
            times[i] = sw.Elapsed.TotalSeconds;
        }

        var perfInfo = TestPerfInfo.Create(displayName, warmupRounds, timingRounds, avgAlert, times);
        _perfs.Add(perfInfo);
        return perfInfo;
    }

    public Task<TestPerfInfo> TimeAsync(string displayName, int warmupRounds, int timingRounds, Func<Task> f)
        => TimeAsync(displayName, warmupRounds, timingRounds, avgAlert: -1.0, f);

    public async Task<TestPerfInfo> TimeAsync(string displayName, int warmupRounds, int timingRounds, double avgAlert, Func<Task> f)
    {
        for (var i = 0; i < warmupRounds; i++) await f();

        var times = new double[timingRounds];
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < timingRounds; i++)
        {
            sw.Restart();
            await f();
            sw.Stop();
            times[i] = sw.Elapsed.TotalSeconds;
        }

        var perfInfo = TestPerfInfo.Create(displayName, warmupRounds, timingRounds, avgAlert, times);
        _perfs.Add(perfInfo);
        return perfInfo;
    }
}