// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using CP.ReactiveUI.Primitives.Windows.Desktop.Windows;

namespace CP.ReactiveUI.Primitives.Windows.Tests.Benchmarks;

/// <summary>Benchmarks operations on an interop window.</summary>
[MinColumn]
[MaxColumn]
[MemoryDiagnoser]
public class InteropWindowBenchmark
{
    /// <summary>The timeout used while waiting for the benchmark process to become idle.</summary>
    private const int InputIdleTimeoutMilliseconds = 2000;

    /// <summary>The process used by the benchmark.</summary>
    private Process _processForTest;

    /// <summary>Starts the process used by the benchmark.</summary>
    [GlobalSetup]
    public void Setup()
    {
        // Start a process to test against
        _processForTest = Process.Start("charmap.exe") ?? throw new NotSupportedException("Couldn't start charmap.exe");

        // Make sure it's started
        // Wait until the process started it's message pump (listening for input)
        if (!_processForTest.WaitForInputIdle(InputIdleTimeoutMilliseconds))
        {
            throw new NotSupportedException("Process not ready");
        }
    }

    /// <summary>Measures filling the target window.</summary>
    [Benchmark]
    [STAThread]
    public void Fill()
    {
        var interopWindow = InteropWindowFactory.CreateFor(_processForTest.MainWindowHandle);
        if (interopWindow.Handle.IsInvalid)
        {
            throw new NotSupportedException("Somehow the window was not found!");
        }

        _ = interopWindow.Fill();
    }

    /// <summary>Stops the process used by the benchmark.</summary>
    [GlobalCleanup]
    public void Cleanup() => _processForTest.Kill();
}
