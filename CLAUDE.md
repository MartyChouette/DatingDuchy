# CLAUDE.md - Project Context for DatingDuchy (CozyTown)

## Project Overview
CozyTown is a cozy city builder + dating sim hybrid: you place and grow a town on a hex grid, manage a light economy, and build relationships with townsfolk. On top of the cozy sim layer, there is a monster + bounty loop that gives the town an outward-facing "problem solving" rhythm and a reason to expand, specialize, and socialize.

Inspirations (mechanical vibe, not direct copies): Majesty-style "indirect town management pressure" meets Stardew Valley-style relationship loops, with its own town simulation twist.

## Design Pillars

1. **Cozy simulation with readable cause and effect** — Players should feel like their placements and choices matter quickly and visibly.
2. **Relationships are gameplay, not garnish** — Dating, friendship, and reputation are systems that affect access, efficiency, unlocks, and story.
3. **Light strategy pressure from the world** — Monsters and bounties create stakes and pacing without turning the game into pure combat.
4. **Always feedback forward** — The player should constantly get small confirmations of "what changed" via UI highlights, popups, and world reactions.

## Core Gameplay Loop

1. Place or upgrade buildings on the grid to shape production and town services.
2. Town sim advances (time, schedules, needs, events).
3. Interact with townsfolk (talk, gift, help, invite, date) to push relationship arcs.
4. Respond to problems (monster activity, needs spikes, shortages).
5. Post or pursue bounties for rewards, safety, and story progression.
6. Town changes (new NPC behaviors, unlocked buildings, new regions, better economy).
7. Repeat with new constraints, new people, and new town identity.

## System Map

### A) Town Grid and Building Placement [BUILT]
- Pointy-top axial hex coordinates (q, r).
- `HexGridManager` handles world<->hex conversion, neighbor lookup, ring/spiral iteration.
- `BuildingDefinition` (ScriptableObject) defines cost, size, outputs, services.
- `BuildingInstance` spawns at runtime with HP, tax value, footprint.
- `BuildingRegistry` (hex-aware) tracks occupied cells, validates placement, handles rotation.
- `BuildingWorldRegistry` is the flat global list for fast nearest-building lookups.
- `BuildToolController` manages placement mode; `BuildMenuController`/`BuildMenuButton`/`BuildMenuDragSource` handle UI.

Common building categories (planned):
- Production (materials, food, crafting) — NOT YET IMPLEMENTED
- Services (tavern, clinic, jobs board, community board) — Tavern exists
- Social (date locations, hangouts, event venues) — NOT YET IMPLEMENTED
- Defense / scouting (watch posts, wards, hunters guild) — KnightsGuild exists

### B) Resources and Economy [PARTIAL]
- **Gold** is the only currency. No materials or multi-resource economy yet.
- Treasury <- TaxCollector collects from buildings (taxValue from BuildingDefinition) -> Treasury pays bounty rewards -> Heroes spend gold at Tavern -> Peasants earn wages at home, spend at Market/Tavern.
- Economy is readable: place a Market, peasants spend there; place a KnightsGuild, heroes spawn and hunt.
- Designed to expand: "I placed X, now Y improves."

### C) Town Sim Tick and World Events [BUILT]
- `GameTime` manages day/year tracking with speed control and day phase enum.
- `GameCalendar` emits festival events on days 60, 120, and new year.
- `GameEventBus.Emit(GameEvent)` — append-only event stream; all systems publish into it.
- `MetricsLedger` subscribes to derive live counters (treasury, population, bounties, buildings destroyed).
- `WorldUpdateSystem` pops reports on festival days; `WorldUpdateReportBuilder` assembles text.
- `LedgerOverlayUI` (L key) shows live stats; `PersonClickSelector` inspects agents on click.

### D) Townsfolk AI and Relationships [PARTIAL]
NPCs have:
- **Simple state machines** — PeasantAgent cycles House->Market->Tavern; HeroAgent patrols tavern then hunts bounties; TaxCollectorAgent routes through buildings collecting taxes.
- **Personality traits** (`InspectablePerson`) — 5 visible (charm, kindness, wit, courage, warmth) + 2 hidden (flirtiness, loyalty), all 0-10, randomized +/-3 in Awake.
- **Relationship stats** — affinity, attraction, trust, irritation, tracked per pair in `RelationshipSystem`.
- **Romance milestones** — Crush, Dating, Lovers (progressive). Jealousy, Mourning, Heartbreak emerge naturally.
- **Time-of-day schedules** — Agents check `GameTime.CurrentPhase` to vary behavior:
  - PeasantAgent: Daytime market-heavy, evening tavern-heavy, late night go home.
  - HeroAgent: Sleeps at tavern during night (Midnight/LateNight/EarlyMorning), hunts bounties during day.
  - TaxCollectorAgent: Only collects taxes during daytime (Morning-LateAfternoon).

