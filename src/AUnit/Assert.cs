/*
 * Copyright (C) 2024 Stefan Maierhofer
 * 
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AUnit;

public sealed class PassException : Exception
{
    public PassException() : base() { }
    public PassException(string message) : base(message) { }
}

public sealed class FailException : Exception
{
    public FailException() : base() { }
    public FailException(string message) : base(message) { }
}

public sealed class IgnoredException : Exception
{
    public IgnoredException() : base() { }
    public IgnoredException(string message) : base(message) { }
}

public sealed class InconclusiveException : Exception
{
    public InconclusiveException() : base() { }
    public InconclusiveException(string message) : base(message) { }
}

public static class Assert
{
    #region True/False

    public static void IsTrue(bool x, string? message = null)
    {
        if (x == false) throw new Exception(message ?? "");
    }

    public static void True(bool x, string? message = null)
    {
        if (x == false) throw new Exception(message ?? "");
    }

    public static void IsFalse(bool x, string? message = null)
    {
        if (x == true) throw new Exception(message ?? "");
    }

    public static void False(bool x, string? message = null)
    {
        if (x == true) throw new Exception(message ?? "");
    }

    public static void That(bool x, string? message = null)
    {
        if (x == false) throw new Exception(message ?? "");
    }

    #endregion

    #region Null/Empty/...

    public static void IsNull(object? o, string? message = null)
    {
        if (o != null) throw new Exception(message ?? "");
    }

    public static void IsNotNull(object? o, string? message = null)
    {
        if (o == null) throw new Exception(message ?? "");
    }

    public static void IsNullOrEmpty(string? s, string? message = null)
    {
        if (!string.IsNullOrEmpty(s)) throw new Exception(message ?? "");
    }

    public static void IsNotNullOrEmpty(string? s, string? message = null)
    {
        if (string.IsNullOrEmpty(s)) throw new Exception(message ?? "");
    }

    public static void IsEmpty<T>(IReadOnlyList<T>? xs, string? message = null)
    {
        if (xs != null && xs.Count > 0) throw new Exception(message ?? "");
    }

    public static void IsNotEmpty<T>(IReadOnlyList<T>? xs, string? message = null)
    {
        if (xs == null || xs.Count == 0) throw new Exception(message ?? "");
    }

    #endregion

    #region Multiple

    public static void Multiple(Action fun)
    {
        fun.Invoke();
    }

    #endregion

    #region Throws/Catch/DoesNotThrow

    public static Exception Throws(Action fun, string? message = null)
    {
        try
        {
            fun.Invoke();
        }
        catch (FailException) { throw; }
        catch (PassException) { throw; }
        catch (IgnoredException) { throw; }
        catch (InconclusiveException) { throw; }
        catch (Exception e) { return e; }

        throw message != null ? new Exception(message) : new Exception();
    }

    public static Exception Throws<E>(Action fun, string? message = null) where E : Exception
    {
        try
        {
            fun.Invoke();
        }
        catch (FailException) { throw; }
        catch (PassException) { throw; }
        catch (IgnoredException) { throw; }
        catch (InconclusiveException) { throw; }
        catch (Exception e)
        {
            if (e.GetType() == typeof(E)) return e;
        }

        throw message != null ? new Exception(message) : new Exception();
    }

    public static async Task<Exception> ThrowsAsync(Func<Task> fun, string? message = null)
    {
        try
        {
            await fun.Invoke();
        }
        catch (FailException) { throw; }
        catch (PassException) { throw; }
        catch (IgnoredException) { throw; }
        catch (InconclusiveException) { throw; }
        catch (Exception e) { return e; }

        throw message != null ? new Exception(message) : new Exception();
    }

    public static async Task<Exception> ThrowsAsync<E>(Func<Task> fun, string? message = null) where E : Exception
    {
        try
        {
            await fun.Invoke();
        }
        catch (FailException) { throw; }
        catch (PassException) { throw; }
        catch (IgnoredException) { throw; }
        catch (InconclusiveException) { throw; }
        catch (Exception e)
        {
            if (e.GetType() == typeof(E)) return e;
        }

        throw message != null ? new Exception(message) : new Exception();
    }

    public static Exception Catch(Action fun, string? message = null)
        => Catch<Exception>(fun, message);

    public static Exception Catch<E>(Action fun, string? message = null) where E : Exception
    {
        try
        {
            fun.Invoke();
        }
        catch (FailException) { throw; }
        catch (PassException) { throw; }
        catch (IgnoredException) { throw; }
        catch (InconclusiveException) { throw; }
        catch (Exception e)
        {
            if (e is E) return e;
        }

        throw message != null ? new Exception(message) : new Exception();
    }

    public static Task<Exception> CatchAsync(Func<Task> fun, string? message = null)
        => CatchAsync<Exception>(fun, message);

    public static async Task<Exception> CatchAsync<E>(Func<Task> fun, string? message = null) where E : Exception
    {
        try
        {
            await fun.Invoke();
        }
        catch (FailException) { throw; }
        catch (PassException) { throw; }
        catch (IgnoredException) { throw; }
        catch (InconclusiveException) { throw; }
        catch (Exception e)
        {
            if (e is E) return e;
        }

        throw message != null ? new Exception(message) : new Exception();
    }

    public static void DoesNotThrow(Action fun)
    {
        fun.Invoke();
    }

    public static async Task DoesNotThrowAsync(Func<Task> fun)
    {
        await fun.Invoke();
    }

    #endregion

    #region Fail/Pass/Ignore/Inconclusive

    /// <summary>
    /// Immediately fails test, also from within other Assert statements like Assert.Throws.
    /// </summary>
    public static void Fail() => Fail(message: null);

    /// <summary>
    /// Immediately fails test, also from within other Assert statements like Assert.Throws.
    /// </summary>
    public static void Fail(string? message, params object[] @params)
    {
        if (message != null)
        {
            throw new FailException(string.Format(message, @params));
        }
        else
        {
            throw new FailException();
        }
    }

    /// <summary>
    /// Immediately passes test as ok, even from within other nested Assert statements like Assert.Throws.
    /// </summary>
    public static void Pass() => Pass(message: null);

    /// <summary>
    /// Immediately passes test as ok, even from within other nested Assert statements like Assert.Throws.
    /// </summary>
    public static void Pass(string? message, params object[] @params)
    {
        if (message != null)
        {
            throw new PassException(string.Format(message, @params));
        }
        else
        {
            throw new PassException();
        }
    }

    /// <summary>
    /// Immediately passes test as ok, even from within other nested Assert statements like Assert.Throws.
    /// </summary>
    public static void Succeed() => Succeed(message: null);

    /// <summary>
    /// Immediately passes test as ok, even from within other nested Assert statements like Assert.Throws.
    /// </summary>
    public static void Succeed(string? message, params object[] @params)
    {
        if (message != null)
        {
            throw new PassException(string.Format(message, @params));
        }
        else
        {
            throw new PassException();
        }
    }

    public static void Ignore() => Ignore(message: null);

    public static void Ignore(string? message, params object[] @params)
    {
        if (message != null)
        {
            throw new IgnoredException(string.Format(message, @params));
        }
        else
        {
            throw new IgnoredException();
        }
    }

    public static void Inconclusive() => Inconclusive(message: null);

    public static void Inconclusive(string? message, params object[] @params)
    {
        if (message != null)
        {
            throw new InconclusiveException(string.Format(message, @params));
        }
        else
        {
            throw new InconclusiveException();
        }
    }

    #endregion
}
