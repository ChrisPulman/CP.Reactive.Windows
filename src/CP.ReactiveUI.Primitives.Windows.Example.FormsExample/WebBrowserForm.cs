// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;
using CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Forms;
using CP.ReactiveUI.Primitives.Windows.Integrations.Browser;

namespace CP.ReactiveUI.Primitives.Windows.Example.FormsExample;

/// <summary>Hosts an embedded Internet Explorer browser control.</summary>
public partial class WebBrowserForm : Form
{
    /// <summary>Writes browser navigation diagnostics.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(WebBrowserForm));

    /// <summary>Composes DPI-aware behavior onto the form.</summary>
    private readonly DpiAwareFormBehavior _dpiAwareBehavior;

    /// <summary>Initializes a new instance of the <see cref="WebBrowserForm"/> class.</summary>
    public WebBrowserForm()
    {
        InternetExplorerVersion.ChangeEmbeddedVersion();

        InitializeComponent();
        _dpiAwareBehavior = this.AttachDpiAwareBehavior();
        _ = extendedWebBrowser1.OnNavigating().Subscribe(static args =>
        {
            Log.Info(args.Url.AbsoluteUri);
        });
    }
}
