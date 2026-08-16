// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CP.ReactiveUI.Primitives.Windows.Building;

/// <summary>Defines the repository's NUKE build pipeline.</summary>
public sealed class Build : NukeBuild
{
    /// <summary>Gets or sets the selected build configuration.</summary>
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    public BuildConfiguration Configuration { get; set; } =
        IsLocalBuild
            ? BuildConfiguration.Debug
            : BuildConfiguration.Release;

    /// <summary>Gets or sets a value indicating whether the build includes the .NET 11 preview target framework.</summary>
    [Parameter("Enable .NET 11 preview target frameworks - Default is 'false' (local) or 'true' (server)")]
    public bool EnableDotNet11PreviewTargetFrameworks { get; set; } = IsServerBuild;

    /// <summary>Gets the package output directory.</summary>
    private static AbsolutePath PackagesDirectory => RootDirectory / "output";

    /// <summary>Gets the solution to build.</summary>
    private static AbsolutePath SolutionFile => RootDirectory / "src" / "CP.ReactiveUI.Primitives.Windows.slnx";

    /// <summary>Gets the target that reports the effective build parameters.</summary>
    private Target Print => _ => _
        .Executes(() =>
        {
            Log.Information("Configuration = {Configuration}", Configuration);
            Log.Information("EnableDotNet11PreviewTargetFrameworks = {Value}", EnableDotNet11PreviewTargetFrameworks);
        });

    /// <summary>Gets the target that cleans server-build package output.</summary>
    private Target Clean => definition => definition
        .Before(Restore)
        .Executes(static () =>
        {
            if (IsLocalBuild)
            {
                return;
            }

            _ = PackagesDirectory.CreateOrCleanDirectory();
        });

    /// <summary>Gets the target that restores solution packages.</summary>
    private Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() => DotNetRestore(settings => settings
            .SetProjectFile(SolutionFile)
            .SetProperty(nameof(EnableDotNet11PreviewTargetFrameworks), EnableDotNet11PreviewTargetFrameworks)));

    /// <summary>Gets the target that compiles the solution.</summary>
    private Target Compile => _ => _
        .DependsOn(Restore, Print)
        .Executes(() => DotNetBuild(settings => settings
            .SetProjectFile(SolutionFile)
            .SetConfiguration(Configuration)
            .SetNoRestore(true)
            .SetProperty(nameof(EnableDotNet11PreviewTargetFrameworks), EnableDotNet11PreviewTargetFrameworks)));

    /// <summary>Runs the NUKE pipeline.</summary>
    /// <returns>The process exit code.</returns>
    public static int Main() => Execute<Build>(build => build.Compile);
}
