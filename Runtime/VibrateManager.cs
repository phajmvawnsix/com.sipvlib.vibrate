using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SiPVLib.Config;
using SiPVLib.Debugging;
using SiPVLib.Event;
using SiPVLib.UserData;
using SiPVLib.Utilities;
using SiPVLib.Vibrate.Configs;
using SiPVLib.Vibrate.Haptics;
using SiPVLib.Vibrate.Haptics.Providers;
using UnityEngine;

namespace SiPVLib.Vibrate
{
    /// <summary>
    /// Central facade for haptic feedback playback. Delegates actual output to a pluggable
    /// <see cref="IHapticProvider"/> (assigned on the inspector; default <see cref="UnityHapticProvider"/>),
    /// resolves preconfigured entries from every cached <see cref="VibrateConfig"/> by a flat Id,
    /// and gates all playback on a <see cref="UserDataManager"/>-persisted on/off switch.
    /// </summary>
    public class VibrateManager : MonoSingleton<VibrateManager>
    {
        /// <summary>Fired when the on/off switch changes. Payload: VibrateToggledEvent</summary>
        public const string EventVibrateToggled = "Vibrate.Toggled";

        /// <summary>Fired after a haptic entry is played. Payload: VibratePlayedEvent</summary>
        public const string EventVibratePlayed = "Vibrate.Played";

        private const string EnabledUserDataKey = "Vibrate_Enabled";

        [Tooltip("Haptic backend. Pick any [SerializeReference] IHapticProvider implementation " +
                 "(default UnityHapticProvider — Unity built-in Handheld.Vibrate). Swap it from your " +
                 "own project to change backend.")]
        [SerializeReference] private IHapticProvider _hapticProvider = new UnityHapticProvider();

        private bool _isInitialized;
        private bool _isEnabled = true;
        private ConfigLocation _configLocation = ConfigLocation.Local;
        private readonly Dictionary<string, VibrateEntry> _entries = new();

        public bool IsInitialized => _isInitialized;
        public bool IsEnabled => _isEnabled;

        /// <summary>Active haptic backend. Assignable at runtime; falls back to a warning when null.</summary>
        public IHapticProvider Provider
        {
            get => _hapticProvider;
            set => _hapticProvider = value;
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        protected override void OnSingletonInitialized()
        {
            // Initialization deferred to explicit Init() call
        }

        // ── Initialization ──────────────────────────────────────────────

        public async UniTask<bool> Init(ConfigLocation configLocation = ConfigLocation.Local)
        {
            if (_isInitialized)
            {
                CustomLog.LogWarning("[VibrateManager] Already initialized.");
                return true;
            }

            _configLocation = configLocation;

            try
            {
                var configManager = ConfigManager.Instance;
                if (configManager == null)
                {
                    CustomLog.LogError("[VibrateManager] ConfigManager not available.");
                    return false;
                }

                int waitTime = 0;
                const int maxWaitTime = 30000;
                while (!configManager.IsFullInitialized && waitTime < maxWaitTime)
                {
                    await UniTask.Delay(100);
                    waitTime += 100;
                }

                if (!configManager.IsFullInitialized)
                {
                    CustomLog.LogError("[VibrateManager] ConfigManager initialization timeout.");
                    return false;
                }

                CacheEntries();
                await LoadEnabledSetting();

                _isInitialized = true;
                CustomLog.Log($"[VibrateManager] Initialized successfully. Cached {_entries.Count} entr(y/ies).");
                return true;
            }
            catch (Exception ex)
            {
                CustomLog.LogError($"[VibrateManager] Initialization failed: {ex.Message}");
                return false;
            }
        }

        private void CacheEntries()
        {
            _entries.Clear();

            var configs = ConfigManager.GetAll<VibrateConfig>(_configLocation);
            if (configs == null) return;

            foreach (var config in configs)
            {
                if (config == null) continue;

                foreach (var entry in config.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Id)) continue;

                    if (!_entries.TryAdd(entry.Id, entry))
                    {
                        CustomLog.LogWarning($"[VibrateManager] Duplicate VibrateEntry Id '{entry.Id}' ignored.");
                    }
                }
            }
        }

        private async UniTask LoadEnabledSetting()
        {
            var userDataManager = UserDataManager.Instance;
            if (userDataManager == null) return;

            _isEnabled = !await userDataManager.HasKeyAsync(EnabledUserDataKey)
                || await userDataManager.GetAsync<bool>(EnabledUserDataKey);
        }

        // ── Playback ─────────────────────────────────────────────────────

        public void Play(string id)
        {
            if (!CanPlay()) return;

            if (!_entries.TryGetValue(id, out var entry))
            {
                CustomLog.LogWarning($"[VibrateManager] VibrateEntry '{id}' not found.");
                return;
            }

            if (!TryGetProvider(out var provider)) return;
            provider.Play(entry);

            EventManager.Invoke(EventVibratePlayed, new VibratePlayedEvent { Id = id });
        }

        public void Play(HapticType type)
        {
            if (!CanPlay() || !TryGetProvider(out var provider)) return;
            provider.PlayPreset(type);
        }

        public void PlayWithCooldown(HapticType type, float cooldown)
        {
            if (!CanPlay() || !TryGetProvider(out var provider)) return;
            provider.PlayPreset(type, cooldown);
        }

        public void Stop()
        {
            if (TryGetProvider(out var provider)) provider.Stop();
        }

        public bool IsSupported()
        {
            return TryGetProvider(out var provider) && provider.IsSupported;
        }

        public void Prewarm()
        {
            if (TryGetProvider(out var provider)) provider.Prewarm();
        }

        private bool TryGetProvider(out IHapticProvider provider)
        {
            provider = _hapticProvider;
            if (provider != null) return true;

            CustomLog.LogWarning("[VibrateManager] No IHapticProvider assigned — no haptic played.");
            return false;
        }

        // ── On/off switch ────────────────────────────────────────────────

        public async UniTaskVoid SetEnabled(bool isEnabled)
        {
            _isEnabled = isEnabled;

            var userDataManager = UserDataManager.Instance;
            if (userDataManager != null)
            {
                await userDataManager.SetAsync(EnabledUserDataKey, isEnabled);
            }

            EventManager.Invoke(EventVibrateToggled, new VibrateToggledEvent { IsEnabled = isEnabled });
        }

        private bool CanPlay()
        {
            if (!_isInitialized)
            {
                CustomLog.LogWarning("[VibrateManager] Not initialized.");
                return false;
            }

            return _isEnabled;
        }
    }

    [Serializable]
    public struct VibrateToggledEvent
    {
        public bool IsEnabled;
    }

    [Serializable]
    public struct VibratePlayedEvent
    {
        public string Id;
    }
}
