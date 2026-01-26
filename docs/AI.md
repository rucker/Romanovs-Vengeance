# AI Difficulty & Economic Bonuses Documentation

This document explains AI difficulty settings and economic cheats in Romanov's Vengeance.

## Overview

The mod has **three AI difficulty levels**. The harder AI difficulties receive **free money** from certain structures, which is their primary economic advantage over human players.

| Difficulty | Display Name | Economic Bonus |
|------------|--------------|----------------|
| `easy` | Easy AI | None |
| `cheater` | Cheater AI | $900/min per structure |
| `vengeful` | Vengeful AI | $1,680/min per structure |

---

## AI Economic Cheats

### How It Works

AI players receive free cash through the `CashTrickler` trait, which is applied to certain structures via the `^AICashCheat` template.

**File:** `mods/rv/rules/defaults.yaml:4073-4089`

```yaml
^AICashCheat:
    GrantConditionOnBotOwner@cheater:
        Condition: enable-cheater-ai
        Bots: cheater
    GrantConditionOnBotOwner@vengeful:
        Condition: enable-vengeful-ai
        Bots: vengeful
    CashTrickler@cheater:
        Interval: 500              # Every 500 game ticks
        Amount: 300                # $300 per interval
        ShowTicks: false           # Hidden from player
        RequiresCondition: enable-cheater-ai
    CashTrickler@vengeful:
        Interval: 250              # Every 250 game ticks
        Amount: 280                # $280 per interval
        ShowTicks: false           # Hidden from player
        RequiresCondition: enable-vengeful-ai
```

### Cash Income Rates Per Structure

| Difficulty | Cash/Interval | Interval (ticks) | Cash/Second | Cash/Minute |
|------------|---------------|------------------|-------------|-------------|
| **Easy AI** | $0 | - | $0 | **$0** |
| **Cheater AI** | $300 | 500 | $15 | **$900** |
| **Vengeful AI** | $280 | 250 | $28 | **$1,680** |

*(Game runs at 25 ticks per second)*

### Structures With Cash Cheat

The `^AICashCheat` template is inherited by these structures:

| Structure | Template | File | Line |
|-----------|----------|------|------|
| Construction Yard | `^ConstructionYard` | `default-structures.yaml` | 7 |
| Refinery | `^Refinery` | `default-structures.yaml` | 392 |
| Ore Purifier (Allied) | `gaorep` | `allied-structures.yaml` | 1424 |

---

## Total AI Income Examples

### Cheater AI

| Base Size | Structures | Free Income |
|-----------|------------|-------------|
| Early game | 1 ConYard + 1 Refinery | $1,800/min |
| Mid game | 1 ConYard + 2 Refineries | $2,700/min |
| Late game | 2 ConYards + 3 Refineries | $4,500/min |
| Late game (Allied) | 2 ConYards + 3 Refineries + Ore Purifier | $5,400/min |

### Vengeful AI

| Base Size | Structures | Free Income |
|-----------|------------|-------------|
| Early game | 1 ConYard + 1 Refinery | $3,360/min |
| Mid game | 1 ConYard + 2 Refineries | $5,040/min |
| Late game | 2 ConYards + 4 Refineries | $10,080/min |
| Late game (Allied) | 2 ConYards + 4 Refineries + Ore Purifier | $11,760/min |

### Comparison to Harvesting

For reference, a single harvester earns approximately:
- ~$2,000-3,000/min on a good ore field
- ~$1,000-1,500/min on a depleted field

A Vengeful AI with 3 structures gets the equivalent of **2-3 free harvesters**.

---

## AI Bot Definitions

**File:** `mods/rv/rules/ai.yaml:1-22`

```yaml
Player:
    ModularBot@easyai:
        Name: bot-easy.name
        Type: easy
    ModularBot@cheater:
        Name: bot-cheater.name
        Type: cheater
    ModularBot@vengefulai:
        Name: bot-vengeful.name
        Type: vengeful
    GrantConditionOnBotOwner@easy:
        Condition: enable-easy-ai
        Bots: easy
    GrantConditionOnBotOwner@cheater:
        Condition: enable-cheater-ai
        Bots: cheater
    GrantConditionOnBotOwner@vengeful:
        Condition: enable-vengeful-ai
        Bots: vengeful
    GrantConditionOnBotOwner@generic:
        Condition: enable-generic-ai
        Bots: easy, cheater, vengeful
```

---

## Other AI Differences by Difficulty

### Base Building Behavior

| Setting | Easy | Cheater | Vengeful |
|---------|------|---------|----------|
| MinimumExcessPower | 60 | 70 | 100 |
| MaximumExcessPower | 160 | 770 | 800 |
| ExcessPowerIncrement | 40 | 50 | 50 |
| Builds Super Weapons | No | Yes (limit 2) | Yes (limit 2) |
| Tower Rush Defenses | No | Yes | Yes |

