// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Drawing;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles;

/// <summary>Provides a safe device-context handle for a <see cref="T:System.Drawing.Graphics" /> instance.</summary>
public class SafeGraphicsDcHandle : SafeDcHandle
{
    /// <summary>Indicates whether the graphics instance is disposed with the handle.</summary>
    private readonly bool _disposeGraphics;

    /// <summary>Stores the composed action that releases the graphics device context.</summary>
    private readonly Action<IntPtr> _releaseDeviceContext;

    /// <summary>Stores the composed action that disposes the graphics instance.</summary>
    private readonly Action _disposeGraphicsAction;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeGraphicsDcHandle" /> class.</summary>
    public SafeGraphicsDcHandle()
        : base(ownsHandle: true) { }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeGraphicsDcHandle" /> class from composed release actions.</summary>
    /// <param name="preexistingHandle">The synthetic or existing device-context handle.</param>
    /// <param name="disposeGraphics">Indicates whether the composed dispose action is invoked.</param>
    /// <param name="releaseDeviceContext">The action that releases the device context.</param>
    /// <param name="disposeGraphicsAction">The action that disposes the graphics owner.</param>
    internal SafeGraphicsDcHandle(
        IntPtr preexistingHandle,
        bool disposeGraphics,
        Action<IntPtr> releaseDeviceContext,
        Action disposeGraphicsAction)
        : base(ownsHandle: true)
    {
        Throw.IfNull(releaseDeviceContext);
        Throw.IfNull(disposeGraphicsAction);
        _releaseDeviceContext = releaseDeviceContext;
        _disposeGraphicsAction = disposeGraphicsAction;
        _disposeGraphics = disposeGraphics;
        SetHandle(preexistingHandle);
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeGraphicsDcHandle" /> class from a graphics device context.</summary>
    /// <param name="graphics">The graphics instance that owns the device context.</param>
    /// <param name="preexistingHandle">The existing device-context handle.</param>
    /// <param name="disposeGraphics">Indicates whether to dispose the graphics instance when this handle is released.</param>
    private SafeGraphicsDcHandle(Graphics graphics, IntPtr preexistingHandle, bool disposeGraphics)
        : base(ownsHandle: true)
    {
        _releaseDeviceContext = graphics.ReleaseHdc;
        _disposeGraphicsAction = graphics.Dispose;
        _disposeGraphics = disposeGraphics;
        SetHandle(preexistingHandle);
    }

    /// <summary>Creates a safe device-context handle that does not dispose the graphics instance.</summary>
    /// <param name="graphics">The graphics instance from which to obtain the device context.</param>
    /// <returns>A safe handle for the graphics device context.</returns>
    public static SafeGraphicsDcHandle FromGraphics(Graphics graphics) =>
        FromGraphics(graphics, disposeGraphics: false);

    /// <summary>Creates a safe device-context handle for a graphics instance.</summary>
    /// <param name="graphics">The graphics instance from which to obtain the device context.</param>
    /// <param name="disposeGraphics">Indicates whether to dispose <paramref name="graphics" /> when this handle is released.</param>
    /// <returns>A safe handle for the graphics device context.</returns>
    public static SafeGraphicsDcHandle FromGraphics(Graphics graphics, bool disposeGraphics)
    {
        Throw.IfNull(graphics);
        return new(graphics, graphics.GetHdc(), disposeGraphics);
    }

    /// <summary>Selects an object into this device context.</summary>
    /// <param name="newHandle">The object to select.</param>
    /// <returns>A handle that restores the previously selected object when disposed.</returns>
    public SafeSelectObjectHandle SelectObject(SafeHandle newHandle) => new(this, newHandle);

    /// <inheritdoc />
    protected override bool ReleaseHandle()
    {
        _releaseDeviceContext(handle);
        if (_disposeGraphics)
        {
            _disposeGraphicsAction();
        }

        return true;
    }
}
