#if VUPLEX_WEBVIEW
using System;
using System.Collections;
using UnityEngine;
using Vuplex.WebView;

namespace StageMind
{
    public class VuplexWebViewController : MonoBehaviour, IWebViewController
    {
        private const float PageLoadTimeoutSeconds = 15f;
        private const float CrashRetryWindowSeconds = 10f;

        private IWebView _webView;
        private RenderTexture _targetRenderTexture;
        private bool _isDirty;
        private bool _isLoading;
        private bool _isReady;
        private string _requestedUrlDomain;
        private Coroutine _loadTimeoutCoroutine;
        private float _lastRecoveryAttemptTime = -CrashRetryWindowSeconds;
        private bool _hasReportedLoadFailure;

        private static readonly string[] AuthDomains =
        {
            "accounts.google.com",
            "login.microsoftonline.com",
            "auth.canva.com",
            "login.live.com"
        };

        public event Action<string> OnLoadSuccess;
        public event Action<WebViewError> OnLoadError;
        public event Action OnCrash;
        public event Action<KeyCode, bool> OnKeyEventResult;

        public bool IsLoading => _isLoading;
        public bool IsReady => _isReady;

        public async void Initialize(RenderTexture targetTexture)
        {
            _targetRenderTexture = targetTexture;

            if (_webView != null)
            {
                Debug.LogWarning("[VuplexWebViewController] Already initialized. Skipping.");
                return;
            }

            try
            {
                _webView = await Web.CreateWebView();
                await _webView.Init(targetTexture.width, targetTexture.height);
                SubscribeToEvents();
                _isReady = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VuplexWebViewController] Initialization failed: {ex.Message}");
                _isReady = false;
                OnLoadError?.Invoke(WebViewError.Unknown);
            }
        }

        public void LoadUrl(string url)
        {
            if (!_isReady || _webView == null)
            {
                Debug.LogError("[VuplexWebViewController] Cannot load URL: WebView not ready.");
                return;
            }

            _requestedUrlDomain = ExtractDomain(url);
            _isLoading = true;
            _isDirty = true;
            _hasReportedLoadFailure = false;
            _webView.LoadUrl(url);

            CancelLoadTimeout();
            _loadTimeoutCoroutine = StartCoroutine(LoadTimeoutCoroutine());
        }

        public bool SendKeyEvent(KeyCode key)
        {
            if (!_isReady || _webView == null)
            {
                OnKeyEventResult?.Invoke(key, false);
                return false;
            }

            string vuplexKey = key switch
            {
                KeyCode.RightArrow => "ArrowRight",
                KeyCode.LeftArrow => "ArrowLeft",
                _ => null
            };

            if (vuplexKey == null)
            {
                OnKeyEventResult?.Invoke(key, false);
                return false;
            }

            _webView.SendKey(vuplexKey);
            _isDirty = true;
            OnKeyEventResult?.Invoke(key, true);
            return true;
        }

        public void Cleanup()
        {
            CancelLoadTimeout();
            UnsubscribeFromEvents();

            try
            {
                Web.ClearAllData();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VuplexWebViewController] ClearAllData failed: {ex.Message}");
            }

            if (_webView != null)
            {
                _webView.Dispose();
                _webView = null;
            }

