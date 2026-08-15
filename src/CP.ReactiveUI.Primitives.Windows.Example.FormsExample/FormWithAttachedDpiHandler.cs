// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Drawing;
using System.Windows.Forms;
using CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
using CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Forms;

namespace CP.ReactiveUI.Primitives.Windows.Example.FormsExample;

/// <summary>This extends the form with extra DPI aware capabilities.</summary>
public partial class FormWithAttachedDpiHandler : Form
{
    /// <summary>Writes DPI diagnostics.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(FormWithAttachedDpiHandler));

    /// <summary>Scales menu images for DPI changes.</summary>
    private BitmapScaleHandler<string, Bitmap> _scaleHandler;

    /// <summary>Handles form DPI changes.</summary>
    private DpiHandler _formDpiHandler;

    /// <summary>Observes form DPI changes.</summary>
    private IDisposable _dpiChangeSubscription;

    /// <summary>Shows environment update notifications.</summary>
    private EnvironmentUpdateDialogBehavior _environmentUpdateDialog;

    /// <summary>Initializes a new instance of the <see cref="FormWithAttachedDpiHandler"/> class.</summary>
    public FormWithAttachedDpiHandler()
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
        if (_formDpiHandler is not null)
        {
            return;
        }

        _formDpiHandler = this.AttachDpiHandler();
        var initialMenuStripSize = menuStrip1.ImageScalingSize;
        _ = _formDpiHandler.ObserveDpiChanges().Subscribe(dpiChangeInfo =>
        {
            menuStrip1.ImageScalingSize = DpiCalculator.ScaleWithDpi(initialMenuStripSize, dpiChangeInfo.NewDpi);
        });

        _scaleHandler = BitmapScaleHandler.WithComponentResourceManager<Bitmap>(
            _formDpiHandler,
            GetType(),
            BitmapScaleHandler.SimpleBitmapScaler)
            .AddTarget(somethingMenuItem, "somethingMenuItem.Image", static b => b)
            .AddTarget(something2MenuItem, "something2MenuItem.Image", static b => b);

        _dpiChangeSubscription = _formDpiHandler.ObserveDpiChanges().Subscribe(static dpi =>
        {
            Log.InfoFormat("New DPI: {0}", dpi);
        });
    }
}
