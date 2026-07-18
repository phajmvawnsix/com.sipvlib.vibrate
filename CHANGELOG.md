# Changelog

## [1.1.0] - 2026-07-19

### Added
- Pluggable haptic backend via the `IHapticProvider` interface. `VibrateManager` now delegates all
  playback to a provider assigned on its inspector (a `[SerializeReference]` field), so a game can
  replace the backend from its own project without editing this package.
- `UnityHapticProvider` — the default provider shipped with the package, backed by Unity's built-in
  `Handheld.Vibrate` (single fixed buzz; no amplitude/pattern/preset control). Auto-assigned on a
  fresh `VibrateManager`.
- Provider-agnostic haptic model in `SiPVLib.Vibrate.Haptics`: `HapticType` (preset enum),
  `HapticPattern` (custom vibration), `HapticWave` (curve), and `HapticSound` (audio-envelope-driven
  "haptic sound").
- `VibrateManager.Provider` property for runtime provider assignment.

### Changed
- `VibrateConfig`/`VibrateEntry` are now provider-agnostic and no longer depend on any specific
  haptic plugin. `VibrateTriggerMode` is now `Preset`/`Pattern`/`Wave`/`Sound` (was
  `Preset`/`Pattern`/`Curve`), with payload fields `Preset` (`HapticType`), `Pattern`
  (`HapticPattern`), `Wave` (`HapticWave`), and `Sound` (`HapticSound`).
- `VibrateManager` haptic overloads are now provider-agnostic: `Play(HapticType)` and
  `PlayWithCooldown(HapticType, float)` replace the removed plugin-typed overloads.

### Removed
- All direct coupling to a specific haptic plugin. Physical output now lives entirely behind
  `IHapticProvider`.

## [1.0.0] - 2026-07-18

Initial extraction from SiPVLib monolith.
