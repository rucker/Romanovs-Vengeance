# Harvesting & Resource Collection Documentation

This document explains harvesting mechanics in Romanov's Vengeance, with focus on the Slave Miner system and storage behavior.

## Overview

Resources (Ore, Gems) are collected by harvesters and delivered to refineries. The game uses a **global storage pool** - all silos and refineries contribute to the player's total storage capacity.

| Resource | Value | Color |
|----------|-------|-------|
| Ore | 25 per unit | Yellow |
| Gems | 50 per unit | Blue |

---

## Harvester Types

### Standard Harvester (`harv`)

- Drives to ore fields, collects resources
- Returns to nearest refinery to unload
- Storage is handled by the refinery building

### War Miner (`wmin` - Soviets)

- Armed harvester with machine gun
- Same behavior as standard harvester
- Returns to refinery to unload

### Slave Miner (`smin`/`yarefn` - Yuri)

- Mobile refinery that deploys
- Spawns slave units to harvest nearby ore
- **Self-contained** - does not need a separate refinery
- Has its own storage capacity (1000)

---

## Slave Miner System

The Slave Miner is a unique harvesting system with three components:

### Components

| Actor ID | Name | Type | Description |
|----------|------|------|-------------|
| `smin` | Slave Miner | Vehicle | Mobile form, can attack, transforms into deployed form |
| `yarefn` | Slave Miner (Deployed) | Structure | Deployed form, acts as refinery, spawns slaves |
| `slav` | Slave | Infantry | Harvests ore and returns it to the deployed miner |

### Resource Flow

```
[Ore Field] --> [Slave harvests ore] --> [Slave returns to yarefn] --> [Refinery processes] --> [Player Resources]
     ^                   |                        |                            |
     |                   v                        v                            v
     |            Capacity: 4 ore          DockHost accepts           UseStorage: true
     |                                      slave docking              (uses global pool)
     +------------------------------------------+
                    (slave returns to harvest)
```

### Configuration

**Slave Miner Vehicle (`smin`)** - `mods/rv/rules/yuri-vehicles.yaml:23`
```yaml
smin:
    Transforms:
        IntoActor: yarefn           # Deploys into refinery form
    StoresPlayerResources:
        Capacity: 1000              # Contributes to global storage
```

**Deployed Slave Miner (`yarefn`)** - `mods/rv/rules/yuri-structures.yaml:362`
```yaml
yarefn:
    Refinery:
        UseStorage: true            # Uses global storage pool
        ShowTicks: True
        TickRate: 10
    DockHost:
        Type: SlaveUnload           # Accepts slaves for unloading
    StoresPlayerResources:
        Capacity: 1000              # Contributes to global storage
```

**Slave (`slav`)** - `mods/rv/rules/yuri-infantry.yaml:1`
```yaml
slav:
    Harvester:
        Type: SlaveUnload           # Docks at SlaveUnload hosts
        BaleLoadDelay: 24
        BaleUnloadDelay: 1
        Resources: Ore, Gems
    StoresResources:
        Capacity: 4                 # Holds 4 ore units before returning
    DockClientManager:
        OccupancyCostModifier: 4
```

---

## Known Issue: Slave Miner Stops Earning When "Full"

### Symptoms

1. Slave Miner appears "full" (all pips filled on the building)
2. Slaves continue to harvest but don't seem to earn money
3. Slaves return to the miner but immediately go back to harvesting
4. No floating cash numbers appear

### Root Cause

The Slave Miner uses `Refinery` with `UseStorage: true`. When the **player's global resource storage capacity is full**, the refinery refuses to accept any resources.

**Code flow in `Refinery.cs:58-97`:**

```csharp
int IAcceptResources.AcceptResources(Actor self, string resourceType, int count)
{
    if (info.UseStorage)
    {
        var storageLimit = Math.Max(playerResources.ResourceCapacity - playerResources.Resources, 0);
        if (!info.DiscardExcessResources)
        {
            // Reduce amount until it fits available storage
            while (value > storageLimit)
                value = Util.ApplyPercentageModifiers(--count * resourceValue, resourceValueModifiers);
        }
        // ...
    }
    return count;  // Returns 0 if storage is full!
}
```

When storage is full:
1. `storageLimit` = 0
2. The loop reduces `count` to 0
3. `AcceptResources` returns 0 (nothing accepted)
4. Slave's ore is NOT removed from inventory
5. Slave appears stuck, keeps trying to deliver

