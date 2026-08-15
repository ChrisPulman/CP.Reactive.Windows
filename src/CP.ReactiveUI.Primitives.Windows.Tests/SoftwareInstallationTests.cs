// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Software Installation Tests behavior.</summary>
public class SoftwareInstallationTests
{
    /// <summary>Writes diagnostic messages for these tests.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(SoftwareInstallationTests));

    /// <summary>Tests Installed Software.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_InstalledSoftwareAsync()
    {
        List<SoftwareDetails> software = [];
        foreach (var softwareDetails in InstallationInformation.InstalledSoftware())
        {
            software.Add(softwareDetails);
        }

        software.Sort(static (left, right) => string.CompareOrdinal(left.DisplayName, right.DisplayName));

        await Assert.That(software.Count > 0).IsTrue();
        foreach (var softwareDetails in software)
        {
            Log.Debug(softwareDetails);
            if (!string.IsNullOrEmpty(softwareDetails.HelpLink))
            {
                Log.DebugFormat("Help - {0}", softwareDetails.HelpLink);
            }
        }
    }
}
