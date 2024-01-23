/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

#pragma warning disable CA1822 // Mark members as static

namespace AUnit.Tests;

internal class FailTests
{
    [Test]
    [ExpectedStatus(TestStatus.Failed)]
    public void Fail()
    {
        Assert.Fail("this should fail");
    }

    [Test]
    [ExpectedStatus(TestStatus.Failed)]
    public void FailNestedThrows()
    {
        Assert.Throws(() =>
        {
            Assert.Fail("this should fail");
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Failed)]
    public void FailNestedThrowsGeneric()
    {
        Assert.Throws<Exception>(() =>
        {
            Assert.Fail("this should fail");
            throw new Exception();
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Failed)]
    public void FailNestedThrowsGenericFailException()
    {
        Assert.Throws<FailException>(() =>
        {
            Assert.Fail("this should fail");
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Failed)]
    public async Task FailNestedThrowsAsync()
    {
        await Assert.ThrowsAsync(async () =>
        {
            await Task.Delay(0);
            Assert.Fail("this should fail");
        });
    }


    [Test]
    [ExpectedStatus(TestStatus.Failed)]
    public async Task FailNestedThrowsAsyncGeneric()
    {
        await Assert.ThrowsAsync<Exception>(async  () =>
        {
            await Task.Delay(0);
            Assert.Fail("this should fail");
            throw new Exception();
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Failed)]
    public async Task FailNestedThrowsAsyncGenericFailException()
    {
        await Assert.ThrowsAsync<FailException>(async  () =>
        {
            await Task.Delay(0);
            Assert.Fail("this should fail");
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Failed)]
    public void FailNestedCatch()
    {
        Assert.Catch<Exception>(() =>
        {
            Assert.Fail("this should fail");
            throw new Exception();
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Failed)]
    public async Task FailNestedCatchAsync()
    {
        await Assert.CatchAsync<Exception>(async () =>
        {
            await Task.Delay(0);
            Assert.Fail("this should fail");
            throw new Exception();
        });
    }

    [Test]
    [ExpectedStatus(TestStatus.Failed)]
    public async Task FailDeeplyNested()
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
                                        Assert.Fail("this should fail");
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
