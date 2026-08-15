// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace CP.ReactiveUI.Primitives.Windows.Tests.Benchmarks;

/// <summary>Runs the benchmark suite.</summary>
public static class Program
{
    /// <summary>The maximum iteration count used by each benchmark job.</summary>
    private const int MaxIterationCount = 20;

    /// <summary>Runs the configured benchmarks.</summary>
    /// <param name="args">The command-line arguments.</param>
    [STAThread]
    public static void Main(string[] args)
    {
        var currentRuntimeJob = Job.Default
            .WithMaxIterationCount(MaxIterationCount)
            .WithPlatform(BenchmarkDotNet.Environments.Platform.X64);
        var config = DefaultConfig.Instance
            .AddJob(currentRuntimeJob);

        _ = BenchmarkRunner.Run<ScreenboundsBenchmark>(config);
        _ = BenchmarkRunner.Run<ClipboardBenchmarks>(config);
        _ = BenchmarkRunner.Run<EnumerateWindowsBenchmark>(config);
        _ = BenchmarkRunner.Run<InteropWindowBenchmark>(config);
        _ = Console.ReadLine();
    }
}
