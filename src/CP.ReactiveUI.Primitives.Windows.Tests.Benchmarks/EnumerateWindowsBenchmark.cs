// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using CP.ReactiveUI.Primitives.Windows.Desktop.Windows;

namespace CP.ReactiveUI.Primitives.Windows.Tests.Benchmarks;

/// <summary>Benchmarks synchronous and reactive window enumeration.</summary>
[MinColumn]
[MaxColumn]
[MemoryDiagnoser]
public class EnumerateWindowsBenchmark
{
    /// <summary>Represents the bounded enumeration size used by the benchmark methods.</summary>
    private const int WindowCountLimit = 10;

    /// <summary>Measures enumeration of every available window.</summary>
    [Benchmark]
    [STAThread]
    public void EnumerateAll()
    {
        foreach (var window in WindowsEnumerator.EnumerateWindows((IInteropWindow)null))
        {
            if (window is null)
            {
                throw new InvalidOperationException("Window enumeration returned a null window.");
            }
        }
    }

    /// <summary>Measures reactive enumeration of every available window.</summary>
    /// <returns>A task that represents the asynchronous benchmark operation.</returns>
    [Benchmark]
    [STAThread]
    public async Task EnumerateAll_Async()
    {
        var result = await WindowsEnumerator.ObserveWindows().All(static window => window is not null).ToTask();
        if (!result)
        {
            throw new InvalidOperationException("Window enumeration returned a null window.");
        }
    }

    /// <summary>Measures reactive enumeration of a bounded window sequence.</summary>
    /// <returns>A task that represents the asynchronous benchmark operation.</returns>
    [Benchmark]
    [STAThread]
    public async Task Enumerate_Take10_Async()
    {
        var result = await WindowsEnumerator.ObserveWindows().Take(WindowCountLimit).Count(static window => window is not null).ToTask();
        ValidateWindowCount(result);
    }

    /// <summary>Measures synchronous enumeration of a bounded window sequence.</summary>
    [Benchmark]
    [STAThread]
    public void Enumerate_Take10()
    {
        var result = 0;
        var processed = 0;
        foreach (var window in WindowsEnumerator.EnumerateWindows((IInteropWindow)null))
        {
            processed++;
            if (window is not null)
            {
                result++;
            }

            if (processed == WindowCountLimit)
            {
                break;
            }
        }

        ValidateWindowCount(result);
    }

    /// <summary>Measures enumeration limited by the enumerator callback.</summary>
    [Benchmark]
    [STAThread]
    public void Enumerate_LimitFunc()
    {
        var result = 0;
        foreach (var window in WindowsEnumerator.EnumerateWindows((IInteropWindow)null, null, static (_, index) => index < WindowCountLimit))
        {
            if (window is not null)
            {
                result++;
            }
        }

        ValidateWindowCount(result);
    }

    /// <summary>Measures reactive handle enumeration with window creation.</summary>
    /// <returns>A task that represents the asynchronous benchmark operation.</returns>
    [Benchmark]
    [STAThread]
    public async Task EnumerateHandlesAll_Select_Async()
    {
        var result = await WindowsEnumerator.ObserveWindowHandles().Select(InteropWindowFactory.CreateFor).All(static window => window is not null).ToTask();
        if (!result)
        {
            throw new InvalidOperationException("Window creation returned a null window.");
        }
    }

    /// <summary>Measures reactive handle-only enumeration.</summary>
    /// <returns>A task that represents the asynchronous benchmark operation.</returns>
    [Benchmark]
    [STAThread]
    public async Task EnumerateHandles_HandlesOnly_Async()
    {
        var result = await WindowsEnumerator.ObserveWindowHandles().All(static window => window != IntPtr.Zero).ToTask();
        if (!result)
        {
            throw new InvalidOperationException("Window enumeration returned a zero handle.");
        }
    }

    /// <summary>Validates that a bounded enumeration produced the required number of windows.</summary>
    /// <param name="count">The number of enumerated windows.</param>
    private static void ValidateWindowCount(int count)
    {
        if (count != WindowCountLimit)
        {
            throw new InvalidOperationException($"Expected {WindowCountLimit}, actual {count}.");
        }
    }
}
