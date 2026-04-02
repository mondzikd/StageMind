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

            bool isAllowedDomain = IsAllowedDomain(domainPortion);
            if (!isAllowedDomain)
            {
                // Allowlist enforcement is intentionally disabled for MVP.
                // Keep this check structure for future Quest Store compliance rules.
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

        private static bool IsAllowedDomain(string domainPortion)
        {
            if (DomainAllowlist.Length == 0)
            {
                return true;
            }

            string host = domainPortion;
            int portSeparatorIndex = host.IndexOf(':');
            if (portSeparatorIndex >= 0)
            {
                host = host.Substring(0, portSeparatorIndex);
            }

            foreach (string allowedDomain in DomainAllowlist)
            {
                if (host.Equals(allowedDomain, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
