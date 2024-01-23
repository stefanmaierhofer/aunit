/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

#pragma warning disable CA1822 // Mark members as static

namespace AUnit.Tests;

internal class TrueFalseTests
{
    [Test]
    public void IsTrue()
    {
        Assert.IsTrue(true);
    }

    [Test]
    public void IsTrueThrows()
    {
        Assert.Throws(() =>
        {
            Assert.IsTrue(false);
        });
    }

    [Test]
    public void True()
    {
        Assert.True(true);
    }

    [Test]
    public void TrueThrows()
    {
        Assert.Throws(() =>
        {
            Assert.IsTrue(false);
        });
    }

    [Test]
    public void That()
    {
        Assert.That(true);
    }

    [Test]
    public void ThatThrows()
    {
        Assert.Throws(() =>
        {
            Assert.That(false);
        });
    }

    [Test]
    public void IsFalse()
    {
        Console.WriteLine("some console output");
        Assert.IsFalse(false);
    }

    [Test]
    public void IsFalseThrows()
    {
        Assert.Throws(() =>
        {
            Assert.IsFalse(true);
        });
    }

    [Test]
    public void False()
    {
        Assert.False(false);
    }

    [Test]
    public void FalseThrows()
    {
        Assert.Throws(() =>
        {
            Assert.False(true);
        });
    }
}
