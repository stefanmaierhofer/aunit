/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace AUnit;

public record TestResult(
    DateTimeOffset Created,
    MethodInfo TestMethod,
    string TargetFramework,
    string Configuration,
    string AssemblyVersion,
    string? SourceData,
    TimeSpan Elapsed,
    TestStatus Status,
    TestEnv Env,
    string? Message,
    string ConsoleOutput,
    Exception? Exception
    )
{
    public Type? TestType => TestMethod.DeclaringType;
    public Assembly? TestAssembly => TestMethod.DeclaringType?.Assembly;

    public bool IsSlow => Env.Perfs.Any(x => x.IsSlow);

    public TestResultDto ToDto() => new(
        Created,
        TestAssembly?.FullName ?? "",
        TestType?.FullName ?? "",
        TestMethod.Name,
        TargetFramework,
        SourceData,
        Elapsed.TotalSeconds,
        Status,
        Env,
        ConsoleOutput,
        Exception?.ToString()
        );
}

public record TestResultDto(
    DateTimeOffset Created,
    string TestAssembly,
    string TestType,
    string TestMethod,
    string TargetFramework,
    string? SourceData,
    double ElapsedSeconds,
    TestStatus Status,
    TestEnv Env,
    string ConsoleOutput,
    string? Exception
    );

public static class TestRunner
{
    public static IEnumerable<(Type type, MethodInfo[] methods)> DiscoverTests(Assembly assembly)
    {
        var types = assembly.GetTypes();
        foreach (var t in types)
        {
            MethodInfo[]? methods;
            try
            {
                methods = t.GetMethods();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[TestRunner] failed to get methods from type {t.AssemblyQualifiedName} {e.Message}");
                continue; // try next type
            }

            var methodsWithTestAttribute = new List<MethodInfo>();
            foreach (var m in methods)
            {
                try
                {
                    var a = m.GetCustomAttribute(typeof(TestAttribute));
                    if (a != null) methodsWithTestAttribute.Add(m);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[TestRunner] {e.Message}");
                }
            }

            if (methodsWithTestAttribute.Count > 0)
            {
                yield return (t, methodsWithTestAttribute.ToArray());
            }
        }
    }

    public static (Type type, MethodInfo[] methods)[] DiscoverTests(Type type) => new[] { type }
        .Select(t =>
        (
            type: t,
            methods: t.GetMethods().Where(m => m.GetCustomAttribute(typeof(TestAttribute)) != null).ToArray()
        ))
        .Where(x => x.methods.Length > 0)
        .ToArray();

    public static async IAsyncEnumerable<TestResult> RunAsync(Assembly assembly, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var testTypes = DiscoverTests(assembly);

        foreach (var (type, _) in testTypes)
        {
            await foreach (var x in RunAsync(type, ct))
            {
                yield return x;
            }
        }
    }

    public static async IAsyncEnumerable<TestResult> RunAsync(Type type, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var testMethods = type.GetMethods()
            .Where(m => m.GetCustomAttribute(typeof(TestAttribute)) != null)
            .ToArray()
            ;

        if (testMethods.Length > 0)
        {
            var testObject = Activator.CreateInstance(type) ?? throw new Exception(
                $"Failed to create test instance of type {type.AssemblyQualifiedName}. " +
                $"Internal error 30ed4b7f-b568-41de-8ead-5540d0452146."
                );

            foreach (var testMethod in testMethods/*.Skip(37).Take(1)*/)
            {
                await foreach (var result in RunTestMethodAsync(new(testObject, testMethod), ct))
                {
                    yield return result;
                }
            }
        }
    }

