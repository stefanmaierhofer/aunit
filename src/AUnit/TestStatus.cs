/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

using System;

namespace AUnit;

[Flags]
public enum TestStatus
{
    Undefined = 0,

    Ok = 1,
    Failed = 2,
    Ignored = 4,
    Inconclusive = 8,
    Slow = 16,

    /// <summary>
    /// If combined with failed, ignored, inconclusive or slow, then the test will be counted as Ok.
    /// Used to test the test framework itself.
    /// E.g. a test that tests if a test fails, should be counted as ok if it fails.  ;-)
    /// </summary>
    Expected = 256
}
