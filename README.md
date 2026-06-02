# PirateGame — Unity 6 Pirate Roguelite Class Project

PirateGame is a Unity 6 / C# 2D pirate action prototype built for the final class presentation. The current vertical slice focuses on starting or continuing a save, choosing a stage from a map, launching a run, sailing a player ship, fighting enemy waves, collecting resources, earning run rewards, defeating a boss, and spending persistent progress in the Ship Shop.

This repository has also used ChatGPT and Codex during development for brainstorming, debugging, UI polish, code support, documentation, and asset/audio support. The team reviewed, tested, and modified generated suggestions before including them in the project.

## Current gameplay loop

1. Open `MainMenu`.
2. Start a new save slot or continue an existing save slot.
3. Use the `Map` scene to pick an unlocked stage.
4. Enter the selected run scene and press the start-run button when the stage UI is configured for button-based starts.
5. Sail the player ship, fight enemy ships, survive timed waves/spawner events, and collect wood/doubloons.
6. Find treasure chests during active runs for temporary upgrade, crew, or crew-upgrade choices.
7. Survive long enough for boss encounters and defeat the boss to complete a stage and unlock later stages.
8. If the player ship is destroyed, the run summary/death flow can show run stats and navigation choices.
9. Return to the map or visit `ShipShop` to spend persistent doubloons on permanent upgrades, unlocks, and crew hiring.
10. Start another run with improved progression.

## Unity version

```text
Unity 6000.4.4f1
```

Open the repository root folder in Unity Hub:

```text
pirateGame/
├── Assets/
├── Packages/
├── ProjectSettings/
└── README.md
```

## Scenes in Build Settings

The current `ProjectSettings/EditorBuildSettings.asset` includes these enabled scenes:

```text
Assets/Scenes/MainMenu.unity
Assets/Scenes/Map.unity
Assets/Scenes/MainSea.unity
Assets/Scenes/ShipShop.unity
Assets/Scenes/Stage2.unity
Assets/Scenes/Stage3.unity
```

Scene roles:

- `MainMenu` — title/menu flow, new game, continue, save-slot list, settings, credits, background music, and global UI button audio.
- `Map` — stage selection, stage unlock status, navigation to the Ship Shop or Main Menu.
- `MainSea` — primary stage/run scene used by Stage 1 and default map configuration.
- `ShipShop` — persistent upgrade and crew-hiring scene.
- `Stage2` / `Stage3` — additional build-setting stage scenes for later/unlocked stage content.

## Controls

### General / ship controls

```text
WASD: move the player ship while boarded
Left mouse button: fire cannonballs toward the mouse cursor while boarded
Space: also fires toward the mouse cursor while boarded
Arrow keys: fire directional cannons while boarded
R: repair the ship if ShipRepairController is present and the player has enough wood
U: open/close the pause/progression menu in run scenes and the ShipShop pause menu where those components are present
Shift: dash if the dash system is present and unlocked
```

### Interaction controls

```text
E: board/interact where applicable, including shop stand interactions when configured
```

## Major features

### Save slots and persistent progression

- `MainMenuController` supports creating a new save, continuing existing saves, selecting save slots, renaming save slots, and deleting save slots.
- `PlayerProgression` stores save-slot data with `PlayerPrefs`, including doubloons, upgrade levels, unlocks, crew unlocks, cosmetics, completed stages, and highest unlocked stage.
- The pause/progression menu can save the active slot and show progression/run information during play.

### Map and stage selection

- `MapSceneController` provides Stage 1, Stage 2, and Stage 3 buttons.
- Stage buttons are gated by progression unlock state.
- Boss victory can mark a stage complete and unlock later stages through `PlayerProgression`.

### Start-run flow

- `StageStartController` can force the player onto the ship at stage start, hide the walking player while boarded, lock unboarding during a run, and show a `Start Run` button.
- `RunTimerDirector` supports runs that start when the player boards, from a button, or immediately, depending on scene configuration.

### Ship combat

- `ShipController2D` handles boarded WASD ship movement.
- `CannonShooter` supports left-click and Space mouse-aimed firing, plus arrow-key directional cannon fire.
- `Cannonball` and ship-health scripts support projectile damage, player/enemy hits, explosive cannonballs, pierce upgrades, and owner filtering.
- `ShipRepairController` supports spending wood with `R` to heal the ship when that component is attached and inventory/health references are available.

### Enemy waves, spawners, and boss encounters

- Enemy spawners and enemy AI scripts create hostile ships that chase/attack the player ship.
- `RunTimerDirector` can enable normal spawners when a run starts, trigger timed spawner events, display event messages, and spawn a boss after a configured time.
- `BossDefeatHandler` handles victory messaging, stage completion, and optional stage-complete summaries.

