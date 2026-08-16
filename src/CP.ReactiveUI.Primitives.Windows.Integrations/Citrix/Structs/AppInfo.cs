// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Structs;

/// <summary>This structure is returned when WFQuerySessionInformation is called with WFInfoClasses.AppInfo.</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public readonly struct AppInfo : IEquatable<AppInfo>
{
    /// <summary>The initial program.</summary>
    [MarshalAs(UnmanagedType.LPWStr)]
    private readonly string _initialProgram;

    /// <summary>The working directory.</summary>
    [MarshalAs(UnmanagedType.LPWStr)]
    private readonly string _workingDirectory;

    /// <summary>The application name.</summary>
    [MarshalAs(UnmanagedType.LPWStr)]
    private readonly string _applicationName;

    /// <summary>Initializes a new instance of the <see cref="AppInfo"/> struct.</summary>
    /// <param name="initialProgram">The initial program.</param>
    /// <param name="workingDirectory">The working directory.</param>
    /// <param name="applicationName">The application name.</param>
    internal AppInfo(string initialProgram, string workingDirectory, string applicationName)
    {
        _initialProgram = initialProgram;
        _workingDirectory = workingDirectory;
        _applicationName = applicationName;
    }

    /// <summary>Gets the initial program.</summary>
    public string InitialProgram => _initialProgram;

    /// <summary>Gets the working directory.</summary>
    public string WorkingDirectory => _workingDirectory;

    /// <summary>Gets the application name.</summary>
    public string ApplicationName => _applicationName;

    /// <summary>Determines whether two values are equal.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if the values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(AppInfo left, AppInfo right) => left.Equals(right);

    /// <summary>Determines whether two values are not equal.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if the values are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(AppInfo left, AppInfo right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is AppInfo other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(AppInfo other) =>
        string.Equals(_initialProgram, other._initialProgram, StringComparison.Ordinal)
        && string.Equals(_workingDirectory, other._workingDirectory, StringComparison.Ordinal)
        && string.Equals(_applicationName, other._applicationName, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(
        StringComparer.Ordinal.GetHashCode(_initialProgram ?? string.Empty),
        StringComparer.Ordinal.GetHashCode(_workingDirectory ?? string.Empty),
        StringComparer.Ordinal.GetHashCode(_applicationName ?? string.Empty));

    /// <inheritdoc/>
    public override string ToString() => $"{ApplicationName}|{InitialProgram}|{WorkingDirectory}";
}
