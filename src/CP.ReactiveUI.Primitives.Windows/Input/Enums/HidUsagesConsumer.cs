// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>The different consumer HID usages See <a href="http://www.usb.org/developers/hidpage/Hut1_12v2.pdf">here</a></summary>
public enum HidUsagesConsumer
{
    /// <summary>No HID usage.</summary>
    None,
    /// <summary>Consumer Control.</summary>
    ConsumerControl
}
