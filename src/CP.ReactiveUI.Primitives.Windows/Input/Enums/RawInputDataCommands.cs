// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>
/// The commands to get the RawInputData
/// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645568.aspx">RAWINPUTDEVICELIST structure</a>
/// </summary>
public enum RawInputDataCommands : uint
{
    /// <summary>No raw input data command is selected.</summary>
    None = 0U,
    /// <summary>RID_INPUT: Get the raw data from the RAWINPUT structure.</summary>
    Input = 268_435_459U,
    /// <summary>RID_HEADER: Get the header information from the RAWINPUT structure.</summary>
    Header = 268_435_461U
}
