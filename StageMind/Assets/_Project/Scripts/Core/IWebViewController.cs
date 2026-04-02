using System;
using UnityEngine;

namespace StageMind
{
    public interface IWebViewController
    {
        void Initialize(RenderTexture targetTexture);
        void LoadUrl(string url);
        void SendKeyEvent(KeyCode key);
        void Cleanup();

        event Action<string> OnLoadSuccess;
        event Action<WebViewError> OnLoadError;
        event Action OnCrash;

        bool IsLoading { get; }
        bool IsReady { get; }
    }
}
