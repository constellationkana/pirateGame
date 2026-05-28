# PirateGame

A Unity 6 2D pirate prototype focused on a small playable vertical slice: board a ship, sail around an ocean scene, fight enemy ships, collect resources, and spend progression in a ship shop.

## Current gameplay loop

1. Start from the main menu or main sea scene.
2. Walk to the player ship and board it.
3. Sail with the player ship.
4. Fire cannons at enemy ships.
5. Avoid enemy ships and projectiles.
6. Destroy enemies to earn wood and doubloons.
7. Use resources and upgrades to improve future runs.
8. When the player ship is defeated, load the `ShipShop` scene.
9. Buy upgrades, then return to `MainSea` for another run.

## Unity version

This project is currently set up for:

```text
Unity 6000.4.4f1
```

Open the project with Unity Hub using the repository root folder, not a duplicate local copy.

Expected local project structure:

```text
pirateGame/
├── Assets/
├── Packages/
├── ProjectSettings/
└── README.md
```

## Scenes

The build settings currently include these scenes:

```text
Assets/Scenes/MainMenu.unity
Assets/Scenes/MainSea.unity
Assets/Scenes/ShipShop.unity
```

`MainSea` is the main gameplay scene. `ShipShop` is the upgrade/shop scene that the player reaches after the player ship is defeated.

## Controls

### On foot

```text
WASD / movement input: move player
E: board/interact where applicable
```

### While boarded on PlayerShip

```text
WASD: move PlayerShip
Arrow keys: fire cannon in fixed directions
Space: fire toward mouse cursor
```

### ShipShop

```text
E: interact with shop stands when in range
```

## Major systems

### Player ship

Key scripts:

```text
Assets/Scripts/ShipController2D.cs
Assets/Scripts/CannonShooter.cs
Assets/Scripts/ShipHealth.cs
Assets/Scripts/PlayerShipDefeatHandler.cs
Assets/Scripts/PlayerStartingStatsApplier.cs
```

The player ship handles boarded movement, cannon shooting, health, defeat scene transition, and application of permanent upgrades from progression.

### Boarding

Key scripts:

```text
Assets/Scripts/BoardShipTrigger.cs
Assets/Scripts/PlayerWalk2D.cs
```

The player can board the ship through a trigger zone. While boarded, the ship becomes the main controlled object.

### Enemy ships

Key scripts:

```text
Assets/Scripts/EnemyShipSpawner.cs
Assets/Scripts/SimpleEnemyShipAI.cs
Assets/Scripts/EnemyShipAttack.cs
Assets/Scripts/ShipDeathDropper.cs
```

Enemy ships spawn around the player ship, receive runtime references to `PlayerShip`, chase/follow the player ship, attack, take damage, and drop resources on death.

When debugging spawned enemies, verify that spawned `EnemyShip(Clone)` objects have:

```text
SimpleEnemyShipAI enabled
EnemyShipAttack enabled
SimpleEnemyShipAI → Target Ship assigned
SimpleEnemyShipAI → Player Ship Controller assigned
EnemyShipAttack → Target Ship assigned
EnemyShipAttack → Player Ship Controller assigned
EnemyShipAttack → Target Ship Health assigned
```

### Projectiles

Key scripts:

```text
Assets/Scripts/Cannonball.cs
Assets/Scripts/CannonShooter.cs
Assets/Scripts/EnemyShipAttack.cs
```

Both player cannon shots and enemy projectiles use projectile damage logic that can damage objects with `ShipHealth` while ignoring the projectile owner.

### Resources and inventory

Key scripts:

```text
Assets/Scripts/PlayerInventory.cs
Assets/Scripts/ResourcePickup.cs
Assets/Scripts/InventoryHUDController.cs
```

Enemy ships can drop wood and doubloons. Pickups add to the player's inventory. Doubloons also feed into persistent progression through `PlayerProgression`.

### Progression and upgrades

