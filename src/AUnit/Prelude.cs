/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

using System.Linq;
using System.Reflection;

namespace System.Runtime.CompilerServices
{
    internal class IsExternalInit { }
}

namespace AUnit
{
    public static class Globals
    {
        public static readonly string Version = ExtractVersionFromAssembly();

        public const string Copyright = "Copyright (c) 2024 Stefan Maierhofer";

        private static string ExtractVersionFromAssembly()
        {
            var s = typeof(Globals)
                .Assembly
                .GetCustomAttributes()
                .OfType<AssemblyInformationalVersionAttribute>()
                .FirstOrDefault()?.InformationalVersion ?? "0.0.0"
                ;

            var i = s.IndexOf('+');
            return i < 1 ? s : s.Substring(0, i);
        }
    }
}