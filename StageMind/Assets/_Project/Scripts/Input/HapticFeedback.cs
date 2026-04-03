using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Input;

namespace StageMind
{
    public class HapticFeedback : MonoBehaviour
    {
        public enum Hand { Left, Right }

        [Header("Haptic Actions")]
        [SerializeField] private InputActionReference _hapticRightAction;
        [SerializeField] private InputActionReference _hapticLeftAction;

        [Header("Haptic Settings")]
        [SerializeField] private float _clickAmplitude = 0.15f;
        [SerializeField] private float _clickDuration = 0.1f;

        public void ClickRight()
        {
            SendHapticImpulse(Hand.Right, _clickAmplitude, _clickDuration);
        }

        public void ClickLeft()
        {
            SendHapticImpulse(Hand.Left, _clickAmplitude, _clickDuration);
        }

        public void SendHapticImpulse(Hand hand, float amplitude, float duration)
        {
            var action = hand == Hand.Right ? _hapticRightAction?.action : _hapticLeftAction?.action;
            if (action == null) return;

            OpenXRInput.SendHapticImpulse(action, amplitude, duration);
        }
    }
}
