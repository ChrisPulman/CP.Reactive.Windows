// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Drawing;
using System.Windows.Forms;
using CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
using CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Forms;

namespace CP.ReactiveUI.Primitives.Windows.Example.FormsExample;

/// <summary>Demonstrates a DPI-aware form composed with <see cref="DpiAwareFormBehavior"/>.</summary>
public partial class FormExtendsDpiAwareForm : Form
{
    /// <summary>Writes DPI diagnostics.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(FormExtendsDpiAwareForm));

    /// <summary>Scales menu images for DPI changes.</summary>
    private BitmapScaleHandler<string, Bitmap> _scaleHandler;

    /// <summary>Composes DPI-aware behavior onto the form.</summary>
    private DpiAwareFormBehavior _dpiAwareBehavior;

    /// <summary>Handles context-menu DPI changes.</summary>
    private DpiHandler _contextMenuDpiHandler;

    /// <summary>Observes context-menu DPI changes.</summary>
    private IDisposable _dpiChangeSubscription;

    /// <summary>Shows environment update notifications.</summary>
    private EnvironmentUpdateDialogBehavior _environmentUpdateDialog;

    /// <summary>Initializes a new instance of the <see cref="FormExtendsDpiAwareForm"/> class.</summary>
    public FormExtendsDpiAwareForm()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        EnsureDpiConfiguration();
        _environmentUpdateDialog ??= new(this, Log);
    }

    /// <summary>Configures DPI behaviors after construction has completed.</summary>
    private void EnsureDpiConfiguration()
    {
        if (_dpiAwareBehavior is not null)
        {
            return;
        }

        _dpiAwareBehavior = this.AttachDpiAwareBehavior();
        _contextMenuDpiHandler = contextMenuStrip1.AttachDpiHandler();
        _dpiChangeSubscription = _contextMenuDpiHandler.ObserveDpiChanges().Subscribe(static dpi =>
        {
            Log.InfoFormat("ContextMenuStrip DPI: {0}", dpi.NewDpi);
        });

        var initialMenuStripSize = menuStrip1.ImageScalingSize;
        _ = _dpiAwareBehavior.DpiHandler.ObserveDpiChanges().Subscribe(dpiChangeInfo =>
        {
            menuStrip1.ImageScalingSize = DpiCalculator.ScaleWithDpi(initialMenuStripSize, dpiChangeInfo.NewDpi);
        });

        _scaleHandler = BitmapScaleHandler.WithComponentResourceManager<Bitmap>(
            _dpiAwareBehavior.DpiHandler,
            GetType(),
            BitmapScaleHandler.SimpleBitmapScaler)
            .AddTarget(somethingMenuItem, "somethingMenuItem.Image", static b => b)
            .AddTarget(something2MenuItem, "something2MenuItem.Image", static b => b);
    }

    /// <summary>Shows the form's context menu.</summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    private void Button1_Click(object sender, EventArgs e) => contextMenuStrip1.Show();
}
