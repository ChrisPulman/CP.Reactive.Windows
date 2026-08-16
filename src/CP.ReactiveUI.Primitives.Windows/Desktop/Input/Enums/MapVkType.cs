// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
#endif
/// <summary>Specifies how virtual-key and scan-code values are translated.</summary>
internal enum MapVkType : uint
{
    /// <summary>
    ///     The uCode parameter is a virtual-key code and is translated into a scan code. If it is a virtual-key code that does
    ///     not distinguish between left- and right-hand keys, the left-hand scan code is returned. If there is no translation,
    ///     the function returns 0.
    /// </summary>
    VkToVsc,
    /// <summary>
    ///     The uCode parameter is a scan code and is translated into a virtual-key code that does not distinguish between
    ///     left- and right-hand keys. If there is no translation, the function returns 0.
    /// </summary>
    VscToVk,
    /// <summary>
    ///     The uCode parameter is a virtual-key code and is translated into an unshifted character value in the low order word
    ///     of the return value. Dead keys (diacritics) are indicated by setting the top bit of the return value. If there is
    ///     no translation, the function returns 0.
    /// </summary>
    VkToChar,
    /// <summary>
    ///     The uCode parameter is a scan code and is translated into a virtual-key code that distinguishes between left- and
    ///     right-hand keys. If there is no translation, the function returns 0.
    /// </summary>
    VscToVkEx,
    /// <summary>
    ///     The uCode parameter is a virtual-key code and is translated into a scan code. If it is a virtual-key code that does
    ///     not distinguish between left- and right-hand keys, the left-hand scan code is returned. If the scan code is an
    ///     extended scan code, the high byte of the uCode value can contain either 0xe0 or 0xe1 to specify the extended scan
    ///     code. If there is no translation, the function returns 0.
    /// </summary>
    VkToVscEx,
}