### Resources: wood and doubloons

- Enemy deaths can drop resources.
- `PlayerInventory` stores run resources such as wood and doubloons.
- Doubloons are also used for persistent progression through `PlayerProgression`.
- Wood is used by the repair controller when repairs are available.

### ShipShop upgrades and crew hiring

`ShipShopController` supports permanent purchases and unlocks such as:

- Base health upgrades
- Health regeneration unlock
- Base speed upgrades
- Dash unlock
- Base cannon damage upgrades
- Cannonball size, speed, shoot-rate, explosion, and pierce unlocks/upgrades
- Magnet unlock
- Force-field unlock/damage upgrades
- Barnacles and cursed-doubloon ability upgrades
- Crew hiring through the crew menu when crew NPCs and data are configured

### Treasure chest rewards

- `TreasureChestSpawner` can spawn treasure chests during active runs.
- `TreasureChestPickup` opens reward choices when the player collects a chest.
- `TreasureChestChoiceUI` pauses the game and lets the player choose up to three rewards from available upgrade, crew, and crew-upgrade choices.

### Music and UI audio

- `MusicManager` is present in `MainMenu`, persists across scene loads, plays configured default background music, and stores music volume with `PlayerPrefs`.
- `MusicVolumeSliderUI` connects a UI slider to the persistent music manager.
- `UIAudioManager` is present in `MainMenu`, persists across scene loads, and automatically adds button-click sound hooks to Unity UI buttons.

### Run summary, death, and stage complete flow

- `RunSummaryController` can show death and stage-complete summaries, pause time while the summary is visible, show run duration/resource/progression information, and navigate to retry, map, Ship Shop, or Main Menu.
- Player death and boss defeat are the main paths that can trigger the summary flow when the scene objects are configured.

## Technologies used

- Unity 6 (`6000.4.4f1`)
- C# scripts
- Unity 2D physics and UI systems
- TextMeshPro
- Unity Input System package
- PlayerPrefs for local save/progression data
- Git and GitHub for version control and collaboration
- ChatGPT/Codex for brainstorming, debugging, UI polish, code support, documentation, and asset/audio support

## Team member contributions

> Replace the placeholders below with exact team member names before the final presentation if desired.

- Team Member 1 — core gameplay programming, player ship controls, combat, and run loop.
- Team Member 2 — enemy spawning/AI, boss encounters, stage timing, and balancing.
- Team Member 3 — UI/menu work, save slots, map flow, run summaries, and layout polish.
- Team Member 4 — ShipShop upgrades, persistent progression, crew systems, and treasure rewards.
- Team Member 5 — art/audio support, music, UI button audio, scene setup, testing, and presentation polish.

## Known issues and limitations

- Some systems are component/configuration dependent. Features such as repairs, boss health UI, start-run panels, treasure chests, run summaries, and crew rewards require the correct scene objects and Inspector references.
- `MapSceneController` defaults Stage 1, Stage 2, and Stage 3 scene-name fields to `MainSea`; the Build Settings also include separate `Stage2` and `Stage3` scenes, so stage scene routing should be verified in the Unity Inspector before the presentation.
- Music and UI click audio are implemented and present in `MainMenu`, but audio clips, mixer settings, and volume-slider wiring should be tested in Play Mode.
- Save data is stored locally with `PlayerPrefs`, so saves are machine/user specific and can persist between test runs unless reset or deleted through the save UI.
- Balance values for enemy spawn timing, boss timing, upgrade prices, repair costs, and reward frequency may still need final tuning.
- Some fallback UI is generated at runtime by scripts if scene UI references are missing; presentation scenes should use polished configured UI where possible.

## AI tools statement

ChatGPT and Codex were used as development assistants for brainstorming gameplay ideas, debugging C# issues, improving UI polish, supporting documentation updates, and helping with asset/audio planning or implementation support. AI output was not treated as final by itself: the team inspected, tested, edited, and integrated the work into the Unity project.

## How to run for the presentation

1. Clone or pull the latest `main` branch.
2. Open the repository root folder in Unity Hub using Unity `6000.4.4f1` or a compatible Unity 6 version.
3. Open `Assets/Scenes/MainMenu.unity`.
4. Confirm Build Settings / Build Profiles include these enabled scenes:

   ```text
   MainMenu
   Map
   MainSea
   ShipShop
   Stage2
   Stage3
   ```

5. Press Play.
6. Start a new save or continue an existing save.
7. Select an unlocked stage on the map.
8. Press the stage `Start Run` button if shown, then play the run.
9. After testing a run, use the run summary, pause menu, map, or Ship Shop navigation to show progression features.
