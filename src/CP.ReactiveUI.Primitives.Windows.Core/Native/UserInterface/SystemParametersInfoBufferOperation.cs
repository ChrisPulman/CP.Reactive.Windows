// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface;

/// <summary>Represents a system-parameters buffer operation.</summary>
/// <param name="action">The system parameter action.</param>
/// <param name="parameterValue">The parameter value.</param>
/// <param name="value">The mutable UTF-16 buffer.</param>
/// <param name="behavior">The update behavior.</param>
/// <returns>True when the operation succeeded.</returns>
internal delegate bool SystemParametersInfoBufferOperation(
    SystemParametersInfoActions action,
    uint parameterValue,
    IntPtr value,
    SystemParametersInfoBehaviors behavior);