### Visual Indicator

The Slave Miner displays 15 pips showing storage status. When all pips are filled, the **global storage is at capacity**.

---

## Fix Options

### Option 1: Discard Excess Resources (Recommended)

Keep harvesting continuously, but excess ore is lost when storage is full.

**File:** `mods/rv/rules/yuri-structures.yaml`

```yaml
yarefn:
    Refinery:
        UseStorage: true
        DiscardExcessResources: true    # ADD THIS LINE
        ShowTicks: True
        TickRate: 10
```

| Pros | Cons |
|------|------|
| Slaves never stop harvesting | Ore is wasted when full |
| No gameplay interruption | May encourage building silos |
| Consistent behavior | Differs from original RA2 |

---

### Option 2: Convert Directly to Cash

Bypass storage entirely - resources convert directly to cash (unlimited).

**File:** `mods/rv/rules/yuri-structures.yaml`

```yaml
yarefn:
    Refinery:
        UseStorage: false               # CHANGE THIS
        ShowTicks: True
        TickRate: 10
```

| Pros | Cons |
|------|------|
| Never stops earning | Storage capacity becomes meaningless |
| Simpler mechanic | May be overpowered |
| No need for silos | Inconsistent with other harvesters |

---

### Option 3: Keep Current Behavior (Stop When Full)

Slaves stop delivering when storage is full. Player must build silos or spend resources.

**Current configuration (no change needed):**

```yaml
yarefn:
    Refinery:
        UseStorage: true
        # DiscardExcessResources defaults to false
```

| Pros | Cons |
|------|------|
| Encourages strategic silo building | Can feel like harvesting is "broken" |
| Resources are never wasted | Poor feedback to player |
| Original RA2-like behavior | Slaves appear idle/stuck |

---

### Option 4: Increase Storage Capacity

Make Slave Miner provide more storage to delay the "full" problem.

**File:** `mods/rv/rules/yuri-structures.yaml`

```yaml
yarefn:
    StoresPlayerResources:
        Capacity: 2000                  # Increase from 1000
```

| Pros | Cons |
|------|------|
| Delays the problem | Doesn't fix root cause |
| Slave Miner becomes more valuable | Still stops eventually |

---

## Storage Capacity Reference

| Building | Storage | Faction | Notes |
|----------|---------|---------|-------|
| Standard Refinery | 2000 | All | Main resource processing |
| Slave Miner (deployed) | 1000 | Yuri | Also a refinery |
| Slave Miner (mobile) | 1000 | Yuri | Transfers to deployed form |
| Ore Silo | 1500 | All | Storage only |

**Example:** With 1 Slave Miner (1000) and 1 Silo (1500), total capacity is 2500.

---

## Technical Reference

### Key Files

| File | Description |
|------|-------------|
| `mods/rv/rules/yuri-vehicles.yaml:23-120` | Slave Miner vehicle definition |
| `mods/rv/rules/yuri-structures.yaml:362-579` | Deployed Slave Miner definition |
| `mods/rv/rules/yuri-infantry.yaml:1-100` | Slave unit definition |
| `engine/OpenRA.Mods.Common/Traits/Buildings/Refinery.cs` | Refinery trait (AcceptResources logic) |
| `engine/OpenRA.Mods.Common/Traits/Harvester.cs` | Harvester trait (dock/unload logic) |

### Relevant Traits

| Trait | Purpose |
|-------|---------|
| `Refinery` | Accepts resources from harvesters, converts to cash/storage |
| `DockHost` | Allows units to dock (for unloading) |
| `Harvester` | Enables resource collection and delivery |
| `StoresResources` | Unit-level resource storage (slave holds ore) |
| `StoresPlayerResources` | Building contributes to player's global storage |
| `DockClientManager` | Allows unit to dock at DockHosts |

### Refinery Trait Properties

| Property | Default | Description |
|----------|---------|-------------|
| `UseStorage` | `true` | Store in silos vs add directly to cash |
| `DiscardExcessResources` | `false` | Continue accepting when full (lose excess) |
| `ShowTicks` | `true` | Show floating cash numbers |
| `TickRate` | `10` | How often to show cash ticks |

---

## Distance Units

OpenRA uses a custom distance system:
- `1c0` = 1 cell = 1024 internal units
- Slave `StoresResources.Capacity: 4` = 4 ore units (not distance)
