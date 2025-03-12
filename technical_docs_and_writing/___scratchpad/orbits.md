# Orbits

    This document is not strictly documentation. It hopefully should be close, but I'm not perfect and sometimes can't be bothered to update every doc.
#








# TODO

for some reason, the vessels don't have rootobjecttransform set when deserializing.






if frame is switched to moving, the planet position changes relative to the vessel position.
opposite happens when it's switched to rested.


so the transform.position (_rb.position) at the end of frame is different between the 2 cases.

--                  NOT ANYMORE if scenereferenceframemanager runs first, the velocity is not applied to the free transform on frame switch.











phys transform positions need to be accurate on every physics frame (not sub-frame).
we can just set the world pos of the planet on every frame right?

maybe invert the responsibility? each body gets a list of things it should act on? instead of each thing enumerating the bodies.


## physics-easing-like thing

if an object is resting on top of another object, the acceleration that moves that object also needs to be applied to the other object.
This is most notable with things like planets, where if the planet starts moving, you'd expect the stuff on top of it to follow.



when should the vessel follow the planet (when planet accelerates):
- at least when it's landed.
    - so only when it's pinned???
    - so we'd need pinning/unpinning based on landed/forces then

we also need a proper celestial factory/serialization.

creation of a timeline should have parameters, somehow.


celestial body should be loaded when creating in the same way as they are during load.
- the planetary system is gonna be in json somewhere anyway, might as well make it the same format.



frame starts

FixedUpdate
- possible modification of values and cache invalidation
Before physicsprocessing
- trajectories are simulated with current values
- positions of physicsobjects need to be back-fed and set to what was simulated, but using moveposition
physicsprocessing
- potential collisions and cache invalidation


we feed the positions and velocities to the simulation


simulation will compute the position of the object, that's correct IF IT HASN'T COLLIDED WITH ANYTHING (during fixedupdate or physicsupdate).
- so if it hasn't collided with anything, then move it, but if it has, then don't?
- also, if the velocity has changed (forces applied outside the trajectory), then we also don't want to apply the trajectory, 
    or the force could be passed into the trajectory as well.



keplerian orbit will only calculate its cached values for current UT on demand

newtonian orbit


how do we want to pass in the parent body?

bodies CAN'T REQUIRE to be created in the order in which they're parented, because not all orbits will have parents (point or newtonian don't eg).
parent body is a trajectory.

what we need about the parent body is its trajectorystate (pos,vel,acc)

parent body needs to be an ITrajectory
parent body needs to be able to be assigned after creating the child trajectory



celestial system creation needs to have its own event/event_listener pretty much
- celestial system creation is gonna be created using deserialization.

each planet is its own serialization unit.

planets need a way to set the fields on-demand based on other planets (parent bodies) but only after those are spawned.
- I put ITrajectory in there for now.

Need a way to pass in the orbit parameters when creating a planet.
**planets shouldn't have the same flat callback to create the trajectories.**
some planets might want to be simulated newtonously, and some keplerianly, or something else.



maybe reference the planet by its string ID (same as in pinned physobject) and get the parentbody via celestialbodymanager (lambda getter) as needed?

**move specific trajectories into vanilla...**



#### before physicsprocessing
regardless of synchronization
- if sim and rigidbody pos/vel is NOT the same (not synchronized)
  - feed rigidbody velocity and position into the sim (synchronize)
- run sim
- sim gives you new velocity
- calculate the delta between the current RB pos and sim's new pos (sim's initial pos and rb pos should be the same)
  - this is not the same as just velocity because velocity uses subframes
- set that divided by fixeddeltatime as the rigidbody velocity

if someone sets the velocity by hand it will be taken into account because we will backfeed it right at the start.

if we don't set position, the position will be out of sync after one frame.
- this averaged velocity is accurate as far as the frame is concerned though, and will help keep things in sync
- sim has partial frames in between frames, and we don't care what those are.

#### physicsprocessing, collision happens, etc. 

if the position and velocity is as expected, there was no collision (forces can be applied later or earlier in the next frame tho)



then you also can have joints and stuff that basically act like collisions



what if we re-set the position after physicsprocessing, but only if the rb position matches the prediction by directly accumulating its pre-physicsprocessing velocity?
- this kind of assumes that the step will be performed after, but that's probably a reasonable assumption for now.

can I somehow use the position from previous frame maybe?

maybe set the position to where it will be after accumulating velocity?

does AddForce/Torque change the rb velocity immediately? if not, this won't work.

3. Use a non-kinematic rigidbody inside unity, but zero out the velocity/angular_velocity and feed collision responses back into the physics transform?
  - So it kind of acts like kinematic but still responds to collisions?


4. overwriting the pos after the body has been moved seems simpler

setting the velocity to make it move the specified distance during accumulation doesn't work because then the velocity is wrong when I get it on the next frame.


5. set the velocity of the object before the physprocessing to be whatever it needs to be for the step, 
  - and after (if it's close enough to what it was), reset it back to the correct value?
  - if it doesn't collide, we don't feed the pos back, because it will introduce roundoff errors

5.1. physics stepping should all happen at a consistent point during the fixed frame.

for systems of objects (joints) the trajectory transform should average the center of mass of everything and use that instead of the rigidbodies directly.
- so the trajectory transform would encapsulate that and hide the 2 transform properties
- unless we make another referenceframetransform for jointed objects, which would probably make more sense
but what if we want to distinguish which part to apply the force to?
vessel is not enough either if vessels are jointed
parts will get the vessel segment instead of the vessel itself

could use events for the step.

add HasCollidedInThisFrame that will ensure that during the frame with collision it stays true, until the start of the next frame?


we could also have an event that would be triggered whenever a collision or other something is detected by the physicstransform, which would be more modular and probably better.

this won't work with only before physprocessing, because this will be a problem when under thrust as well.




we need to only feed the position back to the trajectory when the position is not synchronized
- doing it always multiplies roundoff errors for keplerian



run the game with debug.log logging the input parameters to the method as well as the computed kepler parameters until nan or runaway eccentricity hits.
we can also grab the parameters for which the orbit eccentricity + anomaly + arg pe changes to pi

then write test cases with them to hopefully reproduce

