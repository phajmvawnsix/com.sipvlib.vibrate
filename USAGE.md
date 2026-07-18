# SiPV.Vibrate

Mobile haptic feedback facade with config-driven presets for quick play, a persisted on/off switch,
and a ready-made UI/pointer interaction component. Playback goes through a pluggable
`IHapticProvider` backend assigned on the `VibrateManager` inspector. The package ships a default
`UnityHapticProvider` (Unity built-in `Handheld.Vibrate` — a single fixed buzz); implement your own
provider for richer presets/patterns/waves (see README).

Depends on `SiPV.Config` (`VibrateConfig`/`ConfigManager`), `SiPV.UserData` (on/off switch
persistence), `SiPV.Event` (change/play broadcasts), `SiPV.Debugging` (`CustomLog`), `SiPV.Utilities`
(`MonoSingleton`), and `UniTask`.

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

// Assign / swap the haptic backend at runtime
// VibrateManager.Instance.Provider = new MyHapticProvider();

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
   (`Preset`/`Pattern`/`Wave`/`Sound`), and the matching provider-agnostic payload field:

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

## Haptic providers

`VibrateManager` delegates all output to an `IHapticProvider` assigned on its inspector (a
`[SerializeReference]` field). The shipped default is `UnityHapticProvider` (Unity `Handheld.Vibrate`
— a single fixed buzz, no amplitude/pattern/preset control). For richer output, write a
`[System.Serializable]` class implementing `IHapticProvider` **in your own project** and pick it in
the inspector dropdown, or assign it at runtime via `VibrateManager.Instance.Provider`. The
provider-agnostic model (`HapticType`, `HapticPattern`, `HapticWave`, `HapticSound`) lives in
`SiPVLib.Vibrate.Haptics`. See the README for the full interface and a template.

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

- The default `UnityHapticProvider` (`Handheld.Vibrate`) is a single fixed buzz — no amplitude,
  pattern, curve, or preset differentiation, and `Stop()` cannot cancel it. Assign a richer
  `IHapticProvider` for real presets/patterns/waves.
- Actual device vibration can only be observed on a physical Android/iOS device — Editor Play Mode
  exercises the full call path (including the on/off gate and event dispatch) but produces no
  physical feedback.
- `_vibrateId` on `VibrateOnInteraction` is a plain string field, not a `[ConfigRef]`-backed picker
  — `ConfigRef` resolves against config-asset Ids, not the flattened per-entry Ids `VibrateManager`
  uses, so there's no drag-and-drop Inspector picker yet for entry selection.
