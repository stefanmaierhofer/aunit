/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

using AUnit;
using System.Reflection;

var assembly = Assembly.GetExecutingAssembly();

_ = await TestProtocol.CreateAsync(assembly);

//{
//    var foo = new PerfTests();
//    foo.Time(TestEnv.Empty);
//    return;
//}
