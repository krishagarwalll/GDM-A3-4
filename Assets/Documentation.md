# Game Systems Reference

A guide to the gameplay systems in this project so you can reuse them in your own Unity 2D game. Every section lists what the system does, the files involved, the inspector fields you care about, and a short "how to set it up" recipe.

The systems are designed to be **decoupled through ScriptableObject event channels**, so most pieces talk to each other through assets (channels, level data) rather than direct references. This means most systems can be dropped into a new scene independently.

---

## Table of contents

1. [Event Channels](#1-event-channels)
2. [Scene Flow (Bootstrapper, GameSceneSO, SceneLoader, SceneTransition)](#2-scene-flow)
3. [Level Data and Level Starter](#3-level-data-and-level-starter)
4. [Game Timer](#4-game-timer)
5. [Objective Tracker](#5-objective-tracker)
6. [Audio Manager](#6-audio-manager)
7. [Player Controller](#7-player-controller)
8. [Interactions (IInteractable + interactables)](#8-interactions)
9. [Patrol System](#9-patrol-system)
10. [Vision Cones (FieldOfView + VisionSweep)](#10-vision-cones)
11. [Enemy AI](#11-enemy-ai)
12. [UI System (UIPanel, binders, requesters, displays)](#12-ui-system)
13. [Utilities](#13-utilities)

---

## 1. Event Channels

**Files:** `Assets/Scripts/Core/Events/`

Four ScriptableObject channels that systems use to broadcast events without referencing each other directly:

| Asset | Carries |
|---|---|
| `VoidEventChannelSO` | nothing (just a signal) |
| `IntEventChannelSO` | an `int` |
| `FloatEventChannelSO` | a `float` |
| `GameSceneEventChannelSO` | a `GameSceneSO` |

Each channel exposes:
* `OnEventRaised` (a `UnityAction`) for listeners to subscribe
* `Raise(...)` for senders to fire it

### How to use it

1. In the Project window, right click and pick `Create > Game > Events > <Type> Event Channel`. Name it something like `OnPlayerCaught`.
2. Drag that asset into any **publisher** field on a component (look for headers labelled `Channels (output)`).
3. Drag the same asset into any **subscriber** field (look for `Channels (input)` headers).
4. At runtime, the publisher calls `channel.Raise(...)` and every subscriber's handler fires.

### When to add a new channel type

If you need to pass a new payload (e.g. a `Vector3`), copy one of the existing files, change the type, and update the `[CreateAssetMenu]` path. Keep them as small as possible.

> Tip: channels survive scene loads because they are assets, but their subscriber lists are rebuilt every time a MonoBehaviour calls `OnEnable` / `OnDisable`. Always subscribe in `OnEnable` and unsubscribe in `OnDisable` to avoid stale listeners.

---

## 2. Scene Flow

**Files:** `Assets/Scripts/Core/Scenes/`

A small scene loader that uses additive scenes and a curtain transition.

### `GameSceneSO`

ScriptableObject describing one scene. Fields:
* `sceneName` (must match the file name in Build Settings)
* `displayName`
* `sceneType` (`Menu`, `Gameplay`, `PersistentManagers`)

Create one asset per scene at `Create > Game > Scenes > Game Scene`.

### `Bootstrapper`

First MonoBehaviour to run on game launch. Loads the persistent managers scene additively if it is not already loaded, then raises `loadSceneRequest` with `firstSceneToLoad`. Optionally unloads its own scene afterwards.

### `SceneLoader`

Lives on the persistent managers scene. Listens to a `GameSceneEventChannelSO` and:
1. Plays a `SceneTransition` cover
2. Unloads the current gameplay scene
3. Loads the requested scene additively and sets it active
4. Plays the reveal
5. Raises `onSceneLoaded` so other systems (pause, music, etc) can react

Press `R` (configurable) to reload the current gameplay scene.

### `SceneTransition`

A simple two-bar curtain. Exposes `Cover()`, `Reveal()`, and `RunBetween(IEnumerator middle)`. Any system can drive it (death screens, cutscenes, the loader). Idempotent and uses `unscaledDeltaTime` so it runs while paused.

### Setup recipe

This is already wired up in the project. You only need to redo this if you are starting a fresh project from scratch.

1. Create one `GameSceneSO` asset per scene. Make sure each `sceneName` matches a scene listed in Build Settings.
2. Build a "Boot" scene: empty scene with one Bootstrapper. Put it at build index 0.
3. Build a "_PersistentManagers" scene with your audio manager, pause controller, scene loader, scene transition, UI canvas, and any persistent channels.
4. Create a `GameSceneEventChannelSO` (e.g. `LoadSceneRequest`). Wire it to:
   * `Bootstrapper.loadSceneRequest`
   * `SceneLoader.loadSceneRequest`
   * Every `LoadSceneRequester` / `LoadNextLevelRequester` / `RestartLevelRequester` button.
5. Set `Bootstrapper.firstSceneToLoad` to your main menu scene.

> Reminder: when adding a new gameplay scene, do **not** copy `_PersistentManagers` content into it. The scene loader unloads the old gameplay scene and loads yours additively on top of `_PersistentManagers`, which is already there.

---

## 3. Level Data and Level Starter

**Files:** `Assets/Scripts/Core/Game/LevelDataSO.cs`, `LevelStarter.cs`

`LevelDataSO` is the single source of truth for a level's settings:
* `displayName`, `objectiveLabel`
* `durationSeconds` (timer)
* `objectiveCount` (how many things to collect)
* `currentScene`, `nextScene`, `mainMenuScene` (`GameSceneSO` refs used by UI button requesters)

`LevelStarter` is a scene-side glue object. On `Awake` it:
* Resets `Time.timeScale` to 1 (in case a previous scene paused)
* Pushes `durationSeconds` into a referenced `GameTimer`
* Pushes `objectiveCount` into a referenced `ObjectiveTracker`

### Setup recipe

1. Right click in Project, pick `Create > Game > Game > Level Data`. Configure it for the level.
2. In the gameplay scene, add a GameObject called "LevelStarter" with the `LevelStarter` component.
3. Drag the `LevelDataSO` asset into the `levelData` field.
4. Drag the scene's `GameTimer` and `ObjectiveTracker` into the corresponding fields.

To make a new level, duplicate the `LevelDataSO` asset, point its `currentScene` / `nextScene` at the right `GameSceneSO`s, then duplicate the gameplay scene and swap the `LevelDataSO` reference on the LevelStarter.

---

## 4. Game Timer

**Files:** `Assets/Scripts/Core/Game/GameTimer.cs`

Counts down `durationSeconds` and broadcasts:
* `onTick` (`FloatEventChannelSO`) every frame, with seconds remaining
* `onTimeUp` (`VoidEventChannelSO`) once when it hits 0

Auto-starts by default. Public methods: `StartTimer()`, `StopTimer()`, `Reset(float duration)`.

### Setup recipe

1. Add a `GameTimer` component to a scene GameObject.
2. Create two channels: `OnTimerTick` (Float) and `OnTimeUp` (Void).
3. Hook them up to a `TimerDisplay` for UI and to `PauseController.lockChannels` / `LevelOutcomeBinder` for the lose screen.
4. Let `LevelStarter` push the duration in.

---

## 5. Objective Tracker

**Files:** `Assets/Scripts/Core/Game/ObjectiveTracker.cs`

Counts how many times an `onObjectiveProgress` Void channel has been raised. Once `CurrentCount >= requiredCount`, raises `onComplete` (Void) and stops counting. Also raises `onCountChanged` (Int) every step so the UI can update.

### Setup recipe

1. Create three channels: `OnPaintingStolen` (Void), `OnObjectiveCountChanged` (Int), `OnAllObjectivesComplete` (Void).
2. Add `ObjectiveTracker` to a scene GameObject and wire all three.
3. On every `PaintingInteractable`, set its `onStolen` channel to `OnPaintingStolen`.
4. Hook `OnObjectiveCountChanged` to an `ObjectiveCounterDisplay` and `OnAllObjectivesComplete` to whatever opens the exit door.

---

## 6. Audio Manager

**Files:** `Assets/Scripts/Core/Audio/AudioManager.cs`

Singleton (`AudioManager.Instance`) that lives on the persistent managers scene with `DontDestroyOnLoad`. Splits music (looping) from SFX (one-shots) on two `AudioSource`s.

```csharp
AudioManager.Instance.PlayMusic(clip);
AudioManager.Instance.PlaySFX(clip, volumeScale: 0.8f);
AudioManager.Instance.SetMusicVolume(0.6f);
AudioManager.Instance.SetSfxVolume(1f);
```

### Setup recipe

1. Add a GameObject called "AudioManager" to the persistent managers scene.
2. Add the `AudioManager` component. The two AudioSources are auto-created if you do not assign them.
3. From any script, call `AudioManager.Instance.PlayMusic(...)` / `PlaySFX(...)`.

---

## 7. Player Controller

**Files:** `Assets/Scripts/Character/Player.cs`

A 2D top-down player with WASD movement, sprite flip, and a toggle crouch. Uses Unity's new Input System (reads `Keyboard.current` directly).

Public surface:
* `Player.Instance` (singleton)
* `IsCrouching`, `IsMoving`, `GetPosition`

Animator parameters expected on the controller:
* `isRunning` (bool)
* `isCrouched` (bool)

Crouching halves move speed (`CROUCH_SPEED_MULTIPLIER = 0.35`), reduces vision detection on enemies, and silences hearing.

### Setup recipe

1. Add a 2D GameObject with `Rigidbody2D` (Dynamic, gravity 0, freeze rotation Z), a `Collider2D`, an `Animator`, and the `Player` script.
2. Assign an Animator Controller with `isRunning` and `isCrouched` bool parameters and your idle / walk / crouch states.
3. Optional: change `crouchToggleKey` in the inspector if you want a different key.

> Note: `Player` calls `PauseService.IsPaused` to ignore input while paused.

---

## 8. Interactions

**Files:** `Assets/Scripts/Interactions/`

An interface plus a player-side detector and several reusable interactables.

### `IInteractable`

```csharp
public interface IInteractable {
    void Interact();
    string GetInteractiveText();
}
```

Implement on any component you want the player to be able to "use" with the interact key.

### `PlayerInteract`

Goes on the player. Each frame checks `Keyboard.current.eKey.wasPressedThisFrame`, then does an `OverlapCircleAll` within `interactRange` and calls `Interact()` on the closest `IInteractable`. Respects the pause service.

### `PlayerInteractUI`

UI component. Polls `PlayerInteract.GetInteractableObject()` and shows / hides a prompt with the current `GetInteractiveText()`.

### Built-in interactables

| Component | What it does |
|---|---|
| `KeyInteractable` | Picks itself up, increments `KeyInteractable.HeldKeyCount`. |
| `ChestInteractable` | Plays an `Open` animator trigger, optionally consumes a key. |
| `PaintingInteractable` | Plays a stolen animation, raises `onStolen`. Used to feed `ObjectiveTracker`. |
| `GuardInteractable` | Calls `EnemyAI.KnockOut()` on a guard, optionally requires the player to be crouched. |
| `ExitInteractable` | Raises `onEscaped` once the linked `ObjectiveTracker.IsComplete`. |

### Setup recipe (player)

1. Add `PlayerInteract` to the player GameObject. Tune `interactRange`.
2. Add `PlayerInteractUI` to a Canvas with a TMP label inside a container that can be toggled.

### Setup recipe (an interactable)

1. Drop the matching prefab or add the component to a 2D GameObject with a `Collider2D` (set `IsTrigger` if you want walk-through).
2. Wire its `onXxx` Void channel if it has one.
3. Fill in the inspector text fields so the prompt reads naturally.

### Adding a new interactable

```csharp
[DisallowMultipleComponent]
public class LeverInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private VoidEventChannelSO onPulled;
    [SerializeField] private string text = "Pull lever";
    private bool _used;

    public void Interact() {
        if (_used) return;
        _used = true;
        onPulled?.Raise();
    }

    public string GetInteractiveText() => _used ? "" : text;
}
```

---

## 9. Patrol System

**Files:** `Assets/Scripts/Patrol/`

Strategy pattern for "where should the guard walk next?". `EnemyAI` calls into a `Patroller` without caring which kind it is.

### Abstract `Patroller`

```csharp
public abstract Vector3 GetTarget();
public abstract bool IsReady();
public abstract void Advance();
public bool HasReached(Vector3 currentPosition);
```

### Built-in implementations

| Component | Behaviour |
|---|---|
| `WaypointPatroller` | Cycles through an array of waypoint Transforms. |
| `RandomPatroller` | Picks random points within `[minDistance, maxDistance]` of the guard each Advance. |
| `StaticPatroller` | Returns a single home position. Used together with `VisionSweep` for stationary guards. |

### Setup recipe

1. Add the desired `Patroller` subclass to the guard GameObject.
2. For waypoint guards, populate the `waypoints` array with empty child Transforms in the scene.
3. Drag the patroller into `EnemyAI.patroller`.

### Adding a new pattern

Inherit from `Patroller`, override the three abstract methods, and you are done. The `EnemyAI` will use it automatically.

---

## 10. Vision Cones

**Files:** `Assets/Scripts/VisionCones/FieldOfView.cs`, `VisionSweep.cs`

### `FieldOfView`

Draws a procedural cone mesh (rays from `origin`, blocked by `layerMask`) and exposes:
* `IsTargetVisible(Vector3 target, float distanceMultiplier = 1f)`
* `SetOrigin`, `SetAimDirection`, `SetFoV`, `SetViewDistance`
* Raises `onPlayerSpotted` (Void) the first time it actually sees the player

Inspector:
* `layerMask` should include walls and the player
* `fov` (degrees), `viewDistance`, `rayCount` for resolution

### `VisionSweep`

Sits on the same GameObject as a `FieldOfView`. Sweeps the cone's aim direction back and forth around `baseDirection` over `sweepPeriod` seconds. Used by static guards to feel less robotic.

### Setup recipe

1. Create a guard prefab. Add an empty child called "FOV" with a `MeshFilter`, a `MeshRenderer` (assign your cone material), the `FieldOfView` script, and optionally `VisionSweep`.
2. The `EnemyAI` script instantiates a fresh FOV instance for each guard at runtime, so wire it to the `pfFieldOfView` field as a prefab reference, not a scene reference.
3. Set `layerMask` so it includes geometry that should block sight (walls, doors).

---

## 11. Enemy AI

**Files:** `Assets/Scripts/Enemy/EnemyAI.cs`

State machine guard with the A* Pathfinding Project (`AIPath`). States:

| State | Behaviour |
|---|---|
| `Patrol` | Drives along the assigned `Patroller`. Watches for sight and noise. |
| `Suspicious` | Stops, looks at the player, hesitates `suspicionDelay` seconds before committing. |
| `Alert` | Chases via `AIPath` toward the last known player position. |
| `Search` | Drives to last known position, gives up after `searchDuration`. |
| `KnockedOut` | Frozen, FOV disabled. Triggered by `GuardInteractable`. |

Two guard archetypes via `GuardType`:
* `Patrolling`: walks along its patroller.
* `Static`: stays at home, sweeps with `VisionSweep`, only chases within `chaseLeashRadius` of its home.

Sight is reduced when the player is crouching (`crouchedDetectionMultiplier`). Hearing accumulates while the player is moving and not crouched within `hearingRadius`, decays otherwise.

### Setup recipe

1. Drag in a guard prefab.
2. Add `Seeker` and `AIPath` (from A* Pathfinding Project) plus a `Rigidbody2D`.
3. Add `EnemyAI`. Set `guardType`, drag in the FOV prefab, and either `WaypointPatroller` or `StaticPatroller`.
4. Tune ranges, speeds, and timings in the inspector.
5. Bake an A* grid graph that covers the level.

To wire "guard catches player": on the FOV prefab, fill in `onPlayerSpotted` with a Void channel. Subscribe `LevelOutcomeBinder`'s lose outcome and `PauseController.lockChannels` to it.

---

## 12. UI System

**Files:** `Assets/Scripts/UI/`

A small set of glue components that drive the canvas through event channels. None of them call gameplay code directly.

### `UIPanel`

Toggleable panel. Subscribes to a `showChannel` and `hideChannel`. Optionally pauses the game while visible (via legacy `Time.timeScale`, but for stack-aware pause use `PauseController` instead).

### `PanelChannelBinder`

Workaround for panels that start disabled: a sibling always-active GameObject that subscribes to the channels for them. Use this whenever your panel starts off and needs to be turned on by an event.

### `LevelOutcomeBinder`

Drives a single shared "Win / Lose" panel. For each `Outcome` you list:
* a `trigger` channel (e.g. `OnEscaped`, `OnPlayerCaught`, `OnTimeUp`)
* a `title` string
* a list of buttons to show (others are hidden)

When a trigger fires it sets the title, swaps which buttons are visible, and shows the panel.

### `TimerDisplay`

Subscribes to a Float channel of seconds remaining and writes a `m:ss` TMP label. Below `warnAtSeconds` it tints the label red.

### `ObjectiveCounterDisplay`

Subscribes to an Int channel and writes `"<label>: <current> / <max>"` from the linked `LevelDataSO`.

### `LoadSceneRequester`, `LoadNextLevelRequester`, `RestartLevelRequester`

Tiny adapters for buttons. Each has a `Request()` you wire to the button's `OnClick`. They raise the right `GameSceneEventChannelSO` based on a direct `GameSceneSO` reference or a `LevelDataSO` lookup.

### `PauseRequester`

Three methods (`Pause`, `Resume`, `Toggle`) so a button can hit `PauseController.Instance` without holding a direct reference.

### `PlayerInteractUI`

Toggles a prompt while the player is near an `IInteractable` and writes the text it returns.

### `GameSceneManager` (legacy)

Older singleton-based loader that just calls `SceneManager.LoadSceneAsync` by build index. Prefer the channel-based `SceneLoader` for new work. Kept around because some menu buttons still hook into it.

### Setup recipe (a win panel)

1. Create one channel per outcome (`OnEscaped`, `OnPlayerCaught`, `OnTimeUp`).
2. Build a panel GameObject with a TMP title and three buttons (Next / Restart / Menu).
3. Add `UIPanel` and `LevelOutcomeBinder` on the panel root. Add a `PanelChannelBinder` on a parent that stays active so it can show the panel from off.
4. Configure two `Outcome` entries in `LevelOutcomeBinder` (Win and Lose) with their titles and which buttons they show.
5. On each button, add a `LoadNextLevelRequester` / `RestartLevelRequester` / `LoadSceneRequester`, hook `Request()` to the button's `OnClick`.

---

## 13. Utilities

**Files:** `Assets/Scripts/Utilities/Utils.cs`

Two helpers used by `FieldOfView` and `VisionSweep`:

```csharp
Vector3 dir = Utils.GetVectorFromAngle(45f);          // unit vector at 45°
float deg = Utils.GetAngleFromVectorFloat(Vector3.up); // 90°
```

Add anything that is project-wide, stateless, and small to this file.

---

## Putting it all together: building a new level

1. **Make a `GameSceneSO`** for the new scene and add the scene to Build Settings.
2. **Make a `LevelDataSO`** with the timer length, objective count, and `currentScene` / `nextScene` references. Without this asset the level will technically run, but the win / lose buttons silently disable themselves and the objective counter shows `0 / 0`.
3. **Duplicate an existing gameplay scene** as your starting point. It should contain:
   * A `LevelStarter` pointing at the new `LevelDataSO`
   * A `GameTimer` and `ObjectiveTracker`
   * The player prefab, guard prefabs, paintings, exit, etc
   * An A* graph covering the walkable area

   It should **not** contain an `AudioManager`, `PauseController`, `SceneLoader`, or `SceneTransition`. Those live in `_PersistentManagers` and are loaded for you.
4. **Wire the channels** the level needs: timer tick, time up, painting stolen, objective count changed, all complete, escaped, player caught, player spotted.
5. **Add the level to your level select menu** by creating a button with a `LoadSceneRequester` pointing at the new `GameSceneSO`. Do **not** point the button at the scene directly via `SceneManager.LoadScene` or via Unity's built-in scene loader, those bypass the curtain transition, the level data setup, and the persistent managers.
6. Press play **from the boot scene** (build index 0). The flow is:
   * Boot loads `_PersistentManagers` additively
   * Boot raises the load request for the main menu
   * The player picks a level, which raises another load request
   * `SceneLoader` covers the curtain, swaps scenes, reveals
   * `LevelStarter` configures the timer and tracker for the new level

   If you press play while a gameplay scene is open, none of the persistent systems will be there and most UI buttons will appear to do nothing.

---

## Conventions worth keeping

* Subsystems talk through **ScriptableObject channels**, not through direct references where possible.
* Subscribe in `OnEnable`, unsubscribe in `OnDisable`. Never in `Awake` / `OnDestroy`.
* Anything that should respect pause checks `PauseService.IsPaused` at the top of `Update`.
* Singletons (`Player`, `AudioManager`, `PauseController`, `SceneTransition`) clear their `Instance` reference in `OnDestroy` so domain reloads stay clean.
* Static state on `PauseService` and `KeyInteractable` is reset on `RuntimeInitializeOnLoadMethod` or by design (`PauseService`) / on level load (you may want to do the same for `KeyInteractable.HeldKeyCount` if a player can carry keys between attempts is undesirable).
