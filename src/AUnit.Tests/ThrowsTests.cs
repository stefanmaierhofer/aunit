/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

#pragma warning disable CA1822 // Mark members as static

namespace AUnit.Tests;

internal class ThrowsTests
{
    #region Throws

    [Test]
    public void Throws1()
    {
        var e = Assert.Throws(() =>
        {
            throw new Exception();
        });

        Assert.That(e.GetType() == typeof(Exception));
    }

    [Test]
    public void Throws2()
    {
        var e = Assert.Throws(() =>
        {
            throw new NotImplementedException();
        });

        Assert.That(e.GetType() == typeof(NotImplementedException));
    }

    [Test]
    public void ThrowsGeneric()
    {
        var e = Assert.Throws<Exception>(() =>
        {
            throw new Exception();
        });

        Assert.That(e.GetType() == typeof(Exception));
    }

    [Test]
    public void ThrowsGenericWrongType()
    {
        Assert.Throws(() =>
        {
            Assert.Throws<Exception>(() =>
            {
                throw new NotImplementedException();
            });
        });
    }

    #endregion

    #region ThrowsAsync

    [Test]
    public async Task ThrowsAsync1()
    {
        var e = await Assert.ThrowsAsync(async () =>
        {
            await Task.Delay(0);
            throw new Exception();
        });

        Assert.That(e.GetType() == typeof(Exception));
    }

    [Test]
    public async Task ThrowsAsync2()
    {
        var e = await Assert.ThrowsAsync(async () =>
        {
            await Task.Delay(0);
            throw new NotImplementedException();
        });

        Assert.That(e.GetType() == typeof(NotImplementedException));
    }

    [Test]
    public async Task ThrowsAsyncGeneric()
    {
        var e = await Assert.ThrowsAsync<Exception>(async () =>
        {
            await Task.Delay(0);
            throw new Exception();
        });

        Assert.That(e.GetType() == typeof(Exception));
    }

    [Test]
    public async Task ThrowsAsyncGenericWrongType()
    {
        await Assert.ThrowsAsync(async () =>
        {
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await Task.Delay(0);
                throw new NotImplementedException();
            });
        });
    }

    #endregion

    #region Catch

    [Test]
    public void Catch()
    {
        var e = Assert.Catch<Exception>(() =>
        {
            throw new Exception();
        });

        Assert.That(e.GetType() == typeof(Exception));
    }

    [Test]
    public void CatchSubType()
    {
        var e = Assert.Catch<Exception>(() =>
        {
            throw new NotImplementedException();
        });

        Assert.That(e.GetType() == typeof(NotImplementedException));
    }

    [Test]
    public void CatchGenericWrongType()
    {
        Assert.Throws(() =>
        {
            Assert.Catch<NotImplementedException>(() =>
            {
                throw new Exception();
            });
        });
    }

    #endregion

    #region CatchAsync

    [Test]
    public async Task CatchAsync()
    {
        var e = await Assert.CatchAsync<Exception>(async () =>
        {
            await Task.Delay(0);
            throw new Exception();
        });

        Assert.That(e.GetType() == typeof(Exception));
    }

    [Test]
    public async Task CatchAsyncSubType()
    {
        var e = await Assert.CatchAsync<Exception>(async () =>
        {
            await Task.Delay(0);
            throw new NotImplementedException();
        });

        Assert.That(e.GetType() == typeof(NotImplementedException));
    }

    [Test]
    public async Task CatchAsyncGenericWrongType()
    {
        await Assert.ThrowsAsync(async () =>
        {
            await Assert.CatchAsync<NotImplementedException>(async () =>
            {
                await Task.Delay(0);
                throw new Exception();
            });
        });
    }

    #endregion

    #region DoesNotThrow

    [Test]
    public void DoesNotThrow()
    {
        Assert.DoesNotThrow(() =>
        {
            Console.WriteLine("does not throw");
        });
    }

    [Test]
    public void DoesNotThrowThrows()
    {
        Assert.Throws(() =>
        {
            Assert.DoesNotThrow(() =>
            {
                throw new NotImplementedException();
            });
        });
    }

    #endregion

    #region DoesNotThrowAsync

    [Test]
    public async Task DoesNotThrowAsync()
    {
        await Assert.DoesNotThrowAsync(async () =>
        {
            await Task.Delay(0);
            Console.WriteLine("does not throw");
        });
    }

    [Test]
    public async Task DoesNotThrowAsyncThrows()
    {
        await Assert.ThrowsAsync(async () =>
        {
            await Assert.DoesNotThrowAsync(async () =>
            {
                await Task.Delay(0);
                throw new NotImplementedException();
            });
        });
    }

    #endregion
}
