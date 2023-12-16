# Castle Heart Police

⚠️WIP⚠️

Restricts how many / where castle hearts can be placed, on a per-clan basis.

This is a server-side mod.


## The system

Definitions
- let MaxCastleHeartScorePerClan be a number, specified by the configuration of the server
- let CastleHeartScoreStrategy be a selectable strategy, specified by the configuration of the server
  - e.g. EveryHeartWorthOnePoint
  - e.g. CustomTerritoryScores (with the value of each territory configured in a json file)
- let the value of any castle heart be determined by the selected CastleHeartScoreStrategy

Rules
- castle heart placement:
  - let CastleHeartScore be the value of the territory, hearts owned by the player and all other players in their current clan
  - if CastleHeartScore exceeds MaxCastleHeartScorePerClan, the player cannot place the heart
- joining a clan:
  - let CastleHeartScore be the value of hearts owned by the player and all players in the clan they would join
  - if CastleHeartScore exceeds MaxCastleHeartScorePerClan, the player cannot join the clan
- claiming an exposed heart
  - let CastleHeartScore be the value of the exposed heart, hearts owned by the player and all other players in their current clan
  - if CastleHeartScore exceeds MaxCastleHeartScorePerClan, the player cannot claim the exposed heart


To use a limit of one castle heart per clan:
- MaxCastleHeartScorePerClan = 1
- CastleHeartScoreStrategy = EveryHeartWorthOnePoint

demonstration: https://www.youtube.com/watch?v=pmk5RHCz2_c


## Commands

`.territory-info` (`.ti` for short) will give you information about the territory your character is currently in.
- Useful for players to check how many points a territory is worth.


## Basic Config

Running the server with this mod installed will create a configuration file at `$(VRisingServerPath)/BepInEx/config/CastleHeartPolice.cfg`.

```
## Settings file was created by plugin CastleHeartPolice v1.0.0
## Plugin GUID: CastleHeartPolice

[General]

## The value of castle hearts owned by a single clan may not exceed this score.
# Setting type: Int32
# Default value: 1
MaxCastleHeartScorePerClan = 1

## Determines the value of each castle heart. (EveryHeartWorthOnePoint | CustomTerritoryScores)
# Setting type: String
# Default value: EveryHeartWorthOnePoint
CastleHeartScoreStrategy = EveryHeartWorthOnePoint
```

## Configuring custom scores for territories

You can edit `$(VRisingServerPath)/BepInEx/config/CastleHeartPolice/territoryScores.json` manually.

But I highly recommend using [this tool](https://cheesasaurus.github.io/v-rising-castle-heart-police/MapPainter/index.html) to generate the json file for you to copy/paste.
- It can also generate a map for you to share with the players on your server.
- demo: https://www.youtube.com/watch?v=QddkJ2aoed8
