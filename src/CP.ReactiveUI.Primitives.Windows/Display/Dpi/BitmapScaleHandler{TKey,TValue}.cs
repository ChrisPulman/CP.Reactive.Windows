// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
#endif
/// <summary>Provides disposable bitmap values scaled for the current DPI.</summary>
/// <typeparam name="TKey">The bitmap key type.</typeparam>
/// <typeparam name="TValue">The disposable bitmap value type.</typeparam>
public sealed class BitmapScaleHandler<TKey, TValue> : IDisposable
    where TValue : IDisposable
{
    /// <summary>Synchronizes access to cached images.</summary>
    private readonly ReaderWriterLockSlim _imagesLock = new();

    /// <summary>Synchronizes access to apply actions.</summary>
    private readonly ReaderWriterLockSlim _actionsLock = new();

    /// <summary>Cached images by key.</summary>
    private readonly Dictionary<TKey, TValue> _images = new();

    /// <summary>Registered image apply actions by target.</summary>
    private readonly Dictionary<object, Action> _applyActions = new();

    /// <summary>Indicates that disposal is in progress.</summary>
    private bool _isDisposing;

    /// <summary>The current DPI value.</summary>
    private int _dpi;

    /// <summary>The DPI change subscription.</summary>
    private IDisposable _dpiChangeSubscription;

    /// <summary>The bitmap provider.</summary>
    private Func<TKey, int, TValue> _bitmapProvider;

    /// <summary>The optional bitmap scaler.</summary>
    private Func<TValue, int, TValue> _bitmapScaler;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.BitmapScaleHandler`2" /> class.</summary>
    internal BitmapScaleHandler()
    {
    }

    /// <summary>Add an action which applies a bitmap.</summary>
    /// <param name="apply">Action which assigns a bitmap.</param>
    /// <param name="imageKey">Key of the image.</param>
    /// <returns>The current bitmap scale handler.</returns>
    public BitmapScaleHandler<TKey, TValue> AddApplyAction(Action<TValue> apply, TKey imageKey) => AddApplyAction(apply, imageKey, execute: false);

    /// <summary>Add an action which applies a bitmap.</summary>
    /// <param name="apply">Action which assigns a bitmap.</param>
    /// <param name="imageKey">Key of the image.</param>
    /// <param name="execute">A value indicating whether the assignment is executed immediately.</param>
    /// <returns>The current bitmap scale handler.</returns>
    public BitmapScaleHandler<TKey, TValue> AddApplyAction(Action<TValue> apply, TKey imageKey, bool execute)
    {
        try
        {
            _actionsLock.EnterWriteLock();
            _applyActions[apply] = ApplyAction;
        }
        finally
        {
            _actionsLock.ExitWriteLock();
        }

        if (execute)
        {
            ApplyAction();
        }

        return this;
        void ApplyAction() => apply(GetBitmap(imageKey));
    }

    /// <summary>Add a button as a bitmap target.</summary>
    /// <param name="button">The target button.</param>
    /// <param name="imageKey">Key of the image.</param>
    /// <param name="valueConverter">Function that converts the value to a bitmap.</param>
    /// <returns>The current bitmap scale handler.</returns>
    public BitmapScaleHandler<TKey, TValue> AddTarget(Button button, TKey imageKey, Func<TValue, Bitmap> valueConverter) => AddTarget(button, imageKey, valueConverter, execute: false);

    /// <summary>Add a button as a bitmap target.</summary>
    /// <param name="button">The target button.</param>
    /// <param name="imageKey">Key of the image.</param>
    /// <param name="valueConverter">Function that converts the value to a bitmap.</param>
    /// <param name="execute">A value indicating whether the assignment is executed immediately.</param>
    /// <returns>The current bitmap scale handler.</returns>
    public BitmapScaleHandler<TKey, TValue> AddTarget(Button button, TKey imageKey, Func<TValue, Bitmap> valueConverter, bool execute)
    {
        return AddTarget(button, ApplyAction, execute);
        void ApplyAction() => button.Image = valueConverter(GetBitmap(imageKey));
    }

    /// <summary>Add a tool strip item as a bitmap target.</summary>
    /// <param name="toolStripItem">The target tool strip item.</param>
    /// <param name="imageKey">Key of the image.</param>
    /// <param name="valueConverter">Function that converts the value to a bitmap.</param>
    /// <returns>The current bitmap scale handler.</returns>
    public BitmapScaleHandler<TKey, TValue> AddTarget(ToolStripItem toolStripItem, TKey imageKey, Func<TValue, Bitmap> valueConverter) => AddTarget(toolStripItem, imageKey, valueConverter, execute: false);

    /// <summary>Add a tool strip item as a bitmap target.</summary>
    /// <param name="toolStripItem">The target tool strip item.</param>
    /// <param name="imageKey">Key of the image.</param>
    /// <param name="valueConverter">Function that converts the value to a bitmap.</param>
    /// <param name="execute">A value indicating whether the assignment is executed immediately.</param>
    /// <returns>The current bitmap scale handler.</returns>
    public BitmapScaleHandler<TKey, TValue> AddTarget(ToolStripItem toolStripItem, TKey imageKey, Func<TValue, Bitmap> valueConverter, bool execute)
    {
        return AddTarget(toolStripItem, ApplyAction, execute);
        void ApplyAction() => toolStripItem.Image = valueConverter(GetBitmap(imageKey));
    }

    /// <summary>Dispose implementation.</summary>
    public void Dispose()
    {
        _dpiChangeSubscription?.Dispose();
        ReleaseResources();
    }

    /// <summary>Remove a previously added target.</summary>
    /// <param name="target">The target to remove.</param>
    /// <returns>The current bitmap scale handler.</returns>
    public BitmapScaleHandler<TKey, TValue> RemoveTarget(object target)
    {
        try
        {
            _actionsLock.EnterWriteLock();
            _ = _applyActions.Remove(target);
        }
        finally
        {
            _actionsLock.ExitWriteLock();
        }

        return this;
    }

    /// <summary>Initializes the handler.</summary>
    /// <param name="dpiHandler">DPI handler.</param>
    /// <param name="bitmapProvider">A function which provides the requested bitmap.</param>
    /// <param name="bitmapScaler">A function to provide a newly scaled bitmap.</param>
    internal void Initialize(DpiHandler dpiHandler, Func<TKey, int, TValue> bitmapProvider, Func<TValue, int, TValue> bitmapScaler)
    {
        _bitmapProvider = bitmapProvider;
        _bitmapScaler = bitmapScaler;
        _dpiChangeSubscription = dpiHandler.ObserveDpiChanges().Subscribe(ProcessDpiChange);
    }

    /// <summary>Adds a target apply action.</summary>
    /// <param name="target">The target object.</param>
    /// <param name="applyAction">The action to run.</param>
    /// <param name="execute">A value indicating whether the assignment is executed immediately.</param>
    /// <returns>The current bitmap scale handler.</returns>
    private BitmapScaleHandler<TKey, TValue> AddTarget(object target, Action applyAction, bool execute)
    {
        try
        {
            _actionsLock.EnterWriteLock();
            _applyActions[target] = applyAction;
        }
        finally
        {
            _actionsLock.ExitWriteLock();
        }

        if (execute)
        {
            applyAction();
        }

        return this;
    }

    /// <summary>Get bitmaps for displaying.</summary>
    /// <param name="imageKey">The image key.</param>
    /// <returns>The bitmap value.</returns>
    private TValue GetBitmap(TKey imageKey)
    {
        if (_isDisposing || _bitmapProvider is null)
        {
            return default;
        }

        try
        {
            _imagesLock.EnterUpgradeableReadLock();
            if (_images.TryGetValue(imageKey, out var result))
            {
                return result;
            }

            TValue image = _bitmapProvider(imageKey, _dpi);
            if (image is null)
            {
                return default;
            }

            result = ((_bitmapScaler is null) ? image : _bitmapScaler(image, _dpi));
            if (result is null)
            {
                return default;
            }

            try
            {
                _imagesLock.EnterWriteLock();
                _images.Add(imageKey, result);
            }
            finally
            {
                _imagesLock.ExitWriteLock();
            }

            return result;
        }
        finally
        {
            _imagesLock.ExitUpgradeableReadLock();
        }
    }

    /// <summary>Processes DPI Change information.</summary>
    /// <param name="dpiChangeInfo">DPI change information.</param>
    private void ProcessDpiChange(DpiChangeInfo dpiChangeInfo)
    {
        List<TValue> imagesToDispose = new();
        try
        {
            _imagesLock.EnterWriteLock();
            foreach (TValue image in _images.Values)
            {
                imagesToDispose.Add(image);
            }

            _images.Clear();
        }
        finally
        {
            _imagesLock.ExitWriteLock();
        }

        _dpi = dpiChangeInfo.NewDpi;
        try
        {
            _actionsLock.EnterReadLock();
            foreach (Action value in _applyActions.Values)
            {
                value();
            }
        }
        finally
        {
            _actionsLock.ExitReadLock();
        }

        foreach (TValue item in imagesToDispose)
        {
            item.Dispose();
        }
    }

    /// <summary>Cleanup the images, they are no longer needed.</summary>
    private void ReleaseResources()
    {
        _isDisposing = true;
        try
        {
            _actionsLock.EnterWriteLock();
            _applyActions.Clear();
        }
        finally
        {
            _actionsLock.ExitWriteLock();
        }

        try
        {
            _imagesLock.EnterWriteLock();
            foreach (TValue value in _images.Values)
            {
                value.Dispose();
            }

            _images.Clear();
        }
        finally
        {
            _imagesLock.ExitWriteLock();
        }

        _imagesLock.Dispose();
        _actionsLock.Dispose();
    }
}
