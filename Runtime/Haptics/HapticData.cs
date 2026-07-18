using System;
using UnityEngine;

namespace SiPVLib.Vibrate.Haptics
{
    /// <summary>
    /// One pulse in a <see cref="HapticPattern"/>. Provider-agnostic: a delay, a duration, and a
    /// normalized amplitude. Providers translate these onto their platform (Android amplitude
    /// waveform, iOS transient events, etc.).
    /// </summary>
    [Serializable]
    public struct HapticPulse
    {
        [Tooltip("Silent gap before this pulse, in milliseconds.")]
        [Min(0f)]
        public float DelayMs;

        [Tooltip("Pulse duration, in milliseconds.")]
        [Min(1f)]
        public float DurationMs;

        [Tooltip("Pulse strength. 0 = silent, 1 = maximum.")]
        [Range(0f, 1f)]
        public float Amplitude;
    }

    /// <summary>
    /// Custom vibration: an ordered sequence of <see cref="HapticPulse"/>s.
    /// </summary>
    [Serializable]
    public struct HapticPattern
    {
        [Tooltip("Pulses played back-to-back, in order.")]
        public HapticPulse[] Pulses;
    }

    /// <summary>
    /// Wave-like vibration: a continuous intensity envelope over time.
    /// </summary>
    [Serializable]
    public struct HapticWave
    {
        [Tooltip("Intensity over normalized time. X = 0..1 across DurationMs. Y = intensity 0..1.")]
        public AnimationCurve Intensity;

        [Tooltip("Total wave duration, in milliseconds.")]
        [Min(1f)]
        public float DurationMs;

        [Tooltip("Perceived sharpness. 0 = soft/dull, 1 = crisp/sharp.")]
        [Range(0f, 1f)]
        public float Sharpness;
    }

    /// <summary>
    /// Sound-driven vibration ("haptic sound"): the amplitude envelope of an <see cref="AudioClip"/>
    /// drives the haptic intensity. Providers that lack native audio-haptics sample the clip into a
    /// <see cref="HapticWave"/>.
    /// </summary>
    [Serializable]
    public struct HapticSound
    {
        [Tooltip("Source clip whose loudness envelope drives the vibration.")]
        public AudioClip Clip;

        [Tooltip("Perceived sharpness. 0 = soft/dull, 1 = crisp/sharp.")]
        [Range(0f, 1f)]
        public float Sharpness;

        [Tooltip("Number of envelope samples taken across the clip.")]
        [Range(2, 64)]
        public int Samples;
    }
}
