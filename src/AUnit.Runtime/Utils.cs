/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

using System.Collections.Immutable;
using System.Text.Json;

namespace AUnit;

internal static class Utils
{
    public static readonly string NugetPackageFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".nuget", "packages"
        );

    /// <summary>
    /// Mapping old to new Target Framework Monikers (TFMs).
    /// </summary>
    public static readonly ImmutableDictionary<string, string> TfmOld2New = ImmutableDictionary<string, string>.Empty
        .Add(".NETFramework,Version=v4.8", "net48")
        .Add(".NETFramework,Version=v4.7.2", "net472")
        .Add(".NETFramework,Version=v4.7.1", "net471")
        .Add(".NETFramework,Version=v4.7", "net47")
        .Add(".NETFramework,Version=v4.6.2", "net462")
        .Add(".NETFramework,Version=v4.6.1", "net461")
        .Add(".NETFramework,Version=v4.6", "net46")
        .Add(".NETFramework,Version=v4.5.2", "net452")
        .Add(".NETFramework,Version=v4.5.1", "net451")
        .Add(".NETFramework,Version=v4.5", "net45")
        .Add(".NETStandard,Version=v2.1", "netstandard2.1")
        .Add(".NETStandard,Version=v2.0", "netstandard2.0")
        .Add(".NETStandard,Version=v1.6", "netstandard1.6")
        .Add(".NETStandard,Version=v1.5", "netstandard1.5")
        .Add(".NETStandard,Version=v1.4", "netstandard1.4")
        .Add(".NETStandard,Version=v1.3", "netstandard1.3")
        .Add(".NETStandard,Version=v1.2", "netstandard1.2")
        .Add(".NETStandard,Version=v1.1", "netstandard1.1")
        .Add(".NETStandard,Version=v1.0", "netstandard1.0")
        .Add(".NETCoreApp,Version=v5.0", "net5.0")
        .Add(".NETCoreApp,Version=v3.1", "netcoreapp3.1")
        .Add(".NETCoreApp,Version=v3.0", "netcoreapp3.0")
        .Add(".NETCoreApp,Version=v2.2", "netcoreapp2.2")
        .Add(".NETCoreApp,Version=v2.1", "netcoreapp2.1")
        .Add(".NETCoreApp,Version=v2.0", "netcoreapp2.0")
        .Add(".NETCoreApp,Version=v1.1", "netcoreapp1.1")
        .Add(".NETCoreApp,Version=v1.0", "netcoreapp1.0")
        ;

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}