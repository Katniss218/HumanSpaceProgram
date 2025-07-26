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


camera in the map scene can't be parented to anything.

64-bit 'center' and use a pinned reftransform without a 'parent' to pin to (pinned reftransform needs to be split away from celestialbodies)
- pinned without 'parent' will use scene as the frame.
- so the camera will change where it's pinned in reference to scene origin?
    will work just rotate it so it points to where the cameraParent would be.
needs some mafs


# Rendering celestial bodies:

solution 1:
- render using actual CB modules
adding an interface for IVessel / ICelestialBody is beneficial anyway

solution 2:
- custom map-specific rendering modules (annoying because need to sync with actual CB and duplicate stuff)










