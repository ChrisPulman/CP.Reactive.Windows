// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Security;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Registry Tests behavior.</summary>
public class RegistryTests
{
    /// <summary>Defines the TestValue1000 test value.</summary>
    private const int TestValue1000 = 1000;

    /// <summary>Writes diagnostic messages for these tests.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(RegistryTests));

    /// <summary>Test if the observable functions.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_RegistryMonitorAsync()
    {
        const string internetSettingsKey = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
        const string autoConfigUrlKey = "AutoConfigURL";
        const string autoConfigUrl = "http://somedomain/file.pac";

        using var regKey = Registry.CurrentUser.OpenSubKey(internetSettingsKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
        await Assert.That(regKey).IsNotNull();
        var originalValue = regKey.GetValue(autoConfigUrlKey) as string;
        Log.DebugFormat("Original value {0}", originalValue);
        try
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (RegistryMonitor.ObserveChanges(RegistryHive.CurrentUser, internetSettingsKey).SubscribeOnNext(unit =>
                   {
                       Log.Debug("Got registry change!");
                       tcs.SetResult(true);
                   }))
            {
                // Set some value
                regKey.SetValue(autoConfigUrlKey, autoConfigUrl);

                // Timeout
                var timeoutTask = Task.Delay(TestValue1000, CancellationToken.None);

                // Wait for the value to arrive
                await Task.WhenAny(timeoutTask, tcs.Task).ConfigureAwait(true);
            }

            // The Task should have ran to completion
            await Assert.That(tcs.Task.Status).IsEqualTo(TaskStatus.RanToCompletion);

            var currentValue = regKey.GetValue(autoConfigUrlKey) as string;
            Log.DebugFormat("Current value {0}", currentValue);
        }
        finally
        {
            // Restore back to the original
            if (originalValue is null)
            {
                regKey.DeleteValue(autoConfigUrlKey);
            }
            else
            {
                regKey.SetValue(autoConfigUrlKey, originalValue);
            }

            var resetValue = regKey.GetValue(autoConfigUrlKey) as string;
            Log.DebugFormat("Reset back to value {0}", resetValue);
        }
    }
}
