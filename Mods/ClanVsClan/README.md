# Clan Vs Clan

⚠️WIP⚠️

Mitigates teaming during raids.

This is a server-side mod.

## The system
- start a "raid" instance on some trigger
  - trigger: castle is damaged by golem
  - raid instance
    - which castle 
    - attacking team
    - defending team
    - time started (timestamp)
    - last breach (timestamp)
    - last attack ( timestamp)
    - is expired (bool)
      - true if:
        - x amount of time (configurable) has passed since the last attack
        - AND (castle was never breached OR x amount of time (configurable) has passed since the last breach ended)
          - breach end time = last breach + breach duration (server configuration)
- there can only be 1 active raid instance per castle
  - update the active raid instance, don't start a new one
  - (technical) ecs system to update the lookups for which castles/teams have an active raid
    - remove any raids from the lookups that should be expired
  - persist all raid instances (even expired ones) for monitoring/debugging purposes
    - could be used to implement some anti-griefing rules if clan A is getting successfully raided by clan B every day 
- rules while there's an active raid
  - building mode cannot be used to pull up placed shards in the raided castle, into the defenders' inventory
    - configurable (bool)
      - e.g. ForbidPickingUpOwnPlacedShardDuringRaid = true
  - castles owned by the defending team can only be damaged by the attacking team
  - castles owned by the attacking team can only be damaged by the defending team
  - let involved players be anyone on the attacking team, anyone on the defending team, and admins
    - uninvolved players cannot damage golem stones of involved players and vice-versa
    - uninvolved players cannot enter territories of involved players and vice-versa  
    - uninvolved players cannot damage involved players and vice-versa
      - what about transporting shards?
        - if any involved player had a shard in their inventory outside of their team's castles within last x amount of time (configurable)
          - allow involved/uninvolved players to damage eachother outside of the involved players' territories
          - this special handling for shards should be configurable (bool)
            - e.g. AllowGankingWhenRaidedShardTraveling = true
- clans are locked during raid hours
  - players cannot join a new clan 
  - but players should be able to leave / be kicked any time
    - how to handle this to prevent gaming the system?
      - technical: we need to use our own team model rather than using the game's Team component
          - e.g. instead of  `raidTeam.equals(castle->team)` it would be `raidTeam.hasPlayer(castle->owner)` etc
          - and snapshot the players in the team

## Outcome
- essentially creates a war between 2 clans, with nobody else able to interfere while the war is ongoing
  - attackers can resupply without getting ganked
  - attackers can carry loot home without getting ganked
  - attackers can bring shard home without getting ganked (optional)
  - other clans can't steal loot out of raided castles
- allows raiding multiple castles
- allows defenders to counter-raid the attackers
- prevents clan-hopping shenanigans
- doesn't solve the issue of teaming before the golem is up and hits something


## Addressing pre-raid shenanigans
- a clan can declare war. e.g. `.war-declare Bobby`, where Bobby is one of the enemy players
  - announces the war in chat
  - while the war is active, all the rules about active raids are applied
  - clans can only be in one war at a time
    - (other clans cannot declare war on participants) 
  - the war will expire after x amount of time (configurable) unless:
    - there is an active raid related to the war
    - an involved clan placed a golem stone within x distance of the other clan's castle(s)
      - within last y amount of time (configurable)
    - an involved clan was in golem form within x distance of the other clan's castle(s)
      - within last z amount of time (configurable)
- a clan cannot re-declare war on the same clan too soon
  - within x amount of time (configurable) after the last war with that clan expired
  - only if they were the clan who declared the war

### example:
```
Billy: .war declare Bobby
System: WAR DECLARED!
[SWAG] Billy, Sally, Suzy
vs
[HIVE] Bobby, Joe, John

System (to [SWAG]): You have 20 seconds to place a golem before the war is cancelled.
(after 20 seconds)
System: WAR CANCELLED ([SWAG] vs [HIVE])
Billy: .war declare Bobby
System (to Billy): You must wait 15 minutes before declaring another war on [HIVE]
```