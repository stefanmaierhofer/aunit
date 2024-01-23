/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

using System;

namespace AUnit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TestAttribute : Attribute
{
    public string[] Tags;

    public TestAttribute()
    {
        Tags = [];
    }

    public TestAttribute(params string[] tags)
    {
        Tags = tags ?? [];
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class SetUpAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class DataSourceAttribute(Type? sourceType, string sourceName) : Attribute
{
    public Type? SourceType { get; } = sourceType;
    public string SourceName { get; } = sourceName;
    public DataSourceAttribute(string sourceName) : this(null, sourceName) { }
}

/// <summary>
/// Specifies that this test is expected to have a status other than ok, e.g. failed.
/// Used for testing the test framework itself.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ExpectedStatusAttribute(TestStatus expectedStatus) : Attribute
{
    public TestStatus ExpectedStatus { get; } = expectedStatus;
}