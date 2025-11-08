# Fluid Flow

    This document is not strictly documentation. It hopefully should be close, but I'm not perfect and sometimes can't be bothered to update every doc.
#

## Background/current state of things:

Currently each tank (`FBulkContainer_Sphere`) is fully independent (black-box), connected by pipes (`FBulkConnection`). The solver is not very robust and it's easy to fuck up the flow.
Each tank has an inflow/outflow that can be set, and overfilling is not handled properly either.
Engines can only consume a given amount of specific propellant (unrealistic).

## Introduction:

Lumped flow model. The domain of an individual `tank` is modelled as a tetrahedralization of a set of `nodes` by a network of `edges`.
The edges (not nodes, and not tetrahedrons) are where the propellant is stored on our model.
Each edge will have an 'proper' volume assigned to it - this is how much actual fluid can be stored inside the edge.

This is motivated by the fact that it's trivially easy to get the fluid height of a line segment with arbitrary orientation. Just project it onto the acceleration vector.
And that is needed to tell which inlets have fluid and which ones don't.

## Volume calculation:

To get the 'proper' volume of each edge, we start with the user-defined 'desired' total tank volume. This can be anything, from 0 to infinity.
We then calculate the 'desired' volumes for each tetrahedron (and their total 'desired' volume, i.e. the sum), which will be used for proportional scaling.
The 'actual' volume of a *tetrahedron* is then: `actual = (desired / desired_total) * actual_total`.
That is then split up between the edges that are part of this tetrahedron, according to the edge length (similar proportionality as above).
Then we can get the 'proper' volume of the *edge*, which is just the sum of the contributions from each tetrahedron that this edge is a part of.

## Flow:

#### Inside a tank:

the flow limit through an edge within a tank is infinite.

The tank can contain many different fluids. If it does, they're assumed to be:
- stratified with no mixing (i.e. ullage gas on top) when the tank is under acceleration.
- perfectly mixed when in 0-g.
- in-between (lerp?) near 0-g. Possibly with a time-varying factor smoothing the effect of rapid acceleration changes.

When the fluid exits through the inlet, the fluid is removed from the 3 edges connected to it.
- For liquids - it drains edges until the fluid level is below the inlet (relative to the acceleration vector). 
- For gasses - it can drain from below as well.
The fluid then immediately back-fills/settles the drained edges?

**Important!** Needs to handle the case when the flow *across* the tank is larger than the capacity of the tank/the tank is full.
**Important!** Needs to handle the case where the volume of an edge is not enough to satisfy the outflow from an inlet (feeds from the connected edges, then re-settles).

#### Between the tanks:

Individual tanks are connected by `pipes`. Each pipe must connect to the tank at some node (if it doesn't, a node will be created, more about this below).
This node is known as the `inlet`.
The flow in the pipe (connecting 2 tanks) depends on both of its inlets' areas and the difference in pressure across the pipe.
The pipe can be a simple flow-through pipe, but can also be a pump, a valve, etc.

## Tetrahedralization:

The tank starts out with a tetrahedralization using an arbitrary user-defined set of nodes. The positions of the nodes are serialized to json, the rest is computed on creation.

Voids can be represented by something that prevents the formation of tetrahedrons.

After tetrahedralizing the base structure, inlets can be added/removed. This will change the tetrahedralization by splitting, adding, or removing tetrahedrons.
- if inlet outside of existing tetrahedrons: add a new tetrahedron between the closest triangle and the inlet position.
- if inlet inside of existing tetrahedrons: split the inner tetrahedron into 4 new tetrahedrons with a new node at the inlet position.
- if inlet is removed: either combine (if split when created) or remove (if added when created).
After removing all added inlets the resulting structure should be equivalent to the original.
If the inlet position is 'close enough' to an existing node, that node can be moved to snap to the position where the inlet will be (needs to be moved back again )

## Implementation notes:

Should handle a 100-tank network easily in realtime (if the tank consists of a reasonably-low amount of edges, like 10-20). And multiple smaller networks at once also easily.

Each independent full network (vessel) could be solved in parallel (this necessitates the use of worker-thread-safe types and APIs - Unity annoyance).

Precompute and cache as much as is viable.

#### Resource producers and consumers:

There may or may not be different types of tanks and pipes connecting them, so the entire system should be generalized.
Other components also interface with the system, like air intakes, engines, vents to atmosphere, etc.
They may produce resources from nothing, and/or consume them into nothingness. Each producer/consumer is like a separate tank (our tank must connect to it with a pipe), and may have a maximum flow capacity for creating/destroying fluid connected to it. The flow direction is determined by the pressure across the pipe connecting the tank to the producer/consumer.

#### Other:

The `SubstanceStateCollection` should use pooling to reduce backing array/list allocations for operations.

Each fluid (liquid and gas) should be compressible (at least a little). This both matches reality, and allows easier solving of the flow between tanks/over/underfilling.

An engine should allow you to pump anything into it, but will only work "correctly" when driven near the design parameters. The engine will usually contain its own pump that sets the pressure/flowrate to be the design pressure/flowrate.

Ullage gas is generated by boiling the contents (if tank is sealed and you pull from it, it would generate a vacuum), or filling the tank with it from a separate supply.
The ullage gas is always a fluid that is part of the fluid inside the tank.

Realistically, every tank will always contain *some* fluid (gas), be it atmospheric air, an inert gas, or whatever else.

#### Some cases we need to handle:

Pump dead-heading/overfill (pump tries to pump into a tank with no outlet or a closed valve, or into another pump pointed back at it) - pressure spikes.
Loops (A -> B -> C -> A)
Inlet creation (edge insertion)-order independent.
- example: inlet A snapped to a node, inlet B then creates a new node "off to the side" - both should drain from the same total capacity if their heights are the same.
Inlet edge creation/deletion shouldn't delete/duplicate fluid contents.
Cavitation - pressure drops, liquid boils into a gas.
Rapid acceleration changes - will generate pressure spikes.
Needs a failure condition when tank exceeds allowed pressure.
fluid of equal density at equilibrium pressures should diffuse across the tanks.

#### Solve Step:

no matrices yet.

the solver runs on a separate thread, using snapshots?

there are 2 basic solvers. the tank internal solver and the flow between tanks solver.

every frame
{
    1. settle tanks
    2. get pressures at each inlet
    3. compute flowrates and iterate the solution for the 'next step' until none of the tanks are overfilled or negative.
    - iterate all tanks, compute how much fluid would be in each, and if any are < 0 || > max, then adjust the flowrates between the affected tanks.
    pressure will depend on how much fluid is inside ("everything is compressible" paradigm)?
    so tanks are assumed to be in equilibrium at each step? - not necessarily. only if flowrate is enough to equilibrate in 1 step.
    the tank is too full, if the pressure resulting from the compressed volume is > pressure feeding into the tank.
}

changes:
- the 'clamp inflow' thing gets removed and replaced by compressibility (until pressure equilibrium or structural failure)

substance needs a state (gas/liquid/solid)
we only run settling if the tank contains a liquid or more than 1 gas. Single gas assumed uniform

pressure is equal to the amount of mass (ideal volume) divided somehow by the actual available volume.


FlowXYZ elements (flowtank/flowpipe) are the multithreaded data.

 

flow builder could just register objects. those objects



