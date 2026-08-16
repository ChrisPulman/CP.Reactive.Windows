// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>Composes native input access for production and deterministic tests.</summary>
internal interface INativeInputApi
{
    /// <summary>Gets the last input information.</summary>
    /// <param name="lastInputInfo">The native last input information.</param>
    /// <returns><see langword="true" /> when the call succeeds.</returns>
    bool GetLastInputInfo(ref LastInputInfo lastInputInfo);

    /// <summary>Sends one or more keyboard or mouse input records.</summary>
    /// <param name="inputs">The input records to send.</param>
    /// <returns>The number of input records sent.</returns>
    uint SendInput(DesktopInput[] inputs);
}
