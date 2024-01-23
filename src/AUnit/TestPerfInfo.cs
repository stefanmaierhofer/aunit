/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

using System;

namespace AUnit;

public record TestPerfInfo(
    string? DisplayName,
    int WarmupRounds,
    int TimingRounds,
    double AvgAlert,
    double Min,
    double Max,
    double Avg,
    double StdDev,
    double Sum,
    double[] SamplesInSeconds
    )
{
    public bool IsSlow => AvgAlert >= 0.0 && Avg > AvgAlert;

    public static TestPerfInfo Create(int warmupRounds, int timingRounds, double avgAlert, double[] durationsInSeconds)
        => Create(null, warmupRounds, timingRounds, avgAlert, durationsInSeconds);

    public static TestPerfInfo Create(string? displayName, int warmupRounds, int timingRounds, double avgAlert, double[] durationsInSeconds)
    {
        var n = durationsInSeconds.Length;
        double min = double.MaxValue;
        double max = double.MinValue;
        double sum = 0.0;
        for (var i = 0; i < n; i++)
        {
            var x = durationsInSeconds[i];
            if (x < min) min = x;
            if (x > max) max = x;
            sum += x;
        }

        double avg = sum / n;

        var stddev = 0.0;
        for (var i = 0; i < n; i++)
        {
            var d = durationsInSeconds[i] - avg;
            stddev += d * d;
        }
        stddev = Math.Sqrt(stddev / n);

        return new(displayName, warmupRounds, timingRounds, avgAlert, min, max, avg, stddev, sum, [.. durationsInSeconds]);
    }
}