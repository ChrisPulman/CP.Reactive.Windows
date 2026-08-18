// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>Native input methods.</summary>
public static class NativeInput
{
    /// <summary>The native input API override used by deterministic tests.</summary>
    private static INativeInputApi _apiOverride;

    /// <summary>Gets a DateTimeOffset which specifies the last input timestamp.</summary>
    public static DateTimeOffset LastInputDateTime
    {
        get
        {
            LastInputInfo lastInputInfo = LastInputInfo.Create();
            return GetCurrentApi().GetLastInputInfo(ref lastInputInfo) ? lastInputInfo.LastInputDateTime : DateTimeOffset.MinValue;
        }
    }

    /// <summary>Gets a TimeSpan which specifies how long ago the last input was.</summary>
    public static TimeSpan LastInputTimeSpan
    {
        get
        {
            LastInputInfo lastInputInfo = LastInputInfo.Create();
            return GetCurrentApi().GetLastInputInfo(ref lastInputInfo) ? lastInputInfo.LastInputTimeSpan : TimeSpan.MaxValue;
        }
    }

    /// <summary>Wrapper to simplify sending of inputs.</summary>
    /// <param name="inputs">Input array.</param>
    /// <returns>inputs send.</returns>
    public static uint SendInput(DesktopInput[] inputs) => GetCurrentApi().SendInput(inputs);

    /// <summary>Replaces the native input API for deterministic tests.</summary>
    /// <param name="api">The replacement native input API.</param>
    /// <returns>The previous native input API.</returns>
    internal static INativeInputApi SetApiForTesting(INativeInputApi api)
    {
        Throw.IfNull(api);
        var currentApi = GetCurrentApi();
        _apiOverride = ((api == WindowsNativeInputApi.Instance) ? null : api);
        return currentApi;
    }

    /// <summary>Gets the current native input API.</summary>
    /// <returns>The active native input API.</returns>
    private static INativeInputApi GetCurrentApi() => _apiOverride ?? WindowsNativeInputApi.Instance;
}
