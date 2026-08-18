// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Structs;
#endif
/// <summary>
///     A struct used by SendInput to store information for synthesizing input events such as keystrokes, mouse movement,
///     and mouse clicks.
///     See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646272(v=vs.85).aspx">LASTINPUTINFO structure</a>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct LastInputInfo
{
    /// <summary>Stores the structure size.</summary>
    private readonly uint _size;

    /// <summary>Stores the last input tick count.</summary>
    private readonly uint _time;

    /// <summary>Initializes a new instance of the <see cref="LastInputInfo"/> struct.</summary>
    /// <param name="size">The native structure size.</param>
    private LastInputInfo(uint size)
    {
        _size = size;
        _time = 0U;
    }

    /// <summary>Gets the tick count for the last registered input.</summary>
    public uint TickCountLastInput => _time;

    /// <summary>Gets the timespan for how long ago the last input was.</summary>
    public TimeSpan LastInputTimeSpan => TimeSpan.FromMilliseconds(unchecked((uint)Environment.TickCount - _time));

    /// <summary>Gets returns the DateTimeOffset for the tick count of the last input.</summary>
    public DateTimeOffset LastInputDateTime => GetLastInputDateTime(TimeProvider.System);

    /// <summary>Gets the structure size.</summary>
    internal uint Size => _size;

    /// <summary>A factory method to simplify creating the LastInputInfo struct.</summary>
    /// <returns>LastInputInfo.</returns>
    public static LastInputInfo Create() => new(checked((uint)Marshal.SizeOf<LastInputInfo>()));

    /// <summary>Gets the time for the last input using the supplied time provider.</summary>
    /// <param name="timeProvider">The time provider.</param>
    /// <returns>The local time of the last input.</returns>
    public DateTimeOffset GetLastInputDateTime(TimeProvider timeProvider) => timeProvider.GetLocalNow().Subtract(LastInputTimeSpan);
}
