using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StageMind
{
    public class MockBackendSlideWebViewController : MonoBehaviour, IWebViewController
    {
        [Header("Mock Slide Source")]
        [SerializeField] private List<Texture2D> _mockSlides = new();
        [SerializeField] private float _simulatedLoadSeconds = 0.15f;

        private RenderTexture _targetRenderTexture;
        private Coroutine _activeLoadCoroutine;
        private string _loadedPresentationUrl;
        private int _currentSlideIndex;
        private KeyCode _pendingKeyEvent = KeyCode.None;
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

        public void Initialize(RenderTexture targetTexture)
        {
            _targetRenderTexture = targetTexture;
            _isReady = _targetRenderTexture != null;
            _isLoading = false;

            if (!_isReady)
            {
                Debug.LogError("[MockBackendSlideWebViewController] Initialize failed: target RenderTexture is null.");
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

            if (_mockSlides == null || _mockSlides.Count == 0)
            {
                Debug.LogError("[MockBackendSlideWebViewController] No mock slides assigned.");
                OnLoadError?.Invoke(WebViewError.NetworkFailure);
                return;
            }

            _loadedPresentationUrl = normalizedUrl;
            _currentSlideIndex = 0;
            StartLoadForCurrentSlide();
        }

        public bool SendKeyEvent(KeyCode key)
        {
            if (!EnsureReady() || _isLoading || _mockSlides == null || _mockSlides.Count == 0)
            {
                OnKeyEventResult?.Invoke(key, false);
                return false;
            }

            int nextIndex = _currentSlideIndex;

            if (key == KeyCode.RightArrow)
            {
                nextIndex = Mathf.Min(_currentSlideIndex + 1, _mockSlides.Count - 1);
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
            _pendingKeyEvent = key;
            StartLoadForCurrentSlide();
            return true;
        }

        public void Cleanup()
        {
            if (_activeLoadCoroutine != null)
            {
                StopCoroutine(_activeLoadCoroutine);
                _activeLoadCoroutine = null;
            }

            _isLoading = false;
            _isReady = false;
            _loadedPresentationUrl = null;
            _currentSlideIndex = 0;
            _pendingKeyEvent = KeyCode.None;
        }

        private void StartLoadForCurrentSlide()
        {
            if (_activeLoadCoroutine != null)
            {
                StopCoroutine(_activeLoadCoroutine);
            }

            _activeLoadCoroutine = StartCoroutine(LoadCurrentSlideCoroutine());
        }

        private IEnumerator LoadCurrentSlideCoroutine()
        {
            _isLoading = true;
            if (_simulatedLoadSeconds > 0f)
            {
                yield return new WaitForSeconds(_simulatedLoadSeconds);
            }

            if (_currentSlideIndex < 0 || _currentSlideIndex >= _mockSlides.Count)
            {
                _isLoading = false;
                OnLoadError?.Invoke(WebViewError.Unknown);
                EmitPendingKeyResult(false);
                yield break;
            }

            Texture2D slideTexture = _mockSlides[_currentSlideIndex];
            if (slideTexture == null || _targetRenderTexture == null)
            {
                _isLoading = false;
                OnLoadError?.Invoke(WebViewError.Unknown);
                EmitPendingKeyResult(false);
                yield break;
            }

            Graphics.Blit(slideTexture, _targetRenderTexture);
            _isLoading = false;
            OnLoadSuccess?.Invoke(_loadedPresentationUrl);
            EmitPendingKeyResult(true);
        }

        private bool EnsureReady()
        {
            if (_isReady && _targetRenderTexture != null)
            {
                return true;
            }

            Debug.LogError("[MockBackendSlideWebViewController] Controller is not initialized.");
            OnLoadError?.Invoke(WebViewError.Unknown);
            return false;
        }

        private void EmitPendingKeyResult(bool changed)
        {
            if (_pendingKeyEvent == KeyCode.None)
            {
                return;
            }

            OnKeyEventResult?.Invoke(_pendingKeyEvent, changed);
            _pendingKeyEvent = KeyCode.None;
        }

    }
}