NOT YET IMPLEMENTED:
- Preferences (likes/dislikes for places, gifts, activities)
- Triggers (quests, rivalries beyond jealousy)

### E) Dating Loop / VN Layer [NOT YET IMPLEMENTED]
- Dating scenes triggered by relationship thresholds, invitations, or location-based moments.
- Dates should feed back into simulation: better prices, new building perks, special bounties, social shortcuts.
- No dialogue system, dating events, or visual-novel-style interactions exist yet.

### F) Monsters and Bounty System [BUILT]
- `MonsterAgent` targets buildings (TownHall > House > Market), deals real damage via `TakeDamage()`.
- Buildings have HP (default 50, 0 = indestructible). Destroyed buildings unregister hex cells and emit events.
- `BountySystem` posts/accepts/completes bounties with expiration tracking.
- `BountyPlacer` lets the player raycast to place bounties on monsters under the cursor.
- `BountyFlagVisual` spawns over targeted monsters as feedback.
- Heroes evaluate bounties using bravery/greed scoring (not first-come-first-served).
- Completing bounties pays out resources and updates metrics.

### G) UI and Feedback Systems [PARTIAL]
- `WorldUpdatePanel` — festival/new-year reports with treasury, population, bounty, and relationship highlights.
- `LedgerOverlayUI` — live stats overlay (L key).
- `PersonClickSelector` — click agents to see traits, romance partners, HP, gold.
- `WorldUpdateReportBuilder` — assembles text reports from system data, pulls highlights from `TownLog`.
- `TownLog` (singleton) — unified highlight log that collects entries from all systems (romance, social, economy, combat, building). Auto-subscribes to `GameEventBus` for combat/building events; `RelationshipSystem` pushes romance/social entries directly.

## Data Model

### Definitions (static data)
- `BuildingDefinition` (ScriptableObject) — size, footprint, kind, taxValue, maxHP, jobSlots, housingBeds, spawner hooks, beauty, utility. [EXISTS]
- `NPCDef` (base traits, preferences pools, schedule templates) — NOT YET IMPLEMENTED
- `ItemDef` (gifts, resources, quest items) — NOT YET IMPLEMENTED
- `MonsterDef` (threat level, biome, behaviors, bounty value) — NOT YET IMPLEMENTED
- `BountyDef` (reward rules, expiration rules, modifiers) — NOT YET IMPLEMENTED

### State (save data)
- Town layout, resources, NPC relationships, active bounties, world threat — all runtime only, NO SAVE/LOAD YET.

## Technical Architecture

### Namespaces
- `CozyTown.Core` — Singletons, events, time, metrics (`GameEventBus`, `GameTime`, `MetricsLedger`, `PersonId`)
- `CozyTown.Build` — Hex grid, building placement & registry (`HexGrid`, `BuildingRegistry`, `BuildingWorldRegistry`, `BuildingInstance`, `BuildingDefinition`, `BuildingSimInstaller`)
- `CozyTown.Sim` — Agent behaviours & systems (`AgentBase`, `PeasantAgent`, `HeroAgent`, `MonsterAgent`, `TaxCollectorAgent`, `BountySystem`, `RelationshipSystem`, `PopulationSystem`)
- `CozyTown.UI` — Panels, inspectors, world-update reports

