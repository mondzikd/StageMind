using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace StageMind
{
    public class BackendSlideWebViewController : MonoBehaviour, IWebViewController
    {
        [Header("Backend API")]
        [SerializeField] private string _backendBaseUrl = "http://localhost:8080";
        [SerializeField] private int _requestTimeoutSeconds = 15;

        private RenderTexture _targetRenderTexture;
        private Coroutine _activeRequestCoroutine;
        private string _sessionId;
        private string _loadedPresentationUrl;
        private bool _isLoading;
        private bool _isReady;

        public event Action<string> OnLoadSuccess;
        public event Action<WebViewError> OnLoadError;
        public event Action OnCrash;

        public bool IsLoading => _isLoading;
        public bool IsReady => _isReady;

        public void Initialize(RenderTexture targetTexture)
        {
            _targetRenderTexture = targetTexture;
            _isReady = _targetRenderTexture != null;
            _isLoading = false;

            if (!_isReady)
            {
                Debug.LogError("[BackendSlideWebViewController] Initialize failed: target RenderTexture is null.");
                OnLoadError?.Invoke(WebViewError.Unknown);
            }
        }

        public void LoadUrl(string url)
        {
            if (!EnsureReady())
            {
                return;
            }

            var (isValid, normalizedUrl, _) = UrlValidator.ValidateAndNormalize(url);
            if (!isValid)
            {
                OnLoadError?.Invoke(WebViewError.NetworkFailure);
                return;
            }

            CancelActiveRequest();
            _activeRequestCoroutine = StartCoroutine(StartSessionAndLoadFirstSlide(normalizedUrl));
        }

        public void SendKeyEvent(KeyCode key)
        {
            if (!EnsureReady() || string.IsNullOrEmpty(_sessionId) || _isLoading)
            {
                return;
            }

            string action = key switch
            {
                KeyCode.RightArrow => "next",
                KeyCode.LeftArrow => "previous",
                _ => null
            };

            if (string.IsNullOrEmpty(action))
            {
                return;
            }

            CancelActiveRequest();
            _activeRequestCoroutine = StartCoroutine(NavigateAndLoadSlide(action));
        }

        public void Cleanup()
        {
            CancelActiveRequest();
            _sessionId = null;
            _loadedPresentationUrl = null;
            _isLoading = false;
            _isReady = false;
        }

        private IEnumerator StartSessionAndLoadFirstSlide(string normalizedUrl)
        {
            _isLoading = true;
            _loadedPresentationUrl = normalizedUrl;

            string endpoint = $"{TrimTrailingSlash(_backendBaseUrl)}/api/slides/sessions";
            var requestBody = new StartSessionRequest { presentationUrl = normalizedUrl };
            byte[] bodyBytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(requestBody));

            using var request = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(bodyBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = Mathf.Max(1, _requestTimeoutSeconds);

            yield return request.SendWebRequest();

            if (!RequestSucceeded(request))
            {
                _isLoading = false;
                OnLoadError?.Invoke(MapToWebViewError(request));
                yield break;
            }

            SlideSessionResponse response = ParseResponse(request.downloadHandler.text);
            if (response == null || string.IsNullOrEmpty(response.sessionId) || string.IsNullOrEmpty(response.imageUrl))
            {
                _isLoading = false;
                OnLoadError?.Invoke(WebViewError.Unknown);
                yield break;
            }

            _sessionId = response.sessionId;
            yield return DownloadAndBlit(response.imageUrl);

            if (!_isLoading)
            {
                yield break;
            }

            _isLoading = false;
            OnLoadSuccess?.Invoke(_loadedPresentationUrl);
        }

        private IEnumerator NavigateAndLoadSlide(string action)
        {
            _isLoading = true;

            string endpoint = $"{TrimTrailingSlash(_backendBaseUrl)}/api/slides/sessions/{UnityWebRequest.EscapeURL(_sessionId)}/{action}";
            using var request = UnityWebRequest.Get(endpoint);
            request.timeout = Mathf.Max(1, _requestTimeoutSeconds);

            yield return request.SendWebRequest();

            if (!RequestSucceeded(request))
            {
                _isLoading = false;
                OnLoadError?.Invoke(MapToWebViewError(request));
                yield break;
            }

            SlideSessionResponse response = ParseResponse(request.downloadHandler.text);
            if (response == null || string.IsNullOrEmpty(response.imageUrl))
            {
                _isLoading = false;
                OnLoadError?.Invoke(WebViewError.Unknown);
                yield break;
            }

            yield return DownloadAndBlit(response.imageUrl);

            if (!_isLoading)
            {
                yield break;
            }

            _isLoading = false;
            OnLoadSuccess?.Invoke(_loadedPresentationUrl);
        }

        private IEnumerator DownloadAndBlit(string imageUrl)
        {
            using var imageRequest = UnityWebRequestTexture.GetTexture(imageUrl);
            imageRequest.timeout = Mathf.Max(1, _requestTimeoutSeconds);

            yield return imageRequest.SendWebRequest();

            if (!RequestSucceeded(imageRequest))
            {
                _isLoading = false;
                OnLoadError?.Invoke(MapToWebViewError(imageRequest));
                yield break;
            }

            Texture texture = DownloadHandlerTexture.GetContent(imageRequest);
            if (texture == null || _targetRenderTexture == null)
            {
                _isLoading = false;
                OnLoadError?.Invoke(WebViewError.Unknown);
                yield break;
            }

            Graphics.Blit(texture, _targetRenderTexture);
        }

        private bool EnsureReady()
        {
            if (_isReady && _targetRenderTexture != null)
            {
                return true;
            }

            Debug.LogError("[BackendSlideWebViewController] Controller is not initialized.");
            OnLoadError?.Invoke(WebViewError.Unknown);
            return false;
        }

        private void CancelActiveRequest()
        {
            if (_activeRequestCoroutine != null)
            {
                StopCoroutine(_activeRequestCoroutine);
                _activeRequestCoroutine = null;
            }
        }

        private static bool RequestSucceeded(UnityWebRequest request)
        {
            return request.result == UnityWebRequest.Result.Success;
        }

        private static string TrimTrailingSlash(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.TrimEnd('/');
        }

        private static SlideSessionResponse ParseResponse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonUtility.FromJson<SlideSessionResponse>(json);
            }
            catch
            {
                return null;
            }
        }

        private static WebViewError MapToWebViewError(UnityWebRequest request)
        {
            if (request.responseCode is 403 or 422)
            {
                return WebViewError.LoginWallDetected;
            }

            if (request.responseCode is 408 or 504)
            {
                return WebViewError.PageLoadTimeout;
            }

            if (!string.IsNullOrEmpty(request.error) &&
                request.error.IndexOf("timed out", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WebViewError.PageLoadTimeout;
            }

            return WebViewError.NetworkFailure;
        }

        [Serializable]
        private class StartSessionRequest
        {
            public string presentationUrl;
        }

        [Serializable]
        private class SlideSessionResponse
        {
            public string sessionId;
            public string imageUrl;
        }
    }
}
