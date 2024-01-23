/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

#pragma warning disable CA1822 // Mark members as static

namespace AUnit.Tests;

internal class InconclusiveTests
{
    [Test]
    [ExpectedStatus(TestStatus.Inconclusive)]
    public void Inconclusive()
    {
        Assert.Inconclusive("this should be inconclusive");
    }

    [Test]
    [ExpectedStatus(TestStatus.Inconclusive)]
    public void InconclusiveNestedThrows()
    {
        Assert.Throws(() =>
        {
            Assert.Inconclusive("this should be inconclusive");
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Inconclusive)]
    public void InconclusiveNestedThrowsGeneric()
    {
        Assert.Throws<Exception>(() =>
        {
            Assert.Inconclusive("this should be inconclusive");
            throw new Exception();
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Inconclusive)]
    public void InconclusiveNestedThrowsGenericFailException()
    {
        Assert.Throws<FailException>(() =>
        {
            Assert.Inconclusive("this should be inconclusive");
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Inconclusive)]
    public async Task InconclusiveNestedThrowsAsync()
    {
        await Assert.ThrowsAsync(async () =>
        {
            await Task.Delay(0);
            Assert.Inconclusive("this should be inconclusive");
        });
    }


    [Test]
    [ExpectedStatus(TestStatus.Inconclusive)]
    public async Task InconclusiveNestedThrowsAsyncGeneric()
    {
        await Assert.ThrowsAsync<Exception>(async  () =>
        {
            await Task.Delay(0);
            Assert.Inconclusive("this should be inconclusive");
            throw new Exception();
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Inconclusive)]
    public async Task InconclusiveNestedThrowsAsyncGenericFailException()
    {
        await Assert.ThrowsAsync<FailException>(async  () =>
        {
            await Task.Delay(0);
            Assert.Inconclusive("this should be inconclusive");
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Inconclusive)]
    public void InconclusiveNestedCatch()
    {
        Assert.Catch<Exception>(() =>
        {
            Assert.Inconclusive("this should be inconclusive");
            throw new Exception();
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Inconclusive)]
    public async Task InconclusiveNestedCatchAsync()
    {
        await Assert.CatchAsync<Exception>(async () =>
        {
            await Task.Delay(0);
            Assert.Inconclusive("this should be inconclusive");
            throw new Exception();
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Inconclusive)]
    public async Task InconclusiveDeeplyNested()
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
                                        Assert.Inconclusive("this should be inconclusive");
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