### Key Patterns
- **Event bus**: `GameEventBus.Emit(GameEvent)` — append-only stream; `MetricsLedger` subscribes to derive counters.
- **Static agent registry**: `AgentBase._registry` (Dictionary<int, AgentBase>) — O(1) lookup by PersonId. Public access via `AgentBase.TryGet(int, out AgentBase)` and `FindAgentById<T>(int)`.
- **Static building registry**: `BuildingWorldRegistry` (List + HashSet) — global; `BuildingRegistry` is the hex-aware MonoBehaviour version.
- **Domain-reload safety**: All static state cleared via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`.
- **Singleton pattern**: Most managers use `if (Instance != null && Instance != this) { Destroy(gameObject); return; }` in Awake.
- **Same-frame kill detection**: After `TakeDamage`, check `hp <= 0` (not `== null`) because Unity `Destroy()` is deferred.

### Agent Lifecycle
1. Prefab instantiated by a spawner (`KnightsGuildSpawner`, `MonsterHiveSpawner`, `PopulationSystem`).
2. `AgentBase.Awake()` — registers in static registry via `PersonId.EnsureId()`.
3. `AgentBase.TakeDamage(int)` — decrements `hp`; calls `Die()` -> emits `PersonDied` -> `Destroy(gameObject)`.
4. `AgentBase.OnDestroy()` — removes from registry.

### Romance System Details
Three relationship axes (romance-eligible pairs only — monsters excluded via `IsRomanceEligible()`):
- **Affinity** — base contact gain, modified by trait chemistry (kindness avg boosts 0.06f, warmth similarity boosts 0.04f, wit mismatch friction 0.03f, courage avg boosts 0.04f)
- **Attraction** — driven by charm + flirtiness compatibility; decays during negative trend
- **Trust** — grows after 30s `skinTime` gate; loyalty boosts growth rate; erodes during negative trend

Progressive romance stages (each requires the previous):
- **Crush**: affinity >= 40, attraction >= 25
- **Dating**: affinity >= 55, attraction >= 40, trust >= 15 (registers in `_romances` tracking dict)
- **Lovers**: affinity >= 70, attraction >= 55, trust >= 30

Heartbreak triggers when any axis drops 10 points below its gate (hysteresis prevents oscillation).

Emergent behaviors: Jealousy (+15 irritation to existing partners), Mourning (on death), multiple concurrent romances allowed.

Events: All milestones emit `GameEventType.RomanceMilestone` with text = stage name.

Public API: `GetRelation(int, int)`, `HasActiveRomance(int)`, `GetRomancePartners(int)`

### Bounty Scoring System
Heroes evaluate bounties using bravery and greed (both 0-10, randomized +/-2):
- `rewardScore = reward * (1 + greed * 0.1)` — greed 0 = 1x, greed 10 = 2x
- `distancePenalty = (distance / distanceNormalizer) * (1 - bravery * 0.08)` — bravery 0 = full, bravery 10 = 0.2x
- `score = rewardScore - distancePenalty` — highest positive wins; negative = "not worth it"
- Bounty check interval: `0.7 - greed * 0.04` seconds (greedier heroes poll faster)

### Courage Trait
Courage (`InspectablePerson.courage`, 0-10) affects two systems:
- **Relationship affinity**: coefficient 0.04f, centered at 5 (weaker than kindness at 0.06f)
- **Hero combat damage**: bonus = `floor(courage * 0.2)`, base damage 3 -> effective range 3-5

### Building Damage System
- `BuildingDefinition.maxHP` (default 50, 0 = indestructible).
- `BuildingInstance.TakeDamage(int)` decrements hp, emits `BuildingDamaged`, calls `DestroyBuilding()` at 0.
- `DestroyBuilding()` emits `BuildingDestroyed`, unregisters from hex registry, recalculates neighbor adjacency bonuses, destroys GameObject.
- `BuildingRegistry.Place()` sets `inst.Registry = this` for hex cleanup on destruction.
- `MetricsLedger.buildingsDestroyed` tracks destructions.

### Adjacency Bonus System
- `BuildingDefinition.beauty` and `BuildingDefinition.utility` are float fields on ScriptableObjects.
- On placement and destruction, `BuildingInstance.RecalculateAdjacencyBonuses()` sums beauty/utility from hex-adjacent buildings.
- `beautyBonus` increases effective `taxValue` by `floor(beautyBonus * 0.5)`.
- `utilityBonus` increases `maxHP` by `floor(utilityBonus * 2)` (base HP rebuilt from definition, hp capped at new max).
- `BuildingRegistry.GetAdjacentBuildings(inst)` returns distinct neighboring BuildingInstances across all footprint cells.
- Recalculation runs for the placed/destroyed building AND all its neighbors.
- Notable bonuses are logged to `TownLog` with `LogCategory.Building`.

### Town Log System
- `TownLog` (singleton, namespace `CozyTown.Core`) — unified append-only highlight log.
- `LogCategory` enum: Romance, Social, Economy, Combat, Building.
- `LogEntry` struct: time, category, text.
- Auto-generates entries from `GameEventBus` for: BountyPosted, BountyCompleted, MonsterKilled, BuildingDestroyed, PersonDied.
- `RelationshipSystem` pushes entries directly via `TownLog.Instance?.Push()` for romance/social milestones.
- `WorldUpdateReportBuilder` calls `TownLog.Instance.AppendRecent(sb, count)` for festival reports.
- Domain-reload safe via `[RuntimeInitializeOnLoadMethod]`.
- Max 200 entries (configurable via `maxEntries`).
- Public API: `Push(LogCategory, string)`, `AppendRecent(StringBuilder, int)`, `Entries` (IReadOnlyList).

### NPC Schedule System
- `GameTime` provides convenience properties: `IsDaytime` (Morning-LateAfternoon), `IsEvening` (Night phase), `IsNight` (Midnight, LateNight, EarlyMorning).
- **PeasantAgent**: `ChooseNext()` uses phase-dependent weights:
  - Daytime: Market 50%, Tavern 15%, Home 35%
  - Evening: Market 10%, Tavern 60%, Home 30%
  - Night: Tavern 5%, Home 95%
- **HeroAgent**: Skips bounty checks during `IsNight` (sleeps at tavern). Fighting/celebrating states unaffected.
- **TaxCollectorAgent**: Entire Update() skipped when `!IsDaytime` (no collection at night/evening).

## File Conventions
- Scripts live in `Assets/Scripts/`.
- Prefabs in `Assets/Prefabs/`.
- ScriptableObject assets in `Assets/ScriptableObjectScripts/`.
- Editor-only code guarded with `#if UNITY_EDITOR`.
- Agent kind strings use constants from `GameEvent` (e.g., `GameEvent.KindPeasant`).

