using System;
using System.Collections.Generic;
using SiPVLib.Config;
using SiPVLib.Config.Configs;
using SiPVLib.Vibrate.Haptics;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
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

#if ODIN_INSPECTOR
        [ShowIf(nameof(TriggerMode), VibrateTriggerMode.Preset)]
#endif
        public HapticType Preset;

#if ODIN_INSPECTOR
        [ShowIf(nameof(TriggerMode), VibrateTriggerMode.Pattern)]
#endif
        public HapticPattern Pattern;

#if ODIN_INSPECTOR
        [ShowIf(nameof(TriggerMode), VibrateTriggerMode.Wave)]
#endif
        public HapticWave Wave;

#if ODIN_INSPECTOR
        [ShowIf(nameof(TriggerMode), VibrateTriggerMode.Sound)]
#endif
        public HapticSound Sound;
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
