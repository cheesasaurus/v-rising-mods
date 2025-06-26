# EventScheduler

Enables server operators to schedule recurring Events.


## Installation

- Install [BepInEx](https://v-rising.thunderstore.io/package/BepInEx/BepInExPack_V_Rising/).
- Install [HookDOTS](https://thunderstore.io/c/v-rising/p/cheesasaurus/HookDOTS_API/).
- Extract `EventScheduler.dll` into `(VRising folder)/BepInEx/plugins`.


## Scheduling an event

An Event is really just a series of chat commands to be executed on a schedule.

Events are set up in `BepinEx/config/EventScheduler/events.jsonc`

Example:
```jsonc
{
    // Chat commands have to be executed by a user, so we specify which user
    "executingAdmin": {
        "steamId": 123456
    },

    // List of all events
    "events": [

        // an event to reset shards every 2 weeks
        {
            // after you've decided on an eventId, it shouldn't be changed
            "eventId": "Bi-Weekly Shard Reset",
            "schedule": {
                // when the first run of the event should happen
                "firstRun": "2069-12-15 20:00:00",

                // how frequently the event should run
                "frequency": "2 weeks",

                // In case the event is overdue: allow running it late, but not too late.
                // This could happen if the server was down/restarting when the event was supposed to run.
                // If it's too late, the event will be skipped until the next valid time slot.
                "overdueTolerance": "5 minutes",
            },
            "chatCommands": [
                // a series of chat commands to be executed each time the event runs
                ".shards-reset",
                ".buff all rage 2h",
                ".buff all witch 2h",
            ]
        },

        // an event to give players a gift every hour
        {
            "eventId": "Hourly lumber gift",
            "schedule": {
                "firstRun": "2069-12-15 20:00:00",
                "frequency": "1 hour",
                "overdueTolerance": "1 minute",
            },
            "chatCommands": [
                ".give all 9 wood",
            ]
        },

    ]
}
```

Configuration changes are automatically applied; no need to restart the server every time.

Note that all chat commands shown above are only examples. The actual commands available depend on which mods you have installed.


### Frequency and overdueTolerance
The expected format is `{integer} {unit}`

Valid units are: `weeks`, `days`, `hours`, `minutes`, `seconds`

examples:
- `2 weeks`
- `1 week`
- `30 days`
- `1 day`
- `5 hours`
- `1 hour`
- `90 minutes`
- `1 minute`
- `30 seconds`
- `1 second`




## Support

Join the [modding community](https://vrisingmods.com/discord).

Post an issue on the [GitHub repository](https://github.com/cheesasaurus/V-rising-mods). 