Key scripts:

```text
Assets/Scripts/PlayerProgression.cs
Assets/Scripts/ShipShopController.cs
Assets/Scripts/ShopStandInteraction.cs
Assets/Scripts/UpgradeManager.cs
Assets/Scripts/PlayerStartingStatsApplier.cs
```

Persistent upgrades use `PlayerPrefs` through `PlayerProgression`. The ship shop can sell upgrades such as health, speed, magnet radius, cannon damage, dash unlock, force-field unlock, and cosmetics. `PlayerStartingStatsApplier` applies permanent upgrade levels at the start of a run.

### Runtime upgrades

Key scripts:

```text
Assets/Scripts/UpgradeManager.cs
Assets/Scripts/UpgradeChoiceUI.cs
Assets/Scripts/PlayerLevelSystem.cs
```

The run can offer upgrade choices as the player levels up. Some upgrades are gated by shop unlocks.

## Important setup notes

### PlayerShip requirements

The `PlayerShip` GameObject should have:

```text
ShipController2D
ShipHealth
CannonShooter
PlayerInventory
PlayerShipDefeatHandler
PlayerStartingStatsApplier
Rigidbody2D
Collider2D
```

Recommended:

```text
Name: PlayerShip
Tag: PlayerShip
Rigidbody2D Gravity Scale: 0
Freeze Rotation Z: checked
```

### PlayerShip cannon points

The player ship should have directional cannon spawn points:

```text
CannonPoint
CannonPointUp
CannonPointDown
CannonPointLeft
CannonPointRight
```

Assign those to `CannonShooter` in the Inspector.

### Enemy prefab requirements

Enemy prefabs should have:

```text
SimpleEnemyShipAI
EnemyShipAttack
ShipHealth
ShipDeathDropper
Rigidbody2D
Collider2D
```

Recommended:

```text
Rigidbody2D Gravity Scale: 0
Rigidbody2D Simulated: checked
Freeze Rotation Z: unchecked if using MoveRotation for facing
```

### ShipShop setup

`ShipShop` should include:

```text
ShipShopController
ShopStandInteraction objects
TMP text fields for shop feedback and upgrade labels
A stand or button that calls ShipShopController.StartRun()
```

`ShipShop` must be listed in Build Settings / Build Profiles for `SceneManager.LoadScene("ShipShop")` to work.

## Repository sync warning

This project previously had duplicate local repo folders. If Unity does not show the latest scripts, verify the exact folder Unity opened:

```text
Right-click a script in Unity → Show in Explorer
```

Make sure it points to the same local folder used by GitKraken / terminal. If not, open the correct project folder through Unity Hub.

## Suggested vertical-slice polish checklist

### Critical

- Confirm spawned enemy prefabs move, face, attack, die, and drop resources.
- Confirm enemy projectiles damage `PlayerShip`.
- Confirm `PlayerShip` health reaching zero loads `ShipShop`.
- Confirm `ShipShop` is listed in Build Settings.
- Confirm upgrades bought in `ShipShop` are applied when returning to `MainSea`.

### Must-have polish

- Add clear player health UI.
- Add clearer hit feedback for player and enemy ships.
- Add simple sound effects for cannon fire, hits, pickups, and shop purchases.
- Add a visible defeat transition before loading `ShipShop`.
- Add clearer shop prompts and upgrade descriptions.

### Optional later work

- Add more enemy types.
- Add islands and map objectives.
- Add animations for ship movement, cannon fire, explosions, and pickups.
- Add save reset / debug menu for testing PlayerPrefs progression.
- Add balancing pass for spawn interval, enemy count, projectile speed, and upgrade costs.

## Development notes

- Prefer small pull requests.
- Avoid manually editing prefab YAML unless necessary.
- When Codex changes scripts but Unity does not reflect them, ask for direct copy-paste full-file replacements and manually edit through Visual Studio.
- Test in Play Mode after each small change.
