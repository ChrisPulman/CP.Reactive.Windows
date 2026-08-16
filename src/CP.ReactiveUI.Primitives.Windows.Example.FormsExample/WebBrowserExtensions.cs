// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;
using ReactiveUI.Primitives.Signals;

namespace CP.ReactiveUI.Primitives.Windows.Example.FormsExample;

/// <summary>Provides observable WebBrowser event helpers.</summary>
public static class WebBrowserExtensions
{
    /// <summary>Create an observable for the navigating events</summary>
    /// <param name="webBrowser">The browser whose navigation events to observe.</param>
    /// <returns>IObservable which publishes WebBrowserNavigatingEventArgs</returns>
    extension(WebBrowser webBrowser)
    {
        /// <summary>Create an observable for the navigating events.</summary>
        /// <returns>IObservable which publishes WebBrowserNavigatingEventArgs.</returns>
        public IObservable<WebBrowserNavigatingEventArgs> OnNavigating() => Signal.CreateWithState<WebBrowserNavigatingEventArgs, WebBrowser>(webBrowser, static (browser, observer) =>
            {
                void Handler(object _, WebBrowserNavigatingEventArgs args) => observer.OnNext(args);

                browser.Navigating += Handler;
                return new NavigatingSubscription(browser, Handler);
            });
    }

    /// <summary>Removes a navigation event handler when the subscription is disposed.</summary>
    /// <param name="browser">The browser that owns the event.</param>
    /// <param name="handler">The handler to detach.</param>
    private sealed class NavigatingSubscription(WebBrowser browser, WebBrowserNavigatingEventHandler handler) : IDisposable
    {
        /// <summary>Detaches the event handler.</summary>
        public void Dispose() => browser.Navigating -= handler;
    }
}
