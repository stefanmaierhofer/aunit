/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

#pragma warning disable CA1822 // Mark members as static

namespace AUnit.Tests;

internal class PerfTests
{
    [Test]
    public void Time(TestEnv env)
    {
        var info = env.Time("test 1", 1, 2, () =>
        {
            Task.Delay(110).Wait();
        });

        Assert.True(info.Sum > 0.2 && info.Sum < 0.3, $"Expected timing in range [0.2, 0.3], but found {info.Sum}.");
    }

    [Test]
    public async Task TimeAsync(TestEnv env)
    {
        var info = await env.TimeAsync("test 2", 1, 2, async () =>
        {
            await Task.Delay(110);
        });

        Assert.True(info.Sum > 0.2 && info.Sum < 0.3);
    }

    [Test, DataSource(nameof(IntData))]
    public void TimeWithDataSource(TestEnv env, int i)
    {
        var info = env.Time("test 1", 1, 2, () =>
        {
            Task.Delay(i * 10).Wait();
        });
    }

    [Test, DataSource(nameof(IntData))]
    public async Task TimeAsyncWithDataSource(TestEnv env, int i)
    {
        var info = await env.TimeAsync("test 2", 1, 2, async () =>
        {
            await Task.Delay(i * 10);
        });
    }

    static int[] IntData() => new[] { 2, 5, 7 };
}
