// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Testing.Platform.Builder;

return await CP.ReactiveUI.Primitives.Windows.Tests.Program.RunAsync(args);

namespace CP.ReactiveUI.Primitives.Windows.Tests
{
    /// <summary>Creates and runs the Microsoft Testing Platform application.</summary>
    internal static class Program
    {
        /// <summary>Runs the configured test application.</summary>
        /// <param name="commandLineArguments">The command-line arguments supplied to the test application.</param>
        /// <returns>The test application exit code.</returns>
        internal static async Task<int> RunAsync(string[] commandLineArguments)
        {
            TestLogging.UseConsoleLogger();
            var builder = await TestApplication.CreateBuilderAsync(commandLineArguments);

            builder.AddSelfRegisteredExtensions(commandLineArguments);

            using var app = await builder.BuildAsync();

            return await app.RunAsync();
        }
    }
}
