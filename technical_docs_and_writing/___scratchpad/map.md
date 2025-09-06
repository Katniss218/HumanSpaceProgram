# Map Scene
Map scene is used to draw a 'map' of the celestial system.

The map scene is intended to me loaded on top of the Gameplay scene.




different render modes
- Natural - lifelike
- Other - topographical, land/water, etc.




where are the attractors stored and what ensures they're synchronized?

ephemeris length

flight plan (maneuver nodes) duration player-defined?

'future' ephemeris length per-body?

'attractors' technically have nothing to do with attracting, since acceleration providers can ignore it.
- an attractor is any body that influences how other bodies move.

acceleration provider can invalidate the ephemeris from some UT onwards
- making a maneuver node far into the future shouldn't need recalculating the entire thing.

maneuver nodes would be acceleration providers, ofc.




prediction simulator and flight plan are the same simulator (?)
- leave same for now, will probably have to be separated later due to ephemeris length for drawing orbits





all ephemerides need to be at least as long (UT) as the flight plan length, otherwise they won't fit.

you can add maneuver nodes to any vessel, not just the active one.

technically I guess the flight plan length would be set by the player then and not in the body huh

changing the flight plan length requires updating the ephemerides' durations




maneuver nodes are acceleration providers, but only for the flight plan.

solar system is an acceleration provider that takes the state of the attractors.
- attractor ephemerides can be calculated ahead of time. followers could use the ephemerides instead of magically getting the state of the solar system from somewhere.



need to separate the 'flight plan' acceleration providers from the 'real' ones.
real ones get stored in the trajectory transform.
flight plan uses real ones + some of its own ones


unified ephemeris length would help solve some annoyances
ephemerides themselves shouldn't be defined in the traj transform



flight plan needs to be able to be reverted back to the ground truth state.


```csharp

{
    // reference UT is always TimeManager.UT.
    // this is the UT used when reverting the ephemeris/simulation.

    // needs to be able to partially resimulate bodies. so basically, every body tells it where in time it has been computed (ephemeris), and it can then use that data to roll-back to the old time.
}

```

simulator2 needs to ensure that the previous simulation has completed before it can start simulating again.

so you call simulate, and it starts to simulate.
any staleness will be registered and stop the simulation, and then you tell it to simulate again, which will fix the staleness and continue.




