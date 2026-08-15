// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if !NETSTANDARD2_0
namespace CP.ReactiveUI.Primitives.Windows.Integrations.Browser;

/// <summary>
/// Used to show an extended embedded web-browser. See the IOleCommandTarget
/// documentation for more information on this interface.
/// </summary>
public class ExtendedWebBrowser : WebBrowser
{
    /// <inheritdoc />
    protected override WebBrowserSiteBase CreateWebBrowserSiteBase() => new ExtendedWebBrowserSite(this);

    /// <summary>The extended web browser site implementation.</summary>
    protected class ExtendedWebBrowserSite : WebBrowserSite, IOleCommandTarget
    {
        /// <summary>The command identifier used when a script error was shown.</summary>
        private const int OleCmdDidShowScriptError = 40;

        /// <summary>The successful operation result.</summary>
        private const int Ok = 0;

        /// <summary>The unsupported OLE command result.</summary>
        private const int OleCmmdErrENotsupported = -2_147_221_248;

        /// <summary>The document host command handler identifier.</summary>
        private static readonly Guid CGID_DocHostCommandHandler = new("F38BC242-B950-11D1-8918-00C04FC2C836");

        /// <summary>Initializes a new instance of the <see cref="ExtendedWebBrowserSite"/> class.</summary>
        /// <param name="webBrowser">The web browser.</param>
        public ExtendedWebBrowserSite(WebBrowser webBrowser)
            : base(webBrowser)
        {
        }

        /// <inheritdoc />
        int IOleCommandTarget.QueryStatus(Guid commandGroup, int commandCount, IntPtr commands, IntPtr commandText) =>
            OleCmmdErrENotsupported;

        /// <inheritdoc />
        int IOleCommandTarget.Exec(Guid commandGroup, int commandId, int commandOptions, IntPtr input, IntPtr output) =>
            commandGroup == CGID_DocHostCommandHandler && commandId == OleCmdDidShowScriptError
                ? Ok
                : OleCmmdErrENotsupported;
    }
}
#endif
