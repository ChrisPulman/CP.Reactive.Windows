// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Win Event Hook Tests behavior.</summary>
public class WinEventHookTests
{
    /// <summary>Defines the TestValue10000 test value.</summary>
    private const int TestValue10000 = 10_000;

    /// <summary>Defines the TestValue100 test value.</summary>
    private const int TestValue100 = 100;

    /// <summary>Defines the TestValue5 test value.</summary>
    private const int TestValue5 = 5;

    /// <summary>Writes diagnostic messages for these tests.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(WinEventHookTests));

    /// <summary>Test typing in a notepad.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestWinEventHookAsync()
    {
        var processId = 0;
        var testWindowSource = new TaskCompletionSource<IInteropWindow>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var winEventObservable = WinEventHook.ObserveWindowTitleChanges().SubscribeOnNext(info =>
        {
            var interopWindow = info.Window.Fill();
            if (string.IsNullOrEmpty(interopWindow?.Caption))
            {
                return;
            }

            Log.DebugFormat("Window title change: Process ID {0} - Title: {1}", interopWindow.Handle, interopWindow.Caption);
            if (processId != 0 && interopWindow.ProcessId == processId)
            {
                _ = testWindowSource.TrySetResult(interopWindow);
            }
        });

        // Start a process to test against
        using var process = Process.Start("charmap.exe");
        try
        {
            // Make sure it's started
            await Assert.That(process).IsNotNull();
            processId = process.Id;

            // Wait until the process started its message pump (listening for input)
            var processReady = process.WaitForInputIdle(TestValue10000);
            await Assert.That(processReady).IsTrue();
            if (!processReady)
            {
                return;
            }

            _ = User32Api.SetWindowText(process.MainWindowHandle, "TestWinEventHook - Test");

            // Find the belonging window
            var testWindow = await AwaitWithTimeoutAsync(testWindowSource.Task, TimeSpan.FromSeconds(TestValue5));
            await Assert.That(testWindow?.ProcessId).IsEqualTo(process.Id);
        }
        finally
        {
            process?.Kill();
        }
    }

    /// <summary>Awaits a task, failing with a timeout when it does not complete in time.</summary>
    /// <typeparam name="T">The task result type.</typeparam>
    /// <param name="task">The task to await.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <returns>The completed task result.</returns>
    private static async Task<T> AwaitWithTimeoutAsync<T>(Task<T> task, TimeSpan timeout)
    {
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout));
        if (!ReferenceEquals(completedTask, task))
        {
            throw new TimeoutException("The window-title notification did not arrive before the timeout elapsed.");
        }

        return await task;
    }
}
