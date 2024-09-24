# Shard Police

⚠️WIP⚠️

Introduces optional rules related to Shards, and some admin commands.

This is a server-side mod.

## Features

- Limit players to 1 shard buff at a time. Activating a shard replaces any previously held shard buff with the new one.
  - Optional, can be enabled/disabled via config. Enabled by default.
  - demo: https://www.youtube.com/watch?v=-rOv9I48Xks
- Prevent placed shards from being moved by owner during raid hours.
  - Optional, can be enabled/disabled via config. Enabled by default.
- Commands (admin only) to remove shard buffs from players.
  - `.shard-buffs-remove Bobby` removes the shard buffs from the player named "Bobby". (`.sbr Bobby` for short)
  - `.shard-buffs-remove-everyone` removes the shard buffs from all players. (`.sbre` for short)
  - Affected players are notified when their buffs are removed.
- Command (admin only) to reset shards (the old placeable relics, not the new wearable ones).
  - `.shard-reset-relics`
  - Players are notified when this is used.
  - Removes shard buffs from all players and returns shards (relics) to their bosses.
  - demo: https://www.youtube.com/watch?v=cwIH6dwlZ8Q

## TODO (not implemented)

- mitigate sharing shard buffs outside the clan that holds the shard. strip shard buffs on clan changes. think about potential workarounds
  - e.g. shard holder joins a clan, they all grab the buff, then the shard holder leaves. simply removing the buff from the holder when they leave wouldn't be enough.
  - e.g. exposing castle heart to take control of shard
  - optional, can be enabled/disabled. default enabled
- strip shard buff from players when the placed shard is picked up / dropped on the ground
  - optional, can be enabled/disabled. default enabled

## Config

Running the server with this mod installed will create a configuration file at `$(VRisingServerPath)/BepInEx/config/ShardPolice.cfg`.

```
## Settings file was created by plugin ShardPolice v1.0.0
## Plugin GUID: ShardPolice

[Shard Buffs]

## Whether or not to limit each player to only 1 shard buff at a time
# Setting type: Boolean
# Default value: true
LimitShardBuffsToOnlyOneAtATime = true

[Shard Movement]

## Whether or not to prevent shard owners from moving their placed shard during raid hours
# Setting type: Boolean
# Default value: true
PreventShardOwnersMovingPlacedShardDuringRaidHours = true

```
