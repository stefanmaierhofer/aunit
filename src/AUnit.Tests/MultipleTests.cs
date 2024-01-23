/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

#pragma warning disable CA1822 // Mark members as static

namespace AUnit.Tests;

internal class MultipleTests
{
    [Test]
    public void Multiple()
    {
        Assert.Multiple(() =>
        {
            Assert.IsTrue(true);
            Assert.IsFalse(false);
        });
    }

    [Test]
    public void MultipleThrows()
    {
        Assert.Throws(() =>
        {
            Assert.Multiple(() =>
            {
                Assert.IsTrue(false); // fails
                Assert.IsFalse(false);
            });
        });
    }
}
