/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

#pragma warning disable CA1822 // Mark members as static

namespace AUnit.Tests;

internal class PassTests
{
    [Test]
    public void Pass()
    {
        Assert.Pass("this should pass");
    }

    [Test]
    public void PassNestedThrows()
    {
        Assert.Throws(() =>
        {
            Assert.Pass("this should pass");
        });
    }

    [Test]
    public void PassNestedThrowsGeneric()
    {
        Assert.Throws<Exception>(() =>
        {
            Assert.Pass("this should pass");
            throw new Exception();
        });
    }

    [Test]
    public void PassNestedThrowsGenericFailException()
    {
        Assert.Throws<FailException>(() =>
        {
            Assert.Pass("this should pass");
        });
    }

    [Test]
    public async Task PassNestedThrowsAsync()
    {
        await Assert.ThrowsAsync(async () =>
        {
            await Task.Delay(0);
            Assert.Pass("this should pass");
        });
    }


    [Test]
    public async Task PassNestedThrowsAsyncGeneric()
    {
        await Assert.ThrowsAsync<Exception>(async  () =>
        {
            await Task.Delay(0);
            Assert.Pass("this should pass");
            throw new Exception();
        });
    }

    [Test]
    public async Task PassNestedThrowsAsyncGenericFailException()
    {
        await Assert.ThrowsAsync<FailException>(async  () =>
        {
            await Task.Delay(0);
            Assert.Pass("this should pass");
        });
    }

    [Test]
    public void PassNestedCatch()
    {
        Assert.Catch<Exception>(() =>
        {
            Assert.Pass("this should pass");
            throw new Exception();
        });
    }

    [Test]
    public async Task PassNestedCatchAsync()
    {
        await Assert.CatchAsync<Exception>(async () =>
        {
            await Task.Delay(0);
            Assert.Pass("this should pass");
            throw new Exception();
        });
    }

    [Test]
    public async Task PassDeeplyNested()
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
                                        Assert.Pass("this should pass");
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
