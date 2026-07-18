using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SiPVLib.Vibrate.Runtime.Components
{
    /// <summary>Pointer phase(s) that trigger haptic playback on a <see cref="VibrateOnInteraction"/>.</summary>
    [Flags]
    public enum VibratePhase
    {
        PointerDown = 1,
        PointerUp = 2,
        Click = 4,
        DragBegin = 8,
        Drag = 16
    }

    /// <summary>
    /// Attach to a <see cref="Button"/>, or any GameObject with a raycast target (UI Graphic or 3D/2D
    /// Collider under a PhysicsRaycaster), to play a preconfigured <see cref="VibrateManager"/> entry on
    /// pointer down/up/click/drag. If a <see cref="Button"/> is present its onClick drives the Click
    /// phase; otherwise <see cref="IPointerClickHandler"/> reports the click directly.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VibrateOnInteraction : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private VibratePhase _phases = VibratePhase.Click;
        [SerializeField] private string _vibrateId;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (_button != null) _button.onClick.AddListener(OnClickInternal);
        }

        private void OnDisable()
        {
            if (_button != null) _button.onClick.RemoveListener(OnClickInternal);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Has(VibratePhase.PointerDown)) TryPlay();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Has(VibratePhase.PointerUp)) TryPlay();
        }

        // Avoid double-firing when a Button already handled the click via OnClickInternal.
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_button == null && Has(VibratePhase.Click)) TryPlay();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Has(VibratePhase.DragBegin)) TryPlay();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Has(VibratePhase.Drag)) TryPlay();
        }

        private void OnClickInternal()
        {
            if (Has(VibratePhase.Click)) TryPlay();
        }

        private bool Has(VibratePhase phase) => (_phases & phase) != 0;

        private void TryPlay() => VibrateManager.Instance?.Play(_vibrateId);
    }
}
