using System;

namespace Azunt.Utilities.Navigation
{
    /// <summary>
    /// Provides helper methods to normalize and validate ReturnUrl values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This utility is framework-agnostic and compatible with netstandard2.0+.
    /// It produces a safe root-relative path (e.g., "/dashboard?x=1").
    /// </para>
    /// <para>
    /// Security note: It blocks absolute URLs (e.g., "https://azunt.com") and scheme-relative URLs
    /// (e.g., "//azunt.com") to reduce the risk of open redirect vulnerabilities.
    /// </para>
    /// </remarks>
    public static class ReturnUrlUtility
    {
        /// <summary>
        /// The default fallback path when the ReturnUrl is null, empty, or invalid.
        /// </summary>
        public const string DefaultPath = "/";

        /// <summary>
        /// Normalizes a ReturnUrl into a safe root-relative path.
        /// </summary>
        /// <param name="returnUrl">The input ReturnUrl to normalize.</param>
        /// <param name="defaultPath">The fallback path to use when the input is invalid. Default is "/".</param>
        /// <returns>A safe root-relative path such as "/".</returns>
        /// <example>
        /// "dashboard"    -> "/dashboard"
        /// "~/dashboard"  -> "/dashboard"
        /// "/dashboard"   -> "/dashboard"
        /// "https://..."  -> "/"
        /// "//azunt.com"  -> "/"
        /// </example>
        public static string Normalize(string? returnUrl, string defaultPath = DefaultPath)
        {
            defaultPath = EnsureRootRelative(defaultPath);

            if (string.IsNullOrWhiteSpace(returnUrl))
                return defaultPath;

            var value = returnUrl.Trim();

            // Block absolute URLs: "https://azunt.com/..."
            if (Uri.TryCreate(value, UriKind.Absolute, out _))
                return defaultPath;

            // Block scheme-relative URLs: "//azunt.com/..."
            if (value.StartsWith("//", StringComparison.Ordinal))
                return defaultPath;

            // "~/" (ASP.NET token) -> "/"
            if (StartsWithTildeSlash(value))
                value = value.Substring(1); // remove "~" only

            // "~" -> "/"
            if (value == "~")
                value = "/";

            value = EnsureRootRelative(value);

            // Just in case: "\" -> "/"
            value = value.Replace('\\', '/');

            return value;
        }

        /// <summary>
        /// Normalizes a ReturnUrl and additionally validates it against the specified base URI.
        /// </summary>
        /// <param name="returnUrl">The input ReturnUrl to normalize.</param>
        /// <param name="baseUri">
        /// The application's base URI (e.g., "https://example.com/app/") used to ensure the result stays within the app.
        /// </param>
        /// <param name="defaultPath">The fallback path to use when the input is invalid. Default is "/".</param>
        /// <returns>
        /// A safe root-relative path when valid; otherwise the normalized <paramref name="defaultPath"/>.
        /// </returns>
        /// <remarks>
        /// This overload strengthens validation by ensuring the combined URL remains on the same host and port.
        /// </remarks>
        public static string Normalize(string? returnUrl, Uri baseUri, string defaultPath = DefaultPath)
        {
            if (baseUri == null) throw new ArgumentNullException(nameof(baseUri));

            var normalized = Normalize(returnUrl, defaultPath);

            var combined = new Uri(baseUri, normalized);

            // Ensure same host and port to prevent cross-domain redirects.
            if (!string.Equals(combined.Host, baseUri.Host, StringComparison.OrdinalIgnoreCase) ||
                combined.Port != baseUri.Port)
            {
                return EnsureRootRelative(defaultPath);
            }

            return normalized;
        }

        /// <summary>
        /// Returns true if the input starts with "~/" or "~\".
        /// </summary>
        private static bool StartsWithTildeSlash(string value)
            => value.Length >= 2 && value[0] == '~' && (value[1] == '/' || value[1] == '\\');

        /// <summary>
        /// Ensures the path is root-relative (starts with "/") and normalizes basic ASP.NET "~/" token.
        /// </summary>
        private static string EnsureRootRelative(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "/";

            path = path.Trim().Replace('\\', '/');

            if (path.StartsWith("~/", StringComparison.Ordinal))
                path = path.Substring(1); // "~/" -> "/"

            if (path == "~")
                path = "/";

            if (!path.StartsWith("/", StringComparison.Ordinal))
                path = "/" + path;

            return path;
        }
    }
}
