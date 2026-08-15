// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Structs;

/// <summary>This structure is returned when WFQuerySessionInformation is called with WFInfoClasses.SessionTime.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct SessionTime : IEquatable<SessionTime>
{
    /// <summary>The connect time.</summary>
    private readonly double _connectTime;

    /// <summary>The disconnect time.</summary>
    private readonly double _disconnectTime;

    /// <summary>The last input time.</summary>
    private readonly double _lastInputTime;

    /// <summary>The logon time.</summary>
    private readonly double _logonTime;

    /// <summary>The current time.</summary>
    private readonly double _currentTime;

    /// <summary>Initializes a new instance of the <see cref="SessionTime"/> struct.</summary>
    /// <param name="connectTime">The connect time.</param>
    /// <param name="disconnectTime">The disconnect time.</param>
    /// <param name="lastInputTime">The last input time.</param>
    /// <param name="logonTime">The logon time.</param>
    /// <param name="currentTime">The current time.</param>
    internal SessionTime(double connectTime, double disconnectTime, double lastInputTime, double logonTime, double currentTime)
    {
        _connectTime = connectTime;
        _disconnectTime = disconnectTime;
        _lastInputTime = lastInputTime;
        _logonTime = logonTime;
        _currentTime = currentTime;
    }

    /// <summary>Gets the connect time.</summary>
    public double ConnectTime => _connectTime;

    /// <summary>Gets the last disconnect time.</summary>
    public double DisconnectTime => _disconnectTime;

    /// <summary>Gets the last input time.</summary>
    public double LastInputTime => _lastInputTime;

    /// <summary>Gets the logon time.</summary>
    public double LogonTime => _logonTime;

    /// <summary>Gets the current time.</summary>
    public double CurrentTime => _currentTime;

    /// <summary>Determines whether two values are equal.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if the values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(SessionTime left, SessionTime right) => left.Equals(right);

    /// <summary>Determines whether two values are not equal.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if the values are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(SessionTime left, SessionTime right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is SessionTime other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(SessionTime other) =>
        _connectTime.Equals(other._connectTime)
        && _disconnectTime.Equals(other._disconnectTime)
        && _lastInputTime.Equals(other._lastInputTime)
        && _logonTime.Equals(other._logonTime)
        && _currentTime.Equals(other._currentTime);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(
        _connectTime,
        _disconnectTime,
        _lastInputTime,
        _logonTime,
        _currentTime);

    /// <inheritdoc/>
    public override string ToString() => $"{ConnectTime}|{DisconnectTime}|{LastInputTime}|{LogonTime}|{CurrentTime}";
}