            _isReady = false;
            _isLoading = false;
            _isDirty = false;
        }

        private void LateUpdate()
        {
            if (!_isDirty || _webView == null || _webView.Texture == null) return;

            Graphics.Blit(_webView.Texture, _targetRenderTexture);
            _isDirty = false;
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void SubscribeToEvents()
        {
            if (_webView == null) return;

            _webView.LoadProgressChanged += HandleLoadProgressChanged;
            _webView.LoadFailed += HandleLoadFailed;
            _webView.UrlChanged += HandleUrlChanged;
            _webView.Terminated += HandleTerminated;
        }

        private void UnsubscribeFromEvents()
        {
            if (_webView == null) return;

            _webView.LoadProgressChanged -= HandleLoadProgressChanged;
            _webView.LoadFailed -= HandleLoadFailed;
            _webView.UrlChanged -= HandleUrlChanged;
            _webView.Terminated -= HandleTerminated;
        }

        private void HandleLoadProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            _isDirty = true;

            switch (args.Type)
            {
                case ProgressChangeType.Started:
                    _isLoading = true;
                    break;

                case ProgressChangeType.Finished:
                    _isLoading = false;
                    CancelLoadTimeout();
                    _hasReportedLoadFailure = false;
                    OnLoadSuccess?.Invoke(_webView.Url);
                    break;

                case ProgressChangeType.Failed:
                    ReportNetworkFailure();
                    break;
            }
        }

        private void HandleLoadFailed(object sender, LoadFailedEventArgs args)
        {
            ReportNetworkFailure();
        }

        private void HandleUrlChanged(object sender, UrlChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(_requestedUrlDomain)) return;

            string currentDomain = ExtractDomain(args.Url);
            if (string.IsNullOrEmpty(currentDomain)) return;

            foreach (string authDomain in AuthDomains)
            {
                if (currentDomain.Equals(authDomain, StringComparison.OrdinalIgnoreCase)
                    && !_requestedUrlDomain.Equals(authDomain, StringComparison.OrdinalIgnoreCase))
                {
                    _isLoading = false;
                    CancelLoadTimeout();
                    OnLoadError?.Invoke(WebViewError.LoginWallDetected);
                    return;
                }
            }
        }

        private void HandleTerminated(object sender, TerminatedEventArgs args)
        {
            OnCrash?.Invoke();

            float now = Time.unscaledTime;
            if ((now - _lastRecoveryAttemptTime) < CrashRetryWindowSeconds)
            {
                Debug.LogError("[VuplexWebViewController] Second crash within retry window. Not retrying.");
                _isReady = false;
                _isLoading = false;
                OnLoadError?.Invoke(WebViewError.Unknown);
                return;
            }

            _lastRecoveryAttemptTime = now;
            AttemptRecovery();
        }

        private async void AttemptRecovery()
        {
            Debug.LogWarning("[VuplexWebViewController] Attempting crash recovery...");

            UnsubscribeFromEvents();
            if (_webView != null)
            {
                _webView.Dispose();
                _webView = null;
            }

            _isReady = false;
            _isLoading = false;

            try
            {
                _webView = await Web.CreateWebView();
                await _webView.Init(_targetRenderTexture.width, _targetRenderTexture.height);
                SubscribeToEvents();
                _isReady = true;
                Debug.Log("[VuplexWebViewController] Crash recovery successful.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VuplexWebViewController] Crash recovery failed: {ex.Message}");
                _isReady = false;
                OnLoadError?.Invoke(WebViewError.Unknown);
            }
        }

        private IEnumerator LoadTimeoutCoroutine()
        {
            yield return new WaitForSeconds(PageLoadTimeoutSeconds);

            if (_isLoading && _webView != null)
            {
                _webView.StopLoad();
                _isLoading = false;
                OnLoadError?.Invoke(WebViewError.PageLoadTimeout);
            }
        }

        private void CancelLoadTimeout()
        {
            if (_loadTimeoutCoroutine != null)
            {
                StopCoroutine(_loadTimeoutCoroutine);
                _loadTimeoutCoroutine = null;
            }
        }

        private static string ExtractDomain(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;

            try
            {
                if (!url.Contains("://"))
                    url = "https://" + url;

                var uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void ReportNetworkFailure()
        {
            if (_hasReportedLoadFailure)
            {
                return;
            }

            _hasReportedLoadFailure = true;
            _isLoading = false;
            CancelLoadTimeout();
            OnLoadError?.Invoke(WebViewError.NetworkFailure);
        }
    }
}
#endif
