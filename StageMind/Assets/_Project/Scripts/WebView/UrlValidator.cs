using System;

namespace StageMind
{
    public static class UrlValidator
    {
        private static readonly string[] BlockedProtocols = { "javascript:", "file:", "ftp:", "data:" };
        private static readonly string[] DomainAllowlist = { };

        public static (bool isValid, string normalizedUrl, string errorMessage) ValidateAndNormalize(string input)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
            {
                return (false, null, "Paste your slide link above to get started.");
            }

            string trimmed = input.Trim();

            foreach (string protocol in BlockedProtocols)
            {
                if (trimmed.StartsWith(protocol, StringComparison.OrdinalIgnoreCase))
                {
                    trimmed = trimmed.Substring(protocol.Length).TrimStart('/');
                    break;
                }
            }

            if (!trimmed.Contains("://"))
            {
                trimmed = "https://" + trimmed;
            }

            string domainPortion = ExtractDomainPortion(trimmed);
            if (string.IsNullOrEmpty(domainPortion) || !domainPortion.Contains("."))
            {
                return (false, null, "That doesn't look like a link. Try pasting the full URL from your browser.");
            }

            return (true, trimmed, null);
        }

        private static string ExtractDomainPortion(string url)
        {
            int schemeEnd = url.IndexOf("://", StringComparison.Ordinal);
            if (schemeEnd < 0) return url;

            string afterScheme = url.Substring(schemeEnd + 3);
            int pathStart = afterScheme.IndexOf('/');
            return pathStart >= 0 ? afterScheme.Substring(0, pathStart) : afterScheme;
        }
    }
}
