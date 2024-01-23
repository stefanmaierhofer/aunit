/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

#pragma warning disable CA1822 // Mark members as static

namespace AUnit.Tests;

internal class IgnoreTests
{
    [Test]

    [ExpectedStatus(TestStatus.Ignored)]
    public void Ignore()
    {
        Assert.Ignore("this should be ignored");
    }

    [Test]
    [ExpectedStatus(TestStatus.Ignored)]
    public void IgnoreNestedThrows()
    {
        Assert.Throws(() =>
        {
            Assert.Ignore("this should be ignored");
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Ignored)]
    public void IgnoreNestedThrowsGeneric()
    {
        Assert.Throws<Exception>(() =>
        {
            Assert.Ignore("this should be ignored");
            throw new Exception();
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Ignored)]
    public void IgnoreNestedThrowsGenericFailException()
    {
        Assert.Throws<FailException>(() =>
        {
            Assert.Ignore("this should be ignored");
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Ignored)]
    public async Task IgnoreNestedThrowsAsync()
    {
        await Assert.ThrowsAsync(async () =>
        {
            await Task.Delay(0);
            Assert.Ignore("this should be ignored");
        });
    }


    [Test]
    [ExpectedStatus(TestStatus.Ignored)]
    public async Task IgnoreNestedThrowsAsyncGeneric()
    {
        await Assert.ThrowsAsync<Exception>(async  () =>
        {
            await Task.Delay(0);
            Assert.Ignore("this should be ignored");
            throw new Exception();
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Ignored)]
    public async Task IgnoreNestedThrowsAsyncGenericFailException()
    {
        await Assert.ThrowsAsync<FailException>(async  () =>
        {
            await Task.Delay(0);
            Assert.Ignore("this should be ignored");
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Ignored)]
    public void IgnoreNestedCatch()
    {
        Assert.Catch<Exception>(() =>
        {
            Assert.Ignore("this should be ignored");
            throw new Exception();
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Ignored)]
    public async Task IgnoreNestedCatchAsync()
    {
        await Assert.CatchAsync<Exception>(async () =>
        {
            await Task.Delay(0);
            Assert.Ignore("this should be ignored");
            throw new Exception();
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Ignored)]
    public async Task IgnoreDeeplyNested()
    {
        await Assert.CatchAsync<Exception>(async () =>
        {
            await Task.Delay(0);
            await Assert.ThrowsAsync<NotImplementedException>(async () =>
            {
                await Task.Delay(0);
                await Assert.ThrowsAsync<Exception>(async () =>
                {
                    await Task.Delay(0);
                    await Assert.ThrowsAsync(async () =>
                    {
                        await Task.Delay(0);
                        Assert.Catch<Exception>(() =>
                        {
                            Assert.Throws<NotImplementedException>(() =>
                            {
                                Assert.Throws<Exception>(() =>
                                {
                                    Assert.Throws(() =>
                                    {
                                        Assert.Ignore("this should be ignored");
                                        throw new Exception();
                                    });
                                    throw new Exception();
                                });
                                throw new NotImplementedException();
                            });
                            throw new Exception();
                        });
                        throw new Exception();
                    });
                    throw new Exception();
                });
                throw new NotImplementedException();
            });
            throw new NotImplementedException();
        });
    }
}
