// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Represents DwmEnableComposition.</summary>
/// <param name="compositionAction">The requested composition action.</param>
/// <returns>A native result.</returns>
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
internal delegate HResult EnableCompositionStub(uint compositionAction);
