using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace StageMind
{
    /// <summary>
    /// Fetches slide images directly from Google Slides export URLs and renders them
    /// to a RenderTexture. All slides are pre-loaded into memory on LoadUrl(), and
    /// navigation via SendKeyEvent is an instant in-memory texture swap (GPU blit only).
    /// </summary>
    public class BackendSlideWebViewController : MonoBehaviour, IWebViewController
    {
        [Header("Configuration")]
        [SerializeField] private int _requestTimeoutSeconds = 30;
        [SerializeField] private int _maxSlides = 100;

        private RenderTexture _targetRenderTexture;
        private Coroutine _activeRequestCoroutine;
        private string _loadedPresentationUrl;
        private string _presentationId;
        private readonly List<Texture2D> _slideTextures = new();
        private readonly List<string> _slidePageIds = new();
        private int _currentSlideIndex;
        private bool _isLoading;
        private bool _isReady;

        public event Action<string> OnLoadSuccess;
        public event Action<WebViewError> OnLoadError;
#pragma warning disable CS0067
        public event Action OnCrash;
#pragma warning restore CS0067
        public event Action<KeyCode, bool> OnKeyEventResult;

        public bool IsLoading => _isLoading;
        public bool IsReady => _isReady;
        public int SlideCount => _slideTextures.Count;
        public int CurrentSlideIndex => _currentSlideIndex;

        public void Initialize(RenderTexture targetTexture)
        {
            _targetRenderTexture = targetTexture;
            _isReady = _targetRenderTexture != null;
            _isLoading = false;

            if (!_isReady)
            {
                Debug.LogError("[SlideController] Initialize failed: target RenderTexture is null.");
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

            _presentationId = ExtractPresentationId(normalizedUrl);
            if (string.IsNullOrEmpty(_presentationId))
            {
                Debug.LogError($"[SlideController] Could not extract presentation ID from: {normalizedUrl}");
                OnLoadError?.Invoke(WebViewError.NetworkFailure);
                return;
            }

            Debug.Log($"[SlideController] Extracted presentation ID: {_presentationId}");

            CancelActiveRequest();
            _activeRequestCoroutine = StartCoroutine(DiscoverAndLoadAllSlides(normalizedUrl));
        }

        public bool SendKeyEvent(KeyCode key)
        {
            if (!EnsureReady() || _isLoading || _slideTextures.Count == 0)
            {
                OnKeyEventResult?.Invoke(key, false);
                return false;
            }

            int nextIndex = _currentSlideIndex;

            if (key == KeyCode.RightArrow)
            {
                nextIndex = Mathf.Min(_currentSlideIndex + 1, _slideTextures.Count - 1);
            }
            else if (key == KeyCode.LeftArrow)
            {
                nextIndex = Mathf.Max(_currentSlideIndex - 1, 0);
            }
            else
            {
                OnKeyEventResult?.Invoke(key, false);
                return false;
            }

            if (nextIndex == _currentSlideIndex)
            {
                OnKeyEventResult?.Invoke(key, false);
                return false;
            }

            _currentSlideIndex = nextIndex;
            BlitCurrentSlide();
            Debug.Log($"[SlideController] Slide {_currentSlideIndex + 1}/{_slideTextures.Count}");
            OnKeyEventResult?.Invoke(key, true);
            return true;
        }

        public void Cleanup()
        {
            CancelActiveRequest();
            DestroyAllCachedTextures();
            _slidePageIds.Clear();
            _presentationId = null;
            _loadedPresentationUrl = null;
            _currentSlideIndex = 0;
            _isLoading = false;
            _isReady = false;
        }

        private IEnumerator DiscoverAndLoadAllSlides(string url)
        {
            _isLoading = true;
            _loadedPresentationUrl = url;
            _currentSlideIndex = 0;

            DestroyAllCachedTextures();
            _slidePageIds.Clear();

            yield return DiscoverSlidePageIds();

            if (_slidePageIds.Count == 0)
            {
                Debug.Log("[SlideController] Embed parse found no IDs. Trying sequential probe...");
                yield return DiscoverSlidesBySequentialProbe();
            }

            if (_slidePageIds.Count == 0)
            {
                _isLoading = false;
                Debug.LogError("[SlideController] Failed to discover any slides.");
                OnLoadError?.Invoke(WebViewError.NetworkFailure);
                yield break;
            }

            Debug.Log($"[SlideController] Discovered {_slidePageIds.Count} slides. Downloading sequentially...");

            for (int i = 0; i < _slidePageIds.Count; i++)
            {
                string imageUrl = BuildExportUrl(_slidePageIds[i]);

                using var request = UnityWebRequestTexture.GetTexture(imageUrl);
                request.timeout = Mathf.Max(1, _requestTimeoutSeconds);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    string contentType = request.GetResponseHeader("Content-Type");
                    if (IsHtmlResponse(contentType))
                    {
                        Debug.LogError($"[SlideController] HTML received instead of image for slide {i} — login wall detected.");
                        _isLoading = false;
                        OnLoadError?.Invoke(WebViewError.LoginWallDetected);
                        yield break;
                    }

                    Debug.LogWarning($"[SlideController] Failed to download slide {i} ({_slidePageIds[i]}): {request.error} (HTTP {request.responseCode})");
                    continue;
                }

                string responseContentType = request.GetResponseHeader("Content-Type");
                if (IsHtmlResponse(responseContentType))
                {
                    Debug.LogError($"[SlideController] Expected image but got: {responseContentType} — login wall.");
                    _isLoading = false;
                    OnLoadError?.Invoke(WebViewError.LoginWallDetected);
                    yield break;
                }

                Texture2D tex = DownloadHandlerTexture.GetContent(request);
                if (tex != null)
                {
                    tex.name = $"Slide_{i}_{_slidePageIds[i]}";
                    _slideTextures.Add(tex);
                    Debug.Log($"[SlideController] Downloaded slide {_slideTextures.Count}/{_slidePageIds.Count} ({tex.width}x{tex.height})");
                }
            }

            if (_slideTextures.Count == 0)
            {
                _isLoading = false;
                Debug.LogError("[SlideController] No slides downloaded successfully.");
                OnLoadError?.Invoke(WebViewError.NetworkFailure);
                yield break;
            }

            BlitCurrentSlide();
            _isLoading = false;
            Debug.Log($"[SlideController] Loaded {_slideTextures.Count} slides from: {url}");
            OnLoadSuccess?.Invoke(url);
        }

        private IEnumerator DiscoverSlidePageIds()
        {
            bool isPublishedId = _presentationId.StartsWith("2PACX");

            string fetchUrl = isPublishedId
                ? $"https://docs.google.com/presentation/d/e/{_presentationId}/pub"
                : $"https://docs.google.com/presentation/d/{_presentationId}/embed?start=false&loop=false";

            Debug.Log($"[SlideController] Fetching {(isPublishedId ? "published" : "embed")} page for slide discovery...");

            using var request = UnityWebRequest.Get(fetchUrl);
            request.timeout = Mathf.Max(1, _requestTimeoutSeconds);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[SlideController] Failed to fetch page: {request.error}");
                yield break;
            }

            string html = request.downloadHandler.text;
            if (string.IsNullOrEmpty(html))
            {
                Debug.LogWarning("[SlideController] Empty page response.");
                yield break;
            }

            // For published URLs, resolve the original presentation ID from docId
            if (isPublishedId)
            {
                var docIdMatch = Regex.Match(html, @"docId:\s*'([a-zA-Z0-9_-]+)'");
                if (docIdMatch.Success)
                {
                    string originalId = docIdMatch.Groups[1].Value;
                    Debug.Log($"[SlideController] Resolved original presentation ID: {originalId}");
                    _presentationId = originalId;
                }
                else
                {
                    Debug.LogWarning("[SlideController] Could not resolve original presentation ID from published page.");
                }
            }

            ParseSlideIdsFromHtml(html);
            Debug.Log($"[SlideController] Page parse found {_slidePageIds.Count} slide IDs.");
        }

        private void ParseSlideIdsFromHtml(string html)
        {
            var seenIds = new HashSet<string>();
            var slidesWithIndex = new List<(string id, int index)>();

            // Primary: parse slide entries from the docData JavaScript structure.
            // Google embeds slide metadata as: ["pageId",slideIndex,"title",...
            var docDataMatches = Regex.Matches(html, @"\[""(g[a-f0-9]+_\d+_\d+|p\d*)"",(\d+),""");
            foreach (Match match in docDataMatches)
            {
                string pageId = match.Groups[1].Value;
                if (int.TryParse(match.Groups[2].Value, out int slideIndex) && seenIds.Add(pageId))
                {
                    slidesWithIndex.Add((pageId, slideIndex));
                }
            }

            if (slidesWithIndex.Count > 0)
            {
                slidesWithIndex.Sort((a, b) => a.index.CompareTo(b.index));
                foreach (var (id, _) in slidesWithIndex)
                {
                    _slidePageIds.Add(id);
                }
                return;
            }

            // Fallback: look for slide=id.XXX patterns in URL fragments
            var slideIdMatches = Regex.Matches(html, @"slide=id\.([a-zA-Z0-9_]+)");
            foreach (Match match in slideIdMatches)
            {
                string pageId = match.Groups[1].Value;
                if (seenIds.Add(pageId))
                {
                    _slidePageIds.Add(pageId);
                }
            }
        }

        private IEnumerator DiscoverSlidesBySequentialProbe()
        {
            // Sequential probe: try common page ID patterns as a last resort.
            // Google uses "p" for the title slide and "p1","p2",... for subsequent
            // slides in some presentations. Hash-based IDs (g...) are discovered
            // via the docData parse above and won't appear here.
            string[] candidates = { "p" };
            foreach (string candidate in candidates)
            {
                bool exists = false;
                yield return ProbeSlideExists(candidate, result => exists = result);
                if (exists)
                {
                    _slidePageIds.Add(candidate);
                }
            }

            int consecutiveFailures = 0;
            for (int i = 1; i <= _maxSlides && consecutiveFailures < 3; i++)
            {
                string pageId = $"p{i}";
                bool exists = false;
                yield return ProbeSlideExists(pageId, result => exists = result);

                if (exists)
                {
                    _slidePageIds.Add(pageId);
                    consecutiveFailures = 0;
                }
                else
                {
                    consecutiveFailures++;
                }
            }

            Debug.Log($"[SlideController] Sequential probe found {_slidePageIds.Count} slides.");
        }

        private IEnumerator ProbeSlideExists(string pageId, Action<bool> callback)
        {
            string imageUrl = BuildExportUrl(pageId);

            using var request = UnityWebRequest.Head(imageUrl);
            request.timeout = Mathf.Max(1, _requestTimeoutSeconds);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                callback(false);
                yield break;
            }

            string contentType = request.GetResponseHeader("Content-Type");
            bool isImage = contentType != null && contentType.Contains("image/");
            callback(isImage);
        }

        private void BlitCurrentSlide()
        {
            if (_currentSlideIndex < 0 || _currentSlideIndex >= _slideTextures.Count)
            {
                return;
            }

            if (_targetRenderTexture == null)
            {
                return;
            }

            Texture2D slide = _slideTextures[_currentSlideIndex];
            if (slide != null)
            {
                Graphics.Blit(slide, _targetRenderTexture);
            }
        }

        private string BuildExportUrl(string pageId)
        {
            return $"https://docs.google.com/presentation/d/{_presentationId}/export/png?id={_presentationId}&pageid={pageId}";
        }

        private static string ExtractPresentationId(string url)
        {
            // Published URL: /d/e/{publishedId}/pub — must check BEFORE the generic /d/ pattern
            var pubMatch = Regex.Match(url, @"/presentation/d/e/([a-zA-Z0-9_-]+)");
            if (pubMatch.Success)
            {
                return pubMatch.Groups[1].Value;
            }

            // Regular edit/view URL: /d/{presentationId}/
            var match = Regex.Match(url, @"/presentation/d/([a-zA-Z0-9_-]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;
        }

        private static bool IsHtmlResponse(string contentType)
        {
            return contentType != null && contentType.Contains("text/html");
        }

        private bool EnsureReady()
        {
            if (_isReady && _targetRenderTexture != null)
            {
                return true;
            }

            Debug.LogError("[SlideController] Controller is not initialized.");
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

        private void DestroyAllCachedTextures()
        {
            foreach (var tex in _slideTextures)
            {
                if (tex != null)
                {
                    Destroy(tex);
                }
            }
            _slideTextures.Clear();
        }
    }
}
