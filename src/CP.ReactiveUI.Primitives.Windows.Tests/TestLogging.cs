// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using log4net.Appender;
using log4net.Config;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides deterministic test logging without depending on xUnit output hooks.</summary>
internal static class TestLogging
{
    /// <summary>Tracks whether logging is configured.</summary>
    private static int _configured;

    /// <summary>Registers console logging once for the test process.</summary>
    internal static void UseConsoleLogger()
    {
        if (Interlocked.Exchange(ref _configured, 1) == 0)
        {
            _ = BasicConfigurator.Configure(new ConsoleAppender());
        }
    }
}
