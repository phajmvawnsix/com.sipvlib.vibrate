# com.sipvlib.vibrate

Part of [SiPVLib](https://github.com/phajmvawnsix/SiPVLib). A haptic feedback facade (`VibrateManager`) with config-driven presets (`VibrateConfig`) for quick play by Id, a `UserDataManager`-persisted on/off switch, event broadcasts (`VibrateToggledEvent`/`VibratePlayedEvent`), and a ready-made `VibrateOnInteraction` UI/pointer component.

Playback runs through a pluggable `IHapticProvider` backend, assigned on the `VibrateManager` inspector. The package ships a default `UnityHapticProvider` backed by Unity's built-in `Handheld.Vibrate` — a single fixed-duration buzz (Android ~500 ms, iOS a fixed system vibration; no-op elsewhere and in the Editor). It has no amplitude, pattern, or preset control, so every trigger mode collapses to one buzz. For real presets/patterns/waves, implement a richer `IHapticProvider` in your own project against whatever haptic API you target (a rumble driver, a third-party plugin, a test stub) and assign it in the inspector.

## Install

Add to your project's `Packages\manifest.json`:

```json
"com.sipvlib.vibrate": "https://github.com/phajmvawnsix/com.sipvlib.vibrate.git"
```

This package depends on `com.sipvlib.config`, `com.sipvlib.debugging`, `com.sipvlib.event`, `com.sipvlib.userdata`, `com.sipvlib.utilities`, and UniTask:

```json
"com.sipvlib.config": "https://github.com/phajmvawnsix/com.sipvlib.config.git",
"com.sipvlib.debugging": "https://github.com/phajmvawnsix/com.sipvlib.debugging.git",
"com.sipvlib.event": "https://github.com/phajmvawnsix/com.sipvlib.event.git",
"com.sipvlib.userdata": "https://github.com/phajmvawnsix/com.sipvlib.userdata.git",
"com.sipvlib.utilities": "https://github.com/phajmvawnsix/com.sipvlib.utilities.git",
"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
```

UPM does not automatically resolve nested git dependencies — you must add the `com.sipvlib.*` and UniTask entries above yourself alongside this package. `com.sipvlib.userdata` itself has further transitive dependencies (see its README); resolve those too.

---

## Haptic providers (`IHapticProvider`)

All actual output goes through an `IHapticProvider` assigned on the `VibrateManager` inspector — a `[SerializeReference]` field, so any `[System.Serializable]` implementation shows up in the inspector's provider dropdown. The interface is:

```csharp
public interface IHapticProvider
{
    bool IsSupported { get; }
    void Prewarm();
    void Play(in VibrateEntry entry);
    void PlayPreset(HapticType type, float cooldownSeconds = 0f);
    void Stop();
}
```

The shipped default is `UnityHapticProvider` (Unity built-in `Handheld.Vibrate`), auto-assigned on a fresh `VibrateManager` — enough for a single generic buzz. For distinct presets/patterns/waves, write your own provider **in your own project** (not this package) and pick it in the inspector. Map each `VibrateEntry.TriggerMode` onto your backend:

```csharp
using SiPVLib.Vibrate.Configs;
using SiPVLib.Vibrate.Haptics;

[System.Serializable]
public sealed class MyHapticProvider : IHapticProvider
{
    public bool IsSupported => true;
    public void Prewarm() { }

    public void Play(in VibrateEntry entry)
    {
        switch (entry.TriggerMode)
        {
            case VibrateTriggerMode.Preset: PlayPreset(entry.Preset); break;
            case VibrateTriggerMode.Pattern: /* play entry.Pattern */ break;
            case VibrateTriggerMode.Wave:    /* play entry.Wave */ break;
            case VibrateTriggerMode.Sound:   /* play entry.Sound */ break;
        }
    }

    public void PlayPreset(HapticType type, float cooldownSeconds = 0f) { /* map HapticType → your API */ }
    public void Stop() { }
}
```

You can also swap it at runtime via `VibrateManager.Instance.Provider = new MyHapticProvider();`. If the provider is `null`, `VibrateManager` logs a warning and plays nothing.

The provider-agnostic playback model lives in `SiPVLib.Vibrate.Haptics`:

- `HapticType` — preset enum (`Selection`, `Success`, `Warning`, `Failure`, `LightImpact`, `MediumImpact`, `HeavyImpact`, `RigidImpact`, `SoftImpact`, plus `None`).
- `HapticPattern` — custom vibration: an ordered array of `HapticPulse` (delay / duration / amplitude).
- `HapticWave` — wave-like vibration: an `AnimationCurve` intensity envelope over a duration, plus sharpness.
- `HapticSound` — sound-driven "haptic sound": an `AudioClip` whose loudness envelope drives the vibration.

Everything else — `VibrateManager.Init()`, entry caching, the persisted on/off switch (`IsEnabled`/`SetEnabled`), the `VibrateToggledEvent`/`VibratePlayedEvent` broadcasts, `VibrateConfig`/`VibrateEntry`, and all `SiPVLib.Vibrate.Haptics` types — works and compiles regardless of the provider. If you clear the provider (set it to `null`), `VibrateManager.Play(string id)` still resolves/validates the Id and raises `VibratePlayedEvent` but logs a warning and produces no physical output.

---

## Optional: Odin Inspector

This package integrates with [Odin Inspector](https://odininspector.com) (Sirenix) if you have it installed, but does NOT require it and does NOT bundle it — Odin is a paid Unity Asset Store asset and cannot be redistributed here.

- **Without Odin installed**: `VibrateConfig`'s `VibrateEntry` fields render with plain Unity Inspector — no conditional show/hide based on `TriggerMode`.
- **With Odin installed** (purchase + import from the Asset Store, which auto-defines the `ODIN_INSPECTOR` scripting define symbol): the `Preset`/`Pattern`/`Wave`/`Sound` fields on `VibrateEntry` are shown/hidden via `[ShowIf(nameof(TriggerMode), ...)]` to match the selected `VibrateTriggerMode`.

No manual setup is needed beyond installing Odin itself — detection is automatic via the `ODIN_INSPECTOR` define.

---

## Quick start

```csharp
using SiPVLib.Vibrate;
using SiPVLib.Vibrate.Haptics;

// Boot, once
await VibrateManager.Instance.Init();

// Play a preconfigured entry by Id (looked up across every VibrateConfig asset)
VibrateManager.Instance.Play("button_tap");

// Or play a preset directly, bypassing VibrateConfig
VibrateManager.Instance.Play(HapticType.Selection);
VibrateManager.Instance.PlayWithCooldown(HapticType.MediumImpact, cooldown: 0.1f);

// On/off switch — persisted via UserDataManager, cached in memory
VibrateManager.Instance.SetEnabled(false);
bool enabled = VibrateManager.Instance.IsEnabled;

// Listen for changes
this.ListenEvent<VibrateToggledEvent>(evt => RefreshToggleUi(evt.IsEnabled));
this.ListenEvent<VibratePlayedEvent>(evt => CustomLog.Log($"Played {evt.Id}"));
```

---

## Defining a VibrateConfig

1. Create a `VibrateConfig` asset via the Master Window "+" picker (grouped under the "Vibrate"
   category via `[ConfigCategory("Vibrate")]`).
2. Add entries to its `Entries` list — each entry has an `Id`, a `TriggerMode`
   (`Preset`/`Pattern`/`Wave`/`Sound`), and the matching provider-agnostic payload field. The active
   `IHapticProvider` maps the payload onto its backend:

| TriggerMode | Payload field | Type | Meaning |
|---|---|---|---|
| `Preset` | `Preset` | `HapticType` | Named impact/notification preset |
| `Pattern` | `Pattern` | `HapticPattern` | Custom vibration (pulse sequence) |
| `Wave` | `Wave` | `HapticWave` | Curve-driven intensity envelope |
| `Sound` | `Sound` | `HapticSound` | `AudioClip` loudness drives the vibration |

`VibrateManager.Init()` caches every entry from every `VibrateConfig` asset into one flat
Id-keyed dictionary — entry Ids must be unique across all assets (a duplicate is logged and
ignored).

---

## Interaction component

`VibrateOnInteraction` plays a `VibrateConfig` entry on pointer interaction. Attach it to:

- A UI `Button` — its `onClick` drives the `Click` phase automatically.
- Any GameObject with a raycast target (UI `Graphic` under a `GraphicRaycaster`, or a 3D/2D
  `Collider` under a `PhysicsRaycaster`/`Physics2DRaycaster`) — `IPointerClickHandler` reports the
  click directly when no `Button` is present (avoids double-firing when one is).

Set `_phases` (a `[Flags] VibratePhase`) to choose which pointer phase(s) trigger playback:
`PointerDown`, `PointerUp`, `Click`, `DragBegin`, `Drag`. Set `_vibrateId` to the `VibrateEntry.Id`
to play.

```csharp
// Example: a drag handle that vibrates only when the drag starts
var vibrate = handle.AddComponent<VibrateOnInteraction>();
// _phases = VibratePhase.DragBegin, _vibrateId = "drag_start" (set in Inspector)
```

## Known limitations

- The default `UnityHapticProvider` (Unity `Handheld.Vibrate`) is a single fixed buzz — no amplitude,
  pattern, curve, or preset differentiation; `Stop()` cannot cancel it; and it only vibrates on a
  physical Android/iOS device (no-op in the Editor and on desktop). Assign a richer `IHapticProvider`
  for real presets/patterns/waves. Editor Play Mode still exercises the full call path (Id lookup,
  on/off gate, event dispatch) regardless of the provider. On Android, `Handheld.Vibrate` needs the
  `android.permission.VIBRATE` permission — Unity adds it to the manifest automatically when the API
  is referenced.
- `_vibrateId` on `VibrateOnInteraction` is a plain string field, not a `[ConfigRef]`-backed picker
  — `ConfigRef` resolves against config-asset Ids, not the flattened per-entry Ids `VibrateManager`
  uses, so there's no drag-and-drop Inspector picker yet for entry selection.

## Documentation
- [Usage guide](USAGE.md) — original module documentation carried over from the SiPVLib monolith