### MCV Management

| Setting | Easy | Cheater | Vengeful |
|---------|------|---------|----------|
| MinimumConstructionYardCount | 2 | 2 | 4 |

### Summary

| Aspect | Easy | Cheater | Vengeful |
|--------|------|---------|----------|
| Economic bonus | None | Moderate | High |
| Super weapons | No | Yes | Yes |
| Expansion | Minimal | Moderate | Aggressive |
| Difficulty | Beginner | Intermediate | Expert |

---

## Options to Adjust AI Economy

### Option 1: Reduce Cash Amounts

Lower the `Amount` values to reduce AI income.

```yaml
CashTrickler@cheater:
    Interval: 500
    Amount: 150                # Reduced from 300 ($450/min per structure)
    RequiresCondition: enable-cheater-ai
CashTrickler@vengeful:
    Interval: 250
    Amount: 140                # Reduced from 280 ($840/min per structure)
    RequiresCondition: enable-vengeful-ai
```

---

### Option 2: Increase Intervals

Make cash come in slower by increasing `Interval`.

```yaml
CashTrickler@cheater:
    Interval: 750              # Increased from 500 ($600/min per structure)
    Amount: 300
    RequiresCondition: enable-cheater-ai
CashTrickler@vengeful:
    Interval: 500              # Increased from 250 ($900/min per structure)
    Amount: 280
    RequiresCondition: enable-vengeful-ai
```

---

### Option 3: Remove From Refineries

Only give bonus on Construction Yard, not refineries. This limits the scaling.

**File:** `mods/rv/rules/default-structures.yaml`

```yaml
^Refinery:
    # Remove this line:
    # Inherits@AICash: ^AICashCheat
```

**Result:** AI gets bonus only from ConYards (max 1-2 structures instead of 5-7).

---

### Option 4: Add Easy AI Bonus

Give Easy AI a small bonus so it's not completely helpless.

```yaml
^AICashCheat:
    GrantConditionOnBotOwner@easy:
        Condition: enable-easy-ai
        Bots: easy
    CashTrickler@easy:
        Interval: 750
        Amount: 100                # $200/min per structure
        ShowTicks: false
        RequiresCondition: enable-easy-ai
```

---

### Option 5: Create New "Normal" Difficulty

Add a fourth AI between Easy and Cheater.

**File:** `mods/rv/rules/ai.yaml`

```yaml
Player:
    ModularBot@normalai:
        Name: bot-normal.name
        Type: normal
    GrantConditionOnBotOwner@normal:
        Condition: enable-normal-ai
        Bots: normal
```

**File:** `mods/rv/rules/defaults.yaml`

```yaml
^AICashCheat:
    GrantConditionOnBotOwner@normal:
        Condition: enable-normal-ai
        Bots: normal
    CashTrickler@normal:
        Interval: 625
        Amount: 200                # $480/min per structure
        ShowTicks: false
        RequiresCondition: enable-normal-ai
```

---

### Option 6: Make Bonus Visible

Let players see when AI is receiving free money (for transparency/debugging).

```yaml
CashTrickler@cheater:
    Interval: 500
    Amount: 300
    ShowTicks: true            # Changed from false
    RequiresCondition: enable-cheater-ai
```

---

## Technical Reference

### Key Files

| File | Description |
|------|-------------|
| `mods/rv/rules/ai.yaml` | Bot definitions and support power decisions |
| `mods/rv/rules/defaults.yaml:4073-4089` | `^AICashCheat` template |
| `mods/rv/rules/default-structures.yaml` | Structure templates inheriting `^AICashCheat` |
| `mods/rv/fluent/rules.ftl:4218-4225` | Bot display names |

### Relevant Traits

| Trait | Purpose |
|-------|---------|
| `ModularBot` | Defines a bot type with name and internal type ID |
| `GrantConditionOnBotOwner` | Grants a condition when owned by specific bot types |
| `CashTrickler` | Periodically adds cash to owner's resources |
| `BaseBuilderBotModule` | Controls AI base building behavior |
| `McvManagerBotModule` | Controls AI MCV/expansion behavior |
| `SupportPowerBotASModule` | Controls AI support power usage |

### CashTrickler Trait Properties

| Property | Type | Description |
|----------|------|-------------|
| `Interval` | int | Ticks between cash grants (25 ticks = 1 second) |
| `Amount` | int | Cash amount per interval |
| `ShowTicks` | bool | Whether to show floating cash numbers |
| `RequiresCondition` | string | Condition that must be active |

---

## Tick Rate Reference

- **1 second** = 25 game ticks
- **1 minute** = 1,500 game ticks
- Interval 500 = every 20 seconds
- Interval 250 = every 10 seconds
