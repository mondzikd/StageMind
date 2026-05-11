using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StageMind
{
    /// <summary>
    /// Spike-only controller for Story 1.5 hardware validation.
    /// Uses FindAnyObjectByType intentionally — this is throwaway spike code.
    /// Delete after Epic 1 retrospective.
    /// </summary>
    public class SpikeTestController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private GameStateManager _gameStateManager;

        [Header("Slide Display")]
        [SerializeField] private RenderTexture _slideRenderTexture;

        [Header("Test Configuration")]
        [SerializeField] private string _testUrl = "";

        private IWebViewController _webViewController;
        private Stopwatch _loadStopwatch;
        private int _slideCount;

        private void Start()
        {
            _webViewController = FindAnyObjectByType<BackendSlideWebViewController>();
            if (_webViewController == null)
                _webViewController = FindAnyObjectByType<MockBackendSlideWebViewController>();

            if (_webViewController == null)
            {
                UnityEngine.Debug.LogError("[SpikeTest] No IWebViewController found in scene");
                return;
            }

            _webViewController.OnLoadSuccess += OnSlidesLoaded;
            _webViewController.OnLoadError += OnLoadError;
            _webViewController.OnKeyEventResult += OnKeyEventResult;

            _webViewController.Initialize(_slideRenderTexture);

            if (!string.IsNullOrEmpty(_testUrl))
            {
                _loadStopwatch = Stopwatch.StartNew();
                _webViewController.LoadUrl(_testUrl);
                UnityEngine.Debug.Log($"[SpikeTest] Loading slides from: {_testUrl}");
            }
        }

        private void OnSlidesLoaded(string url)
        {
            _loadStopwatch?.Stop();
            long loadTimeMs = _loadStopwatch?.ElapsedMilliseconds ?? -1;
            UnityEngine.Debug.Log($"[SpikeTest] Slides loaded from: {url} in {loadTimeMs}ms");

            if (_gameStateManager != null)
            {
                _gameStateManager.TransitionTo(GameStateType.LobbySlidesLoaded);
                _gameStateManager.TransitionTo(GameStateType.Rehearsal);
                UnityEngine.Debug.Log("[SpikeTest] State machine transitioned to Rehearsal — slide navigation active via InputRouter");
            }
        }

        private void OnLoadError(WebViewError error)
        {
            _loadStopwatch?.Stop();
            UnityEngine.Debug.LogError($"[SpikeTest] Load failed: {error}");
        }

        private void Update()
        {
            if (_webViewController == null || !_webViewController.IsReady)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.rightArrowKey.wasPressedThisFrame)
            {
                _webViewController.SendKeyEvent(KeyCode.RightArrow);
            }
            else if (keyboard.leftArrowKey.wasPressedThisFrame)
            {
                _webViewController.SendKeyEvent(KeyCode.LeftArrow);
            }
        }

        private void OnKeyEventResult(KeyCode key, bool changed)
        {
            if (changed)
            {
                _slideCount++;
                UnityEngine.Debug.Log($"[SpikeTest] Slide navigated ({key}) — total navigations: {_slideCount}");
            }
        }

        private void OnDestroy()
        {
            if (_webViewController != null)
            {
                _webViewController.OnLoadSuccess -= OnSlidesLoaded;
                _webViewController.OnLoadError -= OnLoadError;
                _webViewController.OnKeyEventResult -= OnKeyEventResult;
                _webViewController.Cleanup();
            }
        }
    }
}
