the map is a separate additive scene



loading the map disables/deletes the default gameplay camera, and uses the map camera.



different render modes
- Natural - lifelike
- Other - topographical, land/water, etc.

the game is still simulated as normal when in the map view



map view has its own celestial body-like objects (they appear at locations where actual celestial bodies are) with their own points of interest
- need to figure out how to tell the map how to draw celestial bodies, since they might not have a unified way to render.

something to construct a display-CB for a specified real CB. Needs to be able to update / recreate in case the real CB changes during gameplay.

celestial bodies consist of components that aren't really standardized. composition allows the modders to specify any 'component' that will be then added to the body.

each component can have a separate kind of drawer or something for the map view?
- components can be complicated, and we don't want to reimplement components as drawers as this duplicates code too 
- no unsealing components to derive a drawer either. wrap if needed instead?


map can 'mimic' gameplay scene, so normal components could just be drawn like anything else.




would be a partially easier if I could decouple graphics and physics, but this also fucks with graphics raycasts, etc, as they would use physics.
- so I can't just reposition gameplay objs



**Map scene is built on top of everything else, that is, normal stuff doesn't know about the map scene**


in the map scene
- draw vessel markers (and orbits)
- draw celestialbody markers (and orbits)
- different map-only UI, but using standard components that also exist in the gameplay scene.


**problems**:
- 2 different cameras, exiting map should restore the *previous* camera (not hardcoded)

- 2 different canvases, 1 per scene?



**proposed solutions**:
- skip 'disabled' objects (would work both with canvases and cameras)

- somehow specify that we want to search only a specific scene (canvases)
- maybe add an event to fire when the active scene switches from the gameplay scene to something else, like 'startup' but with enabled/disabled pair instead?


when map is opened
- disable the gameplay scene cameras














