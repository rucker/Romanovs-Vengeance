# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Romanov's Vengeance (mod id `rv`) is a third-party [OpenRA](https://www.openra.net) mod built on the OpenRA Mod SDK, derived from the OpenRA Red Alert 2 mod. It aims at a balanced multiplayer Red Alert 2 / Yuri's Revenge experience. Most gameplay work is data (MiniYAML rules and weapons); C# is only for engine extensions that the data cannot express.

## Engine dependency

- The engine is **not** upstream OpenRA. It is the fork `jimicze/OpenRA` (itself a fork of `MustaphaTR/OpenRA`, carrying extra performance and crash fixes), pinned by commit in `mod.config` (`ENGINE_VERSION`). That fork bundles `OpenRA.Mods.AS` (Attacque Supérior), which the mod uses heavily alongside `Common`, `Cnc` and `D2k`.
- `make` runs `fetch-engine.sh`, which downloads that commit into `./engine` (git-ignored) and builds it. Treat `./engine` as read-only, because it gets re-fetched whenever `ENGINE_VERSION` changes. Engine bumps are done by editing `ENGINE_VERSION` (see commits like "Engine update.").
- Before adding a C# trait, look for an existing one in `engine/OpenRA.Mods.AS` or `engine/OpenRA.Mods.Common`. Trait names used in YAML resolve across every assembly listed under `Assemblies:` in `mods/rv/mod.yaml`.
- `mod.config` must keep LF line endings. The `*.sh` scripts must stay executable; CI checks both.

## Commands

Requires Python and a .NET SDK able to build the engine's `net8.0` target (newer SDKs work). Running the game/utility needs the .NET 8 runtime, or `DOTNET_ROLL_FORWARD=Major` if only a newer runtime is installed (put `export DOTNET_ROLL_FORWARD=Major` in the git-ignored `user.config`; `launch-game.sh`/`utility.sh` source it, so `make test` picks it up). CI still pins 6.0.x, which is stale. `RUNTIME=mono` still exists in the Makefile but is no longer tested in CI.

```sh
make                    # fetch + build engine, then build OpenRA.Mods.RA2 (Release)
make check              # Debug build + StyleCop + explicit-interface / conditional-trait override checks
make test               # validate mod MiniYAML (./utility.sh --check-yaml): catches bad trait/field names, missing refs
make check-scripts      # luac -p on map Lua scripts (needs lua5.1 / luac)
make version VERSION=x  # stamp version into mods/rv/mod.yaml
./launch-game.sh        # run the game; extra args are passed as OpenRA settings, e.g. Launch.Map=<uid>
./utility.sh <command>  # OpenRA.Utility for the rv mod (e.g. --check-yaml, --docs, --lua-docs)
```

On Windows, use `make.cmd` / `make.ps1` with the same targets. There are no unit tests: `make check` + `make test` (what CI runs) is the verification loop. After any YAML edit, run `make test`.

## Layout and architecture

- `OpenRA.Mods.RA2/`: the single C# assembly (`OpenRA.Mods.RA2.dll`), referencing the engine projects via `../engine`. It holds traits, activities, warheads, widgets/logic, the `.bag` audio filesystem, and the `ImportRA2MapCommand` utility command. Classes suffixed `Old` (e.g. `InfectorOld`, `BallisticMissileOld`, `MissileSpawnerOld*`) are legacy variants kept alongside newer engine/AS equivalents. Check which one the YAML actually references before changing them. Code follows OpenRA engine style (tabs, GPL header, StyleCop rules from `engine/` + `.editorconfig`), and `[Desc]` attributes on trait info fields feed the generated docs.
- `mods/rv/mod.yaml`: the manifest, which is the source of truth for what gets loaded. Every rules, sequences, weapons, chrome, tileset and fluent file must be listed here or the engine ignores it. It also defines the filesystem: original RA2/YR `.mix` content is mounted from the user's support dir (`content`), with YR mounted before RA2 so YR art wins, then RV's own assets under `mods/rv/bits`.
- `mods/rv/rules/*.yaml`: actor definitions, split by faction (allied / soviet / yuri / bakuvian / civilian) and by category (infantry, vehicles, naval, structures). Shared abstract templates (`^Name` actors) live in `defaults.yaml`, `default-*.yaml` and `misc.yaml`, and concrete actors inherit them with `Inherits:`. Removing a trait from an inherited template uses `-TraitName:`. Gameplay heavily uses the conditions system (`GrantConditionOn*`, `RequiresCondition`) and prerequisites. `world.yaml` and `player.yaml` hold the World/Player actors (lobby options, factions, AI in `ai.yaml`).
- `mods/rv/weapons/*.yaml`: weapons and warheads. `mods/rv/sequences/*.yaml`: sprite sequences (`voxels.yaml` is for voxel models).
- `mods/rv/chrome/lobby.yaml` and `lobby-players.yaml` are RV copies of engine `common|chrome` layouts (referenced as `rv|chrome/...` in `mod.yaml`). Engine updates to the originals don't reach RV automatically, so re-check them when bumping `ENGINE_VERSION`.
- `mods/rv/fluent/*.ftl`: all player-visible strings (names, descriptions, UI). YAML refers to message keys, so text changes like unit descriptions go in `rules.ftl`, not in the YAML.
- `mods/rv/scripts/*.lua`: global Lua game-mode scripts (regicide, KOTH, domination, commander's powers), wired up via `LuaScript` on the World actor in `rules/world.yaml`. Map-specific Lua lives inside map folders.
- `mods/rv/maps/`: packaged `.oramap` files plus unpacked shellmaps. Map-specific rule overrides live inside each map.
- `mods/rv-content/`: content-installer mod that prompts users for and extracts the original RA2/YR game files.
- `packaging/`, `.github/workflows/packaging.yml`: installer builds (AppImage, macOS dmg, Windows NSIS) triggered by git tags. `artsrc/` holds PSD/PNG source art and is not loaded by the game.
