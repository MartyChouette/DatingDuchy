# CLAUDE.md - Project Context for DatingDuchy (CozyTown)

## Project Overview
Unity C# city-builder / dating sim hybrid. Namespace: `CozyTown`. Agents (peasants, heroes, monsters, tax collectors) roam a hex-grid town, interact with buildings, form relationships, and drive an emergent economy.

## Architecture

### Namespaces
- `CozyTown.Core` — Singletons, events, time, metrics (`GameEventBus`, `GameTime`, `MetricsLedger`, `PersonId`)
- `CozyTown.Build` — Hex grid, building placement & registry (`HexGrid`, `BuildingRegistry`, `BuildingWorldRegistry`, `BuildingInstance`, `BuildingDefinition`, `BuildingSimInstaller`)
- `CozyTown.Sim` — Agent behaviours & systems (`AgentBase`, `PeasantAgent`, `HeroAgent`, `MonsterAgent`, `TaxCollectorAgent`, `BountySystem`, `RelationshipSystem`, `PopulationSystem`)
- `CozyTown.UI` — Panels, inspectors, world-update reports

### Key Patterns
- **Event bus**: `GameEventBus.Emit(GameEvent)` — append-only stream; `MetricsLedger` subscribes to derive counters.
- **Static agent registry**: `AgentBase._registry` (Dictionary<int, AgentBase>) — O(1) lookup by PersonId, used instead of `FindObjectsOfType`. Public access via `AgentBase.TryGet(int, out AgentBase)` and `FindAgentById<T>(int)`.
- **Static building registry**: `BuildingWorldRegistry` (List + HashSet) — global; `BuildingRegistry` is the hex-aware MonoBehaviour version.
- **Domain-reload safety**: All static state cleared via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`.
- **Singleton pattern**: Most managers use `if (Instance != null && Instance != this) { Destroy(gameObject); return; }` in Awake.

### Agent Lifecycle
1. Prefab instantiated by a spawner (`KnightsGuildSpawner`, `MonsterHiveSpawner`, `PopulationSystem`).
2. `AgentBase.Awake()` — registers in static registry via `PersonId.EnsureId()`.
3. `AgentBase.TakeDamage(int)` — decrements `hp`; calls `Die()` → emits `PersonDied` → `Destroy(gameObject)`.
4. `AgentBase.OnDestroy()` — removes from registry.
5. **Important**: Unity `Destroy()` is deferred. After `TakeDamage`, check `hp <= 0` (not `== null`) for same-frame kill detection.

### Romance System
Emergent romance built into `RelationshipSystem`. Agents develop relationships through progressive stages driven by personality trait compatibility.

**Personality Traits** (`InspectablePerson`):
- 5 visible (0-10, default 5, randomized ±3 in Awake): `charm`, `kindness`, `wit`, `courage`, `warmth`
- 2 hidden (0-10, default 5, randomized ±3 in Awake): `flirtiness`, `loyalty`

**Three relationship axes** (romance-eligible pairs only — monsters excluded via `IsRomanceEligible()`):
- **Affinity** — base contact gain, modified by trait chemistry (kindness average boosts, warmth similarity boosts, wit mismatch friction)
- **Attraction** — driven by charm + flirtiness compatibility; decays during negative trend
- **Trust** — grows after 30s `skinTime` gate; loyalty boosts growth rate; erodes during negative trend

**Progressive romance stages** (each requires the previous):
- **Crush**: affinity ≥ 40, attraction ≥ 25
- **Dating**: affinity ≥ 55, attraction ≥ 40, trust ≥ 15 (registers in `_romances` tracking dict)
- **Lovers**: affinity ≥ 70, attraction ≥ 55, trust ≥ 30

**Heartbreak** — triggers when any axis drops 10 points below its gate (hysteresis prevents oscillation):
- Lovers breakup clears all romance flags; dating breakup clears dating+crush; crush just fades

**Emergent behaviors**:
- **Jealousy**: when dating starts, existing partners get +15 irritation and a jealousy highlight
- **Mourning**: on `PersonDied`, surviving partners' romance flags are cleared and mourning highlight emitted
- **Monogamy not enforced** — multiple concurrent romances allowed; jealousy emerges naturally

**Events**: All milestones emit `GameEventType.RomanceMilestone` with text = stage name (Crush/Dating/Lovers/Heartbreak/CrushFaded/Jealousy/Mourning).

**Public API**: `GetRelation(int, int)`, `HasActiveRomance(int)`, `GetRomancePartners(int)`

**UI**: `PersonClickSelector` displays trait abbreviations (CHM/KND/WIT/CRG/WRM) and active romances with partner name and stage.

### Economy Flow
Treasury ← TaxCollector collects from buildings (taxValue from BuildingDefinition) → Treasury pays bounty rewards → Heroes spend gold at Tavern → Peasants earn wages at home, spend at Market/Tavern.

### Hex Grid
- Pointy-top axial coordinates (q, r).
- `HexGrid` handles world↔hex conversion, neighbor lookup, ring/spiral iteration.
- `BuildingRegistry` tracks which hex cells are occupied; `BuildingWorldRegistry` is the flat global list.

## ScriptableObjects (Assets/ScriptableObjectScripts/)
- `Market.asset` — kind=1 (Market), taxValue=2
- `Tavern.asset` — kind=2 (Tavern), taxValue=2
- `KnightsGuild.asset` — kind=4 (KnightsGuild), spawns heroes
- `Townhall.asset` — kind=5 (TownHall)

## File Conventions
- Scripts live in `Assets/Scripts/`.
- Prefabs in `Assets/Prefabs/`.
- ScriptableObject assets in `Assets/ScriptableObjectScripts/`.
- Editor-only code guarded with `#if UNITY_EDITOR`.
- Agent kind strings use constants from `GameEvent` (e.g., `GameEvent.KindPeasant`).

## Audit History (completed)
All issues from the full codebase audit have been resolved across 6 commits:
1. `b3c2c26` — Critical & significant bug fixes (tax collection, calendar sync, static leaks, bounty expiration, mover, peasant wages, etc.)
2. `2ded763` — Removed duplicate KnightsGuild.cs / MonsterHive.cs scripts
3. `ca36e6b` — Performance fixes (agent registry, spawn caps, cached lookups, Queue buffers)
4. `cb78327` — Code smells (filename typos, magic strings→constants, key conflicts, editor stripping, BuildingRegistry.Unregister)
5. `fc5d9af` — Remaining unstaged/untracked files
6. `b8742c9` — Final audit fixes (HeroAgent bounty completion hp check, Tavern/Market asset metadata, domain-reload OnEvent clear, encoding fix, GameTime day overflow guard)

## Known Design Stubs (intentional TODOs in code)
- `courage` trait not yet used in gameplay (available on InspectablePerson but no system reads it)
- Monster damage to buildings (currently emits a Note event only)
- Distance-based bounty acceptance for heroes (bravery/greed not yet influencing decisions)
