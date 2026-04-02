using NUnit.Framework;

namespace StageMind.Tests.EditMode
{
    [TestFixture]
    public class UrlValidatorTests
    {
        [Test]
        public void ValidateAndNormalize_ValidHttpsUrl_PassesUnchanged()
        {
            var (isValid, normalizedUrl, errorMessage) = UrlValidator.ValidateAndNormalize("https://slides.google.com/deck");

            Assert.IsTrue(isValid);
            Assert.AreEqual("https://slides.google.com/deck", normalizedUrl);
            Assert.IsNull(errorMessage);
        }

        [Test]
        public void ValidateAndNormalize_ValidHttpUrl_PassesUnchanged()
        {
            var (isValid, normalizedUrl, errorMessage) = UrlValidator.ValidateAndNormalize("http://example.com");

            Assert.IsTrue(isValid);
            Assert.AreEqual("http://example.com", normalizedUrl);
            Assert.IsNull(errorMessage);
        }

        [Test]
        public void ValidateAndNormalize_NoProtocol_PrependsHttps()
        {
            var (isValid, normalizedUrl, errorMessage) = UrlValidator.ValidateAndNormalize("slides.google.com/deck");

            Assert.IsTrue(isValid);
            Assert.AreEqual("https://slides.google.com/deck", normalizedUrl);
            Assert.IsNull(errorMessage);
        }

        [Test]
        public void ValidateAndNormalize_JavascriptProtocol_Rejected()
        {
            var (isValid, _, _) = UrlValidator.ValidateAndNormalize("javascript:alert(1)");

            Assert.IsFalse(isValid);
        }

        [Test]
        public void ValidateAndNormalize_JavascriptProtocol_WithDomain_PrependsHttps()
        {
            var (isValid, normalizedUrl, _) = UrlValidator.ValidateAndNormalize("javascript:slides.google.com");

            Assert.IsTrue(isValid);
            Assert.AreEqual("https://slides.google.com", normalizedUrl);
        }

        [Test]
        public void ValidateAndNormalize_FileProtocol_Rejected()
        {
            var (isValid, _, _) = UrlValidator.ValidateAndNormalize("file:///etc/passwd");

            Assert.IsFalse(isValid);
        }

        [Test]
        public void ValidateAndNormalize_FtpProtocol_StrippedAndPrependsHttps()
        {
            var (isValid, normalizedUrl, _) = UrlValidator.ValidateAndNormalize("ftp://example.com");

            Assert.IsTrue(isValid);
            Assert.AreEqual("https://example.com", normalizedUrl);
        }

        [Test]
        public void ValidateAndNormalize_DataProtocol_Rejected()
        {
            var (isValid, _, _) = UrlValidator.ValidateAndNormalize("data:text/html,<h1>hi</h1>");

            Assert.IsFalse(isValid);
        }

        [Test]
        public void ValidateAndNormalize_NoDot_Rejected()
        {
            var (isValid, normalizedUrl, errorMessage) = UrlValidator.ValidateAndNormalize("hello");

            Assert.IsFalse(isValid);
            Assert.IsNull(normalizedUrl);
            Assert.IsNotNull(errorMessage);
        }

        [Test]
        public void ValidateAndNormalize_NoDot_Localhost_Rejected()
        {
            var (isValid, normalizedUrl, errorMessage) = UrlValidator.ValidateAndNormalize("localhost");

            Assert.IsFalse(isValid);
            Assert.IsNull(normalizedUrl);
            Assert.IsNotNull(errorMessage);
        }

        [Test]
        public void ValidateAndNormalize_WithDot_Passes()
        {
            var (isValid, normalizedUrl, errorMessage) = UrlValidator.ValidateAndNormalize("example.com");

            Assert.IsTrue(isValid);
            Assert.AreEqual("https://example.com", normalizedUrl);
            Assert.IsNull(errorMessage);
        }

        [Test]
        public void ValidateAndNormalize_Empty_Rejected()
        {
            var (isValid, normalizedUrl, errorMessage) = UrlValidator.ValidateAndNormalize("");

            Assert.IsFalse(isValid);
            Assert.IsNull(normalizedUrl);
            Assert.IsNotNull(errorMessage);
        }

        [Test]
        public void ValidateAndNormalize_WhitespaceOnly_Rejected()
        {
            var (isValid, normalizedUrl, errorMessage) = UrlValidator.ValidateAndNormalize("   ");

            Assert.IsFalse(isValid);
            Assert.IsNull(normalizedUrl);
            Assert.IsNotNull(errorMessage);
        }

        [Test]
        public void ValidateAndNormalize_UrlWithPath_PrependsHttps()
        {
            var (isValid, normalizedUrl, errorMessage) = UrlValidator.ValidateAndNormalize("docs.google.com/presentation/d/abc/pub");

            Assert.IsTrue(isValid);
            Assert.AreEqual("https://docs.google.com/presentation/d/abc/pub", normalizedUrl);
            Assert.IsNull(errorMessage);
        }

        [Test]
        public void ValidateAndNormalize_SlidesGoogleCom_Passes()
        {
            var (isValid, normalizedUrl, errorMessage) = UrlValidator.ValidateAndNormalize("slides.google.com");

            Assert.IsTrue(isValid);
            Assert.AreEqual("https://slides.google.com", normalizedUrl);
            Assert.IsNull(errorMessage);
        }

        [Test]
        public void ValidateAndNormalize_Null_Rejected()
        {
            var (isValid, normalizedUrl, errorMessage) = UrlValidator.ValidateAndNormalize(null);

            Assert.IsFalse(isValid);
            Assert.IsNull(normalizedUrl);
            Assert.IsNotNull(errorMessage);
        }
    }
}