## ScriptableObjects (Assets/ScriptableObjectScripts/)
- `Market.asset` — kind=1 (Market), taxValue=2
- `Tavern.asset` — kind=2 (Tavern), taxValue=2
- `KnightsGuild.asset` — kind=4 (KnightsGuild), spawns heroes
- `Townhall.asset` — kind=5 (TownHall)

## Implementation Status

### Complete
- Hex grid with placement, rotation, footprint validation
- Building system with HP, damage, destruction, hex cleanup
- Agent framework (registry, lifecycle, movement, combat)
- 4 agent types (Peasant, Hero, Monster, TaxCollector)
- Romance system (5 trait-driven axes, 3 progressive stages, jealousy, mourning, heartbreak)
- Bounty system (post/accept/complete/expire, bravery/greed scoring, player-placed via raycast)
- Economy loop (treasury, taxes, bounty rewards, tavern spending)
- Time system (day/year, phases, speed control, festivals)
- Population system (auto-spawn peasants for jobs, auto-build houses)
- UI overlays (ledger, agent inspector, world update reports)
- Event bus architecture
- Unified town log (`TownLog` singleton — collects highlights from all systems)
- NPC schedules (time-of-day gating for peasant, hero, tax collector)
- Adjacency bonuses (beauty boosts tax value, utility boosts HP for neighboring buildings)

### Not Yet Implemented
- Items, inventory, and gift-giving
- Dialogue and dating scenes (VN layer)
- Building upgrades and progression chains
- Feature/building unlock conditions
- Multi-resource economy (materials, food beyond gold)
- Building production/consumption per tick
- World threat escalation
- NPC preferences (likes/dislikes)
- Save/load persistence
- ScriptableObject definitions for NPCs, items, monsters, bounties

## Audit History (completed)
All issues from the full codebase audit have been resolved across 6 commits:
1. `b3c2c26` — Critical & significant bug fixes (tax collection, calendar sync, static leaks, bounty expiration, mover, peasant wages, etc.)
2. `2ded763` — Removed duplicate KnightsGuild.cs / MonsterHive.cs scripts
3. `ca36e6b` — Performance fixes (agent registry, spawn caps, cached lookups, Queue buffers)
4. `cb78327` — Code smells (filename typos, magic strings->constants, key conflicts, editor stripping, BuildingRegistry.Unregister)
5. `fc5d9af` — Remaining unstaged/untracked files
6. `b8742c9` — Final audit fixes (HeroAgent bounty completion hp check, Tavern/Market asset metadata, domain-reload OnEvent clear, encoding fix, GameTime day overflow guard)
