
Vessels will now consist of 'islands'.
These islands are completely rigid.
Between the islands, some attach nodes define joints.
{
    each island needs to get a unity rigidbody. each island needs to have mass computed independently. Also the moments of inertia.
    physx joints are components, and only 1 side has them. so we need to define which one.
}

Decouplers split islands.
So do joint-type attachment nodes.

each part is connected by edges between attachment nodes.
These attachment connections don't specify the unity hierarchy anymore.

Needs a way to track what is connected to what.
Connections are still directed (directed graph) to be able to tell children from non-children.


construction system and vessel hierarchy need to change.
- Vessels will now have the root, then islands under that, then each part in the island flat.

Joint attach nodes add unity/physx joints between the islands?

Each part is indivisible again. We'll just add more parts to the composition if needed.

serialization of vessels needs to change.
{
    now uses 3 separate files
    vessel stub/meta
    gameobjects
    attachment graph? maybe not? I guess that could be stored in the gameobjects file.
    each part/island maybe separate? hmm...
}

needs a multi-rigidbody hybrid referenceframetransform
{
    when in absolute simulation, it freezes the movements of rigidbodies relative to each other.
    Has a 'root' rigidbody that the main position follows.
}

vessels will now be loaded/unloaded when needed.
- make a setting for the load/unload distance. Use hysteresis to avoid flickers.
All vessel stubs are always loaded, but the actual gameobjects may not be.
disable gameobjects without pausing when un/loading in the background (over multiple frames)
- components need to handle being disabled without being paused. so in that case they stop updating.

Allow vessels to perform background processing by a separate system.
- we want to perform this without actually loading gameobjects. maybe to be added later?

needs to have a flag on the vessel to check which simulation mode it's in (background/physicsless/physical) - possibly to stop numerical instability caused by high timewarp rates.

change the FComponents to all inherit from a common component. Replace the unity callbacks with custom ones? (for performance) maybe? need to check the difference in perf.

Far Vessel Pinning
- pin vessels that are landed, not moving, and are getting far-ish away from scene origin.
- unpin if the vessel doesn't have an anchor and a force is applied/velocity changed by hand.

always disable/reenable entire scene when saving? Camera could stop rendering for that time / show a saving screen?
would be nice to save in the background, but positions and values will be wrong and all out of sync/timestep if we do that.