    private static async IAsyncEnumerable<TestResult> RunTestMethodAsync(TestConfig config, [EnumeratorCancellation] CancellationToken ct)
    {
        var sw = new Stopwatch();
        var stdout = Console.Out;

        var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        var customAttributes = config.TestMethod.DeclaringType?.Assembly.CustomAttributes;

        var targetFramework = customAttributes
            ?.SingleOrDefault(x => x.AttributeType == typeof(TargetFrameworkAttribute))
            ?.NamedArguments[0].TypedValue.Value?.ToString() ?? ""
            ;

        var configuration = customAttributes
            ?.SingleOrDefault(x => x.AttributeType == typeof(AssemblyConfigurationAttribute))
            ?.ConstructorArguments.SingleOrDefault().Value as string ?? ""
            ;

        var assemblyVersion = customAttributes
            ?.SingleOrDefault(x => x.AttributeType == typeof(AssemblyFileVersionAttribute))
            ?.ConstructorArguments.SingleOrDefault().Value as string ?? ""
            ;

        var expectedStatusAttributes = config.TestMethod.GetCustomAttributes(typeof(ExpectedStatusAttribute)).ToArray();
        var expectedStatus = expectedStatusAttributes.Length switch
        {
            0 => TestStatus.Ok,
            1 => ((ExpectedStatusAttribute)expectedStatusAttributes[0]).ExpectedStatus,
            _ => throw new Exception(
                $"Expected at most 1 ExpectedStatusAttribute on method {config.TestMethod.Name}, but found {expectedStatusAttributes.Length}." +
                $"Error db457f6d-a192-4f51-8df4-cf443efdc6ce."
                )
        };

        var dataSourceAttributes = config.TestMethod.GetCustomAttributes(typeof(DataSourceAttribute)).ToArray();
        if (dataSourceAttributes.Length > 0)
        {
            foreach (var _dataSourceAttribute in dataSourceAttributes.Cast<DataSourceAttribute>())
            {
                var dataSourceType = _dataSourceAttribute.SourceType ?? config.TestMethod.DeclaringType;
                var dataSourceName = _dataSourceAttribute.SourceName;

                var dataSourceFunc = dataSourceType?.GetMethod(dataSourceName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (dataSourceFunc != null)
                {
                    var dataSourceInstance = (dataSourceFunc.IsStatic, dataSourceType == config.TestMethod.DeclaringType) switch
                    {
                        (true, _) => null,
                        (false, true) => config.TestObject,
                        (false, false) => Activator.CreateInstance(dataSourceType!)
                    };
                    
                    var dataItems = dataSourceFunc.Invoke(dataSourceInstance, null)!;
                    var dataItemsType = dataItems.GetType();
                    var returnType = dataSourceFunc.ReturnType;

                    if (dataItems is IEnumerable xs)
                    {
                        foreach (var dataItem in xs)
                        {
                            var result = await ExecuteTest(dataItem);
                            yield return result;
                        }
                    }
                    else if (returnType.Name == "IAsyncEnumerable`1")
                    {
                        var getAsyncEnumerator = dataItemsType.GetRuntimeMethods().Where(x => x.Name.Contains("GetAsyncEnumerator")).Single();

                        var enumerator = getAsyncEnumerator.Invoke(dataItems, [ct])!;

                        var enumeratorType = enumerator.GetType();
                        var moveNextAsync = enumeratorType.GetRuntimeMethods().Where(x => x.Name.Contains("MoveNextAsync")).Single();
                        var getCurrent = enumeratorType.GetRuntimeProperties().Single();

                        try
                        {
                            while (await (ValueTask<bool>)moveNextAsync.Invoke(enumerator, null)!)
                            {
                                var arg = getCurrent.GetValue(enumerator)!;
                                var result = await ExecuteTest(arg);
                                yield return result;
                            }
                        }
                        finally
                        {
                            if (enumerator is IAsyncDisposable asyncDisposable)
                            {
                                await asyncDisposable.DisposeAsync();
                            }
                        }
                    }
                    else
                    {
                        ct.ThrowIfCancellationRequested();
                        var outputText = consoleOutput.ToString(); consoleOutput.Dispose();
                        Console.SetOut(stdout);

                        var status = TestStatus.Inconclusive;
                        if (expectedStatus == TestStatus.Inconclusive) status |= TestStatus.Expected;

                        yield return new(
                            Created: DateTimeOffset.UtcNow,
                            TestMethod: config.TestMethod,
                            TargetFramework: targetFramework,
                            Configuration: configuration,
                            AssemblyVersion: assemblyVersion,
                            SourceData: null,
                            Elapsed: sw.Elapsed,
                            Status: status,
                            Env: TestEnv.Empty,
                            Message: $"No source data was generated.\"{dataSourceType}.{dataSourceName}\".",
                            ConsoleOutput: outputText,
                            Exception: null
                            );
                        consoleOutput = new StringWriter(); Console.SetOut(consoleOutput);
                    }

                    async Task<TestResult> ExecuteTest(object dataItem)
                    {
                        Exception? exception = null;
                        string? message = null;
                        var status = TestStatus.Ok;
                        var env = TestEnv.Empty;

                        sw.Restart();
                        try
                        {
                            var paramCount = config.TestMethod.GetParameters().Length;
                            if (paramCount > 1)
                            {
                                if (config.TestMethod.Invoke(config.TestObject, [env, dataItem]) is Task t)
                                {
                                    await t;
                                }
                            }
                            else
                            {
                                if (config.TestMethod.Invoke(config.TestObject, [dataItem]) is Task t)
                                {
                                    await t;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            exception = e.InnerException ?? e;
                            switch (exception)
                            {
                                case FailException eFail:
                                    status = TestStatus.Failed;
                                    exception = null;
                                    message = eFail.Message;
                                    break;
                                case PassException ePass:
                                    status = TestStatus.Ok;
                                    exception = null;
                                    message = ePass.Message;
                                    break;
                                case IgnoredException eIgnored:
                                    status = TestStatus.Ignored;
                                    exception = null;
                                    message = eIgnored.Message;
                                    break;
                                case InconclusiveException eInconclusive:
                                    status = TestStatus.Inconclusive;
                                    exception = null;
                                    message = eInconclusive.Message;
                                    break;
                                default:
                                    status = TestStatus.Failed;
                                    message = exception.Message;
                                    break;
                            }
                        }

                        ct.ThrowIfCancellationRequested();

                        string outputText;

                        try
                        {
                            outputText = consoleOutput.ToString(); consoleOutput.Dispose();
                        }
                        catch (Exception e)
                        {
                            outputText = $"[AUnit][INTERNAL ERROR] failed to process console output ({e.Message})";
                        }

                        Console.SetOut(stdout);
                        consoleOutput = new StringWriter(); Console.SetOut(consoleOutput);

                        if (status != TestStatus.Ok && status == expectedStatus)
                        {
                            status |= TestStatus.Expected;
                        }

                        return new(
                            Created: DateTimeOffset.UtcNow,
                            TestMethod: config.TestMethod,
                            TargetFramework: targetFramework,
                            Configuration: configuration,
                            AssemblyVersion: assemblyVersion,
                            SourceData: dataItem.ToString(),
                            Elapsed: sw.Elapsed,
                            Status: status,
                            Env: env,
                            Message: message,
                            ConsoleOutput: outputText,
                            Exception: exception
                            );
                    }
                }
                else
                {
                    ct.ThrowIfCancellationRequested();
                    var outputText = consoleOutput.ToString(); consoleOutput.Dispose();
                    Console.SetOut(stdout);

                    var status = TestStatus.Failed;
                    if (expectedStatus == TestStatus.Failed) status |= TestStatus.Expected;

                    yield return new(
                        Created: DateTimeOffset.UtcNow,
                        TestMethod: config.TestMethod,
                        TargetFramework: targetFramework,
                        Configuration: configuration,
                        AssemblyVersion: assemblyVersion,
                        SourceData: null,
                        Elapsed: sw.Elapsed,
                        Status: status,
                        Env: TestEnv.Empty,
                        Message: $"Failed to find source data generator \"{dataSourceType}.{dataSourceName}\".",
                        ConsoleOutput: outputText,
                        Exception: new Exception("")
                        );
                    consoleOutput = new StringWriter(); Console.SetOut(consoleOutput);
                }
            }
        }
        else
        {
            Exception? exception = null;
            string? message = null;
            var status = TestStatus.Ok;
            var env = TestEnv.Empty;

            sw.Restart();
            try
            {
                var paramCount = config.TestMethod.GetParameters().Length;
                if (paramCount > 0)
                {
                    if (config.TestMethod.Invoke(config.TestObject, new[] { env } ) is Task t)
                    {
                        await t;
                    }
                }
                else
                {
                    if (config.TestMethod.Invoke(config.TestObject, null) is Task t)
                    {
                        await t;
                    }
                }
            }
            catch (Exception e)
            {
                exception = e.InnerException ?? e;
                switch (exception)
                {
                    case FailException eFail:
                        status = TestStatus.Failed;
                        exception = null;
                        message = eFail.Message;
                        break;
                    case PassException ePass:
                        status = TestStatus.Ok;
                        exception = null;
                        message = ePass.Message;
                        break;
                    case IgnoredException eIgnored:
                        status = TestStatus.Ignored;
                        exception = null;
                        message = eIgnored.Message;
                        break;
                    case InconclusiveException eInconclusive:
                        status = TestStatus.Inconclusive;
                        exception = null;
                        message = eInconclusive.Message;
                        break;
                    default:
                        status = TestStatus.Failed;
                        message = exception.Message;
                        break;
                }
            }

            ct.ThrowIfCancellationRequested();

            string outputText;

            try
            {
                outputText = consoleOutput.ToString(); consoleOutput.Dispose();
            }
            catch (Exception e)
            {
                outputText = $"[AUnit][INTERNAL ERROR] failed to process console output ({e.Message})";
            }

            Console.SetOut(stdout);
            
            if (status != TestStatus.Ok && status == expectedStatus)
            {
                status |= TestStatus.Expected;
            }

            yield return new(
                Created: DateTimeOffset.UtcNow,
                TestMethod: config.TestMethod,
                TargetFramework: targetFramework,
                Configuration: configuration,
                AssemblyVersion: assemblyVersion,
                SourceData: null,
                Elapsed: sw.Elapsed,
                Status: status,
                Env: env,
                Message: message,
                ConsoleOutput: outputText,
                Exception: exception
                );
            consoleOutput = new StringWriter(); Console.SetOut(consoleOutput);
        }
    }

    private record TestConfig(
        object TestObject,
        MethodInfo TestMethod
        );

}