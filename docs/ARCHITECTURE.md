# Architecture

`Core` has no Unity references. `Simulation.Tick(dt)` owns prepare/combat/pause/result state, resources, active enemies, towers, queues, hazards and skill timers. The view consumes snapshots from its lists and never writes balance data back to ScriptableObjects. `Campaign.Create()` creates independent configurations for five stages; `Rules` owns initial balance values.

`Runtime/GuardianGame` handles the menu, HUD, mouse interaction, selection and preview, audio, JSON storage, and rendering. It caps frame catch-up at 250 ms and subdivides simulation updates into ≤1/60 s. Pausing changes simulation state instead of relying on `Time.timeScale`. GUI controls use the same scaled canvas as board input. The world input handler is restricted to the board rectangle and disabled behind modal panels.

`Editor/ProjectBuilder` restores missing editable `LevelData` assets on import, configures the three bootstrap scenes and supports a Windows build. The checked-in scenes already bind the runtime script by deterministic GUID. Resource lookups avoid texture references going missing across a fresh Unity import.

`SaveData` holds best stars per stage, unlocked stage, purchases and settings. Available stars are recomputed from best-star totals minus purchased upgrades. Runtime health, towers and skill cooldowns are never serialized as progress. `GuardianGame` saves on victory, purchase or setting changes and keeps the previous file as `.bak`. The backup is a recovery file, not automatic corruption recovery.

## Intentional implementation differences from the suggested brief architecture

- Map presentation is IMGUI textured geometry, not Tilemap. Rivers/cinematic camera choreography and production animation are pending.
- Enemy/tower runtime entities are plain C# objects, not GameObject prefabs; no physics collision or projectile pooling. Attack damage resolves immediately; particles/projectiles illustrate the attack afterward. This avoids frame-dependent projectile damage but differs from physical projectile prefabs.
- Tower/enemy balance is in `Rules`; only levels are ScriptableObjects. A later authoring pass can move these tables to `TowerData` / `EnemyData` while keeping immutable runtime copies.
- Gameplay, LevelSelect and MainMenu are thin entry scenes; a single application controller switches panels. Gameplay currently opens the menu until a stage is selected.
- The tutorial intentionally asks for the first upgrade before allowing the first combat clock to advance, preserving enough money and making the action deterministic.

At current campaign sizes, lists and direct target scans are intentionally simple. Profile on target hardware before introducing pooling/spatial indexing. `Application.targetFrameRate=60` is a target, not performance evidence.
