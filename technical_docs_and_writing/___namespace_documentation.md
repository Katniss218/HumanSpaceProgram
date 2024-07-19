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
HSP.Content
HSP.Content.Trajectories                - Metadata for timelines.
HSP.Content.Vessels                     - Asset loaders and metadata for vessels/parts.
HSP.ControlSystems                      - The core of the system for sending and receiving control signals.
HSP.Input
HSP.ReferenceFrames
HSP.ResourceFlow
HSP.SceneManagement                     - Scene management and loading.
HSP.ScreenCapturing
HSP.Timelines
HSP.Trajectories                        - Orbital mechanics and related.
HSP.UI                                  - Shared UI code. Possibly merge into UnityPlus.
HSP.Vessels
HSP.Vessels.Construction
HSP.ViewportTools                       - Viewport gizmos for translating, rotating, and scaling. Possibly merge into UnityPlus.

#### Layer 2 ('glue' layer):

HSP.Vanilla                             - Shared vanilla code. Can only reference `HSP`. Referencees go into a subnamespace.
HSP.Vanilla.Content
HSP.Vanilla.Components                  - Vanilla implementations of most FComponents.
HSP.Vanilla.Scenes                      - Defines how each scene is set up in vanilla.
HSP.Vanilla.Scenes.PostProcessing       - Adds post processing to the vanilla scenes.
HSP.Vanilla.UI                          - Shared vanilla UI code.
HSP.Vanilla.UI.Components
HSP.Vanilla.UI.Scenes
HSP.Vanilla.UI.Timelines
HSP.Vanilla.UI.Vessels
HSP.Vanilla.UI.Vessels.Construction



## Likely Future Namespaces:

HSP.Audio
HSP.Audio.<category>
HSP.Audio.Music

HSP.Settings
HSP.Vanilla.UI.Settings
> in-game settings ui

HSP.Vanilla.Scenes
- SettingsScene
HSP.Vanilla.UI.Scenes
- SettingsScene

HSP.Vanilla.UI.Trajectories
> orbit drawers, etc.

HSP.Voxelization
HSP.Voxelization.Vessels

HSP.Aerodynamics
> atmo and water drag

HSP.Buoyancy
> separate from drag, but will need to use the same voxel data

HSP.VisualEffects
> core function for plumes and shit, no specific implementations

HSP.VisualEffects.ResourceFlow
> integration with resflow


HSP.DataFixer
> upgrading old save files
HSP.Vanilla.DataFixer
> actual implementations of things to fix between versions.

HSP.MapView

HSP.Scenarios
> tutorial goes here

HSP.Terraforming
> editing the heightmaps of planets, building spaceports, etc.


UnityPlus.Localization




