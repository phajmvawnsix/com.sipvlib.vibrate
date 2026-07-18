using SiPVLib.Vibrate.Configs;

namespace SiPVLib.Vibrate.Haptics
{
    /// <summary>
    /// Pluggable haptic backend used by <see cref="VibrateManager"/>. Assign an implementation on the
    /// VibrateManager inspector (a <c>[SerializeReference]</c> field). Ship a custom backend from your
    /// own project — not this package — by writing a <c>[System.Serializable]</c> class that implements
    /// this interface; it appears in the inspector's provider dropdown.
    /// </summary>
    public interface IHapticProvider
    {
        /// <summary>Whether haptics can play on the current device/platform.</summary>
        bool IsSupported { get; }

        /// <summary>Warm up native haptic engines to remove first-play latency. Optional/no-op.</summary>
        void Prewarm();

        /// <summary>Play a preconfigured entry, dispatching on its <see cref="VibrateEntry.TriggerMode"/>.</summary>
        void Play(in VibrateEntry entry);

        /// <summary>Play a preset directly. <paramref name="cooldownSeconds"/> 0 = no cooldown gating.</summary>
        void PlayPreset(HapticType type, float cooldownSeconds = 0f);

        /// <summary>Stop any in-progress playback.</summary>
        void Stop();
    }
}
