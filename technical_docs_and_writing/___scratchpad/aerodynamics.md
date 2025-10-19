# Aerodynamics

## Gameplay integration:

Not part of Trajectories, implemented similarly to rocket engines.
- trajectories are not relevant for powered flight anyway, too much noise in the wind/etc.

Modular design.

Pinned vessels (buildings, etc) are not subject to drag. Only freely moving ones.
There is a similar thing already implemented for trajectories, where pinned vessels are not simulated by the trajectory transform/manager.

Apply direct drag/lift forces/torques to vessels via IPhysicsTransform / IReferenceFrameTransform.

`Aerodynamic integrator` - a monobehaviour added to each vessel root object.

`Aerodynamic integrator` will query the `Atmosphere provider` for the atmospheric properties at point (xyz) every fixedupdate, 
    compute the lift/drag, and apply it to the vessel.

`Atmosphere provider` - will be part of the global spatial data provider system. 
    you give it a point and what data you want to return and it looks that up.
    - for now, it can be a simple static class that collects all atmospheres and just queries the closest one.

saved with the vessel or recreated on every spawn?
- saving is simpler to implement
- recreating more difficult.

## Flight Planning (Trajectories system) integration:

TODO - figure out.
right now there is no way to get the atmo data for a given trajectory body, and anything used for that would need to be thread safe.
flight planning is inherently multithreaded.

needs an acceleration provider and that in turn needs to know which body has which atmosphere data.
- for now, just leave it.

## Trajectories vs Forces:

Generally, trajectories are for 'simple' mechanics, like constant thrust, gravity, simplified aero, etc.
- simple meaning in situations where no other 'complex' forces are applied directly to the rigidbody.
- keep in mind that forces can also be applied directly under physicsless timewarp (physicsless meaning that physX is not involved).

we need a thrust module for the trajectories.
- needs to modify the mass.
- needs to have rotation - this defines where the engines are currently pointing, lol.


## Impl details:

iaerointegrator goes in a new assembly `HSP.Aerodynamics`

globalatmosphereprovider goes in core hsp? (the global spatial data system needs to be public across all assemblies I think)
iatmosphereprovider goes next to global

The initial drag/lift solver should be very simple and suitable to verify that the system is working correctly.
a simple constant Cd + reference area maybe?


