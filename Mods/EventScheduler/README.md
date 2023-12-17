# EventScheduler

Enables server operators to schedule recurring Events.

## Scheduling an event

An Event is really just a series of chat commands to be executed on a schedule.

Events are set up in `BepinEx/config/EventScheduler/events.jsonc`

e.g.
```
[
    // an event to reset shards every 2 weeks
    {
        // after you've decided on an eventId, it shouldn't be changed
        "eventId": "Bi-Weekly Shard Reset",
        "schedule": {
            // when the first run of the event should happen
            "firstRun": "2069-12-15 20:00:00",

            // how frequently the event should run
            "frequency": "2 weeks"
        },
        "executingAdmin": {
            // chat commands have to be executed by a user, so we specify which user
            "steamId": 123456
        },
        "chatCommands": [
            // a series of chat commands to be executed each time the event runs
            ".shards-reset",
            ".buff all rage 2h",
            ".buff all witch 2h"
        ]
    },

    // an event to give players a gift every hour
    {
        "eventId": "Hourly lumber gift",
        "schedule": {
            "firstRun": "2069-12-15 20:00:00",
            "frequency": "1 hour"
        },
        "executingAdmin": {
            "steamId": 123456
        },
        "chatCommands": [
            ".give all 9 wood"
        ]
    }

]
```

Note that a server restart is required after changing configuration.