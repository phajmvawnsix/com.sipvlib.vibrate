using System;
using System.Collections.Generic;
using Alchemy.Inspector;
using SiPVLib.Config;
using SiPVLib.Config.Configs;
using SiPVLib.Vibrate.Haptics;
using UnityEngine;

namespace SiPVLib.Vibrate.Configs
{
    /// <summary>
    /// Which payload field on a <see cref="VibrateEntry"/> is active.
    /// </summary>
    public enum VibrateTriggerMode
    {
        Preset,
        Pattern,
        Wave,
        Sound
    }

    /// <summary>
    /// One preconfigured, provider-agnostic haptic entry. Only the field matching
    /// <see cref="TriggerMode"/> is used. The active <see cref="IHapticProvider"/> maps it onto its backend.
    /// </summary>
    [Serializable]
    public struct VibrateEntry
    {
        public string Id;
        public VibrateTriggerMode TriggerMode;

        [ShowIf(nameof(IsPresetMode))]
        public HapticType Preset;

        [ShowIf(nameof(IsPatternMode))]
        public HapticPattern Pattern;

        [ShowIf(nameof(IsWaveMode))]
        public HapticWave Wave;

        [ShowIf(nameof(IsSoundMode))]
        public HapticSound Sound;

        private bool IsPresetMode => TriggerMode == VibrateTriggerMode.Preset;
        private bool IsPatternMode => TriggerMode == VibrateTriggerMode.Pattern;
        private bool IsWaveMode => TriggerMode == VibrateTriggerMode.Wave;
        private bool IsSoundMode => TriggerMode == VibrateTriggerMode.Sound;
    }

    /// <summary>
    /// Stores a list of preconfigured haptic entries for quick play via <see cref="VibrateManager"/>.
    /// </summary>
    [ConfigCategory("Vibrate")]
    public class VibrateConfig : GameConfig
    {
        [SerializeField] private List<VibrateEntry> _entries = new();

        public IReadOnlyList<VibrateEntry> Entries => _entries;

        /// <summary>Finds an entry by Id within this config asset only. Use <see cref="VibrateManager"/> for a flat lookup across all VibrateConfig assets.</summary>
        public bool GetEntry(string id, out VibrateEntry entry)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Id == id)
                {
                    entry = _entries[i];
                    return true;
                }
            }

            entry = default;
            return false;
        }
    }
}
