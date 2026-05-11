using UnityEngine;

namespace StageMind
{
    /// <summary>
    /// Spike-only controller for QR code scanning feasibility (Story 1.5, Task 6).
    /// Evaluates Quest 3 passthrough camera access and QR decode capability.
    /// Delete after Epic 1 retrospective.
    ///
    /// Prerequisites (not installed by this spike):
    /// - Meta MRUK v81+ with PassthroughCameraAccess API
    /// - horizonos.permission.HEADSET_CAMERA in AndroidManifest.xml
    /// - ZXing.Net or Unity-compatible QR decode library
    ///
    /// If passthrough API integration proves too complex, document blockers
    /// and defer QR scanning (FR3) to post-MVP.
    /// </summary>
    public class QRSpikeController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int _decodeEveryNthFrame = 3;

        private int _frameCounter;
        private int _decodeAttempts;
        private int _decodeSuccesses;
        private float _totalDecodeTimeSeconds;
        private bool _passthroughAvailable;

        private void Start()
        {
            Debug.Log("[QRSpike] QR scanning feasibility evaluation starting...");
            InitializePassthrough();
        }

        private void Update()
        {
            if (!_passthroughAvailable)
                return;

            _frameCounter++;
            if (_frameCounter % _decodeEveryNthFrame != 0)
                return;

            AttemptQRDecode();
        }

        private void InitializePassthrough()
        {
            // Meta Passthrough Camera API integration point.
            // Requires: OVRManager with "Passthrough Camera Access" enabled
            // and PassthroughCameraAccess from MRUK v81+.
            //
            // Implementation:
            //   var cameraAccess = GetComponent<PassthroughCameraAccess>();
            //   if (cameraAccess != null && cameraAccess.IsAvailable)
            //   {
            //       _passthroughAvailable = true;
            //       Debug.Log("[QRSpike] Passthrough camera access available");
            //   }
            //
            // For this spike, log the status and document blockers if the API
            // is not available in the current MRUK version.

            Debug.LogWarning("[QRSpike] PassthroughCameraAccess API not yet integrated. " +
                             "Add Meta MRUK v81+ and enable headset camera permission to test.");
            _passthroughAvailable = false;
        }

        private void AttemptQRDecode()
        {
            // QR decode integration point using ZXing.Net:
            //
            //   Texture2D frame = PassthroughCameraAccess.GetTexture();
            //   if (frame == null) return;
            //
            //   float startTime = Time.realtimeSinceStartup;
            //   var reader = new ZXing.BarcodeReader();
            //   var result = reader.Decode(frame.GetPixels32(), frame.width, frame.height);
            //   float decodeTime = Time.realtimeSinceStartup - startTime;
            //
            //   _decodeAttempts++;
            //   if (result != null)
            //   {
            //       _decodeSuccesses++;
            //       _totalDecodeTimeSeconds += decodeTime;
            //       Debug.Log($"[QRSpike] Decoded: {result.Text} in {decodeTime*1000:F1}ms");
            //   }

            _decodeAttempts++;
        }

        public void LogResults()
        {
            float successRate = _decodeAttempts > 0
                ? (float)_decodeSuccesses / _decodeAttempts * 100f
                : 0f;
            float avgDecodeMs = _decodeSuccesses > 0
                ? _totalDecodeTimeSeconds / _decodeSuccesses * 1000f
                : 0f;

            Debug.Log($"[QRSpike] === QR Scanning Feasibility Results ===\n" +
                      $"  Passthrough available: {_passthroughAvailable}\n" +
                      $"  Decode attempts: {_decodeAttempts}\n" +
                      $"  Decode successes: {_decodeSuccesses}\n" +
                      $"  Success rate: {successRate:F1}%\n" +
                      $"  Avg decode time: {avgDecodeMs:F1}ms\n" +
                      $"  Verdict: {(successRate > 80f ? "RELIABLE — FR3 confirmed for MVP" : "UNRELIABLE — FR3 deferred to post-MVP")}");
        }

        private void OnDestroy()
        {
            LogResults();
        }
    }
}
