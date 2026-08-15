// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Test mouse hooking.</summary>
public class MouseHookTests
{
    /// <summary>The mouse hook timeout in seconds.</summary>
    private const int MouseHookTimeoutSeconds = 2;

    /// <summary>Tests Left Mouse Down.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_LeftMouseDownAsync()
    {
        var observed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        IDisposable subscription;
        try
        {
            subscription = MouseHook.MouseHookEvents.SubscribeOnNext(args =>
            {
                if (args.WindowsMessage == WindowsMessages.WM_LBUTTONDOWN)
                {
                    _ = observed.TrySetResult(true);
                }
            });
        }
        catch (EntryPointNotFoundException exception)
        {
            await Assert.That(exception.Message).Contains("SetWindowsHookEx");
            return;
        }

        using (subscription)
        {
            _ = MouseInputGenerator.MouseClick(MouseButtons.Left);

            var completedTask = await Task.WhenAny(observed.Task, Task.Delay(TimeSpan.FromSeconds(MouseHookTimeoutSeconds)));
            await Assert.That(ReferenceEquals(completedTask, observed.Task)).IsTrue();
            await Assert.That(await observed.Task).IsTrue();
        }
    }
}
