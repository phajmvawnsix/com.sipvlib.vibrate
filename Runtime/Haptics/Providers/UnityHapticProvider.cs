using System;
using SiPVLib.Vibrate.Configs;
using UnityEngine;

namespace SiPVLib.Vibrate.Haptics.Providers
{
    /// <summary>
    /// Default <see cref="IHapticProvider"/> shipped with this package. Uses Unity's built-in
    /// <see cref="Handheld.Vibrate"/> — a single fixed-duration buzz with no amplitude, pattern, or
    /// preset control (Android ~500 ms via the system <c>Vibrator</c>; iOS a fixed system vibration;
    /// no-op on other platforms and in the Editor).
    /// <para>
    /// Because the built-in API exposes no intensity or timing, every trigger mode collapses to one
    /// <see cref="Handheld.Vibrate"/> call: <see cref="HapticType"/> strength, <see cref="HapticPattern"/>
    /// timing/amplitude, and <see cref="HapticWave"/>/<see cref="HapticSound"/> envelopes are ignored.
    /// For real presets/patterns/waves, implement a richer <see cref="IHapticProvider"/> in your project.
    /// </para>
    /// </summary>
    [Serializable]
    public sealed class UnityHapticProvider : IHapticProvider
    {
        public bool IsSupported => Application.isMobilePlatform;

        public void Prewarm()
        {
            // Nothing to warm up — Handheld.Vibrate has no engine state.
        }

        public void Play(in VibrateEntry entry)
        {
            // Preset with HapticType.None is intentionally silent; every other entry is a single buzz.
            if (entry.TriggerMode == VibrateTriggerMode.Preset && entry.Preset == HapticType.None)
                return;

            Handheld.Vibrate();
        }

        public void PlayPreset(HapticType type, float cooldownSeconds = 0f)
        {
            if (type == HapticType.None) return;
            Handheld.Vibrate();
        }

        public void Stop()
        {
            // Built-in vibration cannot be cancelled once triggered.
        }
    }
}
