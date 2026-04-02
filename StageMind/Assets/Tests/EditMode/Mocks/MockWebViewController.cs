using System;
using System.Collections.Generic;
using UnityEngine;

namespace StageMind.Tests.EditMode.Mocks
{
    public class MockWebViewController : IWebViewController
    {
        public event Action<string> OnLoadSuccess;
        public event Action<WebViewError> OnLoadError;
        public event Action OnCrash;

        public bool IsLoading { get; set; }
        public bool IsReady { get; set; }

        public List<string> LoadedUrls { get; } = new();
        public List<KeyCode> SentKeyEvents { get; } = new();
        public int InitializeCallCount { get; private set; }
        public int CleanupCallCount { get; private set; }

        public void Initialize(RenderTexture targetTexture) => InitializeCallCount++;
        public void LoadUrl(string url) { LoadedUrls.Add(url); IsLoading = true; }
        public void SendKeyEvent(KeyCode key) => SentKeyEvents.Add(key);
        public void Cleanup() => CleanupCallCount++;

        public void SimulateLoadSuccess(string url) { IsLoading = false; OnLoadSuccess?.Invoke(url); }
        public void SimulateLoadError(WebViewError error) { IsLoading = false; OnLoadError?.Invoke(error); }
        public void SimulateCrash() => OnCrash?.Invoke();
    }
}
