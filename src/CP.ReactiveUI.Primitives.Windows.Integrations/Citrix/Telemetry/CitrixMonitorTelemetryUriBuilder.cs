// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Telemetry;

/// <summary>Builds Citrix Monitor Service OData URIs for telemetry requests.</summary>
public sealed class CitrixMonitorTelemetryUriBuilder
{
    /// <summary>The Monitor Service base URI.</summary>
    private readonly Uri _baseUri;

    /// <summary>Initializes a new instance of the <see cref="CitrixMonitorTelemetryUriBuilder"/> class.</summary>
    /// <param name="baseUri">
    /// The Monitor Service base URI, for example <c>https://api.cloud.com/monitorodata/</c>
    /// or <c>https://server/Citrix/Monitor/OData/v4/Data/</c>.
    /// </param>
    public CitrixMonitorTelemetryUriBuilder(Uri baseUri)
    {
        Throw.IfNull(baseUri);
        _baseUri = EnsureTrailingSlash(baseUri);
    }

    /// <summary>Creates an absolute request URI from a telemetry request.</summary>
    /// <param name="request">The telemetry request.</param>
    /// <returns>The absolute request URI.</returns>
    public Uri CreateRequestUri(CitrixMonitorTelemetryRequest request)
    {
        Throw.IfNull(request);
        var relativePath = string.IsNullOrWhiteSpace(request.Query)
            ? request.Entity
            : $"{request.Entity}{NormalizeQuery(request.Query)}";
        return new(_baseUri, relativePath);
    }

    /// <summary>Ensures a URI can be used as a path-combining base URI.</summary>
    /// <param name="uri">The source URI.</param>
    /// <returns>The source URI with a trailing slash.</returns>
    private static Uri EnsureTrailingSlash(Uri uri)
    {
        var text = uri.AbsoluteUri;
        return EndsWithSlash(text)
            ? uri
            : new($"{text}/", UriKind.Absolute);
    }

    /// <summary>Checks whether text ends with a URI slash without requiring modern-only APIs on legacy targets.</summary>
    /// <param name="text">The text to check.</param>
    /// <returns><see langword="true"/> when the text ends with <c>/</c>.</returns>
    private static bool EndsWithSlash(string text)
    {
#if NETFRAMEWORK
        return text.EndsWith("/", StringComparison.Ordinal);
#else
        return text.EndsWith('/');
#endif
    }

    /// <summary>Normalizes a supplied OData query string.</summary>
    /// <param name="query">The query string.</param>
    /// <returns>The normalized query string.</returns>
    private static string NormalizeQuery(string query)
    {
        var trimmed = query.TrimStart();
        return trimmed.Length > 0 && trimmed[0] == '?'
            ? trimmed
            : $"?{trimmed}";
    }
}
