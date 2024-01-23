/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

#pragma warning disable CA1822 // Mark members as static

namespace AUnit.Tests;

internal class NullEmptyTests
{
    [Test]
    public void IsNull()
    {
        Assert.IsNull(null);
    }

    [Test]
    public void IsNullThrows()
    {
        Assert.Throws(() =>
        {
            Assert.IsNull("foo");
        });
    }

    [Test]
    public void IsNotNull()
    {
        Assert.IsNotNull("foo");
    }

    [Test]
    public void IsNotNullThrows()
    {
        Assert.Throws(() =>
        {
            Assert.IsNotNull(null);
        });
    }


    [Test]
    public void IsNullOrEmpty()
    {
        Assert.IsNullOrEmpty((string?)null);
        Assert.IsNullOrEmpty("");
    }

    [Test]
    public void IsNullOrEmptyThrows()
    {
        Assert.Throws(() =>
        {
            Assert.IsNullOrEmpty("foo");
        });
    }


    [Test]
    public void IsNotNullOrEmpty()
    {
        Assert.IsNotNullOrEmpty("foo");
    }

    [Test]
    public void IsNotNullOrEmptyThrows()
    {
        Assert.Throws(() =>
        {
            Assert.IsNotNullOrEmpty((string?)null);
        });
        
        Assert.Throws(() =>
        {
            Assert.IsNotNullOrEmpty("");
        });
    }

    [Test]
    public void IsEmpty()
    {
        Assert.IsEmpty((int[]?)null); 
        Assert.IsEmpty(Array.Empty<int>());
    }

    [Test]
    public void IsEmptyThrows()
    {
        int[]? xs = new[] { 1 };
        Assert.Throws(() =>
        {
            Assert.IsEmpty(xs);
        });
    }

    [Test]
    public void IsNotEmpty()
    {
        int[]? xs = new[] { 1 };
        Assert.IsNotEmpty(xs);
    }

    [Test]
    public void IsNotEmptyThrows()
    {
        Assert.Throws(() =>
        {
            Assert.IsNotEmpty((int[]?)null);
        });

        Assert.Throws(() =>
        {
            Assert.IsNotEmpty(Array.Empty<int>());
        });
    }
}
