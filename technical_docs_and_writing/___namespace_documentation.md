# Namespace Documentation, or 'How the HSP Unity project is laid out'

aming scheme broadly follows the idea of "what's more closely related".

## Structure overview:

HSP is divided into loosely independent building blocks, or modules.
Each module is defined by an .asmdef


There is a "binder" layer (HSP.Vanilla, or more specifically HSP.Vanilla.Scenes) that has a bunch of listeners set up, which add the appropriate objects and set the getters/setters on everything.

Reference rules:

- Only `HSP.Vanilla.{subnamespace}` can reference `HSP.Vanilla` and `HSP.Vanilla.{other_subnamespace}`
    I.e. you can't reference from a higher layer.

If something doesn't reference anything other than `UnityPlus.{subnamespace}` or `HSP`, it shouldn't be made aware of anything else.


**Choosing the appropriate namespace/subsystem:**

1. Choose a namespace based on the primary relationship of its contents.
    E.g. vessels are being included as content, `so HSP.Content.Vessels` is preferred over `HSP.Vessels.Content`, 
    as the namespace directly contains content, but only relates to vessels.

2. The `HSP.Vanilla.xyz` subnamespace is for concrete implementations. 
    The game should still compile and run if you remove everything inside `HSP.Vanilla.xyz`, and replace it by modded variants.

3. If a component is not directly related to the core functionality of the namespace, but is an implementation of it, it should probably be moved to `HSP.Vanilla.Components`.

4. if something builds directly on top of something else, you can include it like so:
    `HSP.Vessels` --> `HSP.Vessels.Construction`






## Existing Namespaces:

#### Layer 1 ('library' layer):

HSP
HSP._DevUtils                           - Utilities for developers, temporary assets.
HSP.CelestialBodies
HSP.Content                             - The idea of content, files, and GameData.
HSP.ControlSystems                      - The core of the system for sending and receiving control signals between different objects.
HSP.Input
HSP.ReferenceFrames                     - 'Floating Origin' and 'Krakensbane' implementation.
HSP.ResourceFlow
HSP.SceneManagement
HSP.ScreenCapturing
HSP.Timelines                           - Game saves / "timelines".
HSP.Trajectories                        - Orbital and not-so-orbital mechanics.
HSP.Trajectories.Vessels                - Integration of trajectories with vessels for easy use. Could be moved inside HSP.Vanilla.
HSP.UI                                  - Some shared UI code.
HSP.Vessels
HSP.Vessels.Construction
HSP.ViewportTools                       - Viewport gizmos for translating, rotating, and scaling. Possibly move to UnityPlus.ViewportTools.

#### Layer 2 ('glue' layer):

HSP.Vanilla                             - Most of the vanilla "glue" code, also vanilla implementations of concepts from the lower layer.
HSP.Vanilla.Content.AssetLoaders        - External file asset loaders.
HSP.Vanilla.PostProcessing
HSP.Vanilla.UI                          - Most of the vanilla UI elements.
HSP.Vanilla.UI.Vessels
HSP.Vanilla.UI.Vessels.Construction



## Likely Future Namespaces:

HSP.Music

HSP.Vanilla.UI.Settings
> in-game settings ui

HSP.Vanilla.UI.Trajectories
> orbit drawers, etc.

HSP.Voxelization
HSP.Voxelization.Vessels

HSP.Buoyancy
> separate from drag, but will need to use the same voxel data

HSP.Terraforming
> editing the heightmaps of planets, building spaceports, etc.

UnityPlus.Localization




