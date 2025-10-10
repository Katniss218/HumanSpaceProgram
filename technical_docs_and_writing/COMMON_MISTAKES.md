# Common Mistakes

Or how to avoid unnecessary edits during a code review :)

## Unity:
Pitfalls related to Unity itself, common across different projects.

#### 1. Material vs SharedMaterial:
Use `.sharedMaterial` instead of `.material` (unless you know what you're doing).
The `.material` accessor lazily creates a copy of the Material instance (even if you're just reading its value!), impacting the performance compared to using `.sharedMaterial`.

#### 2. GetComponent<T> and similar:
Certain Unity methods (like `GetComponent<T>`, `FindObjectsOfType<T>`, etc) are expensive.
If you are using the result frequently - cache it in a field instead of re-getting it every frame.

Example:
```csharp
Rigidbody _rb;
void Awake()
{
    _rb = this.GetComponent<Rigidbody>();
}
```

#### 3. Object Pooling:
Object pooling is very useful when you have a lot of very similar objects that live for a bit and then die, only to be respawned later.
Unity already has an object pool class - `ObjectPool<T>`, and UnityPlus implements one as well - Use it when applicable - `ObjectPool<TItem, TItemData>`.



## HSP-specific:
Pitfalls specific to HSP and HSP only.

#### Unity Editor:
Try to not rely on the editor, and (important) never add objects/components to the unity scenes (main menu, gameplay, design, etc).
These scenes are intended to be populated by HSPEvents listening to the respective scene lifetime HSPEvents.
Implementing things that way has the benefit of letting mods disable the function and replace it with their own easily.

Example:
- Someone makes a mod that replaces the sequencer UI panel with their own.

#### Singletons:
Use `SingletonMonoBehaviour<T>` (global singleton) or `SingletonPerSceneMonoBehaviour<T>` (one per scene) instead of reimplementing your own.

Example:
```csharp
public class MyManager : SingletonMonoBehaviour<MyManager>
{
    // You can use the `.instance` and `.instanceExists` inherited static members.
    // ...
}
public class MyPerSceneManager : SingletonMonoBehaviour<MyPerSceneManager>
{
    // You can use the `.GetInstance(unityScene)` inherited static member.
    // ...
}
```

#### HSPEvents:
The `HSPEvent` lies at the core of Human Space Program. It binds a lot of the systems together and lets the game consist of mostly self-contained modules.
Use HSPEvents if you want to respond to all the event invocations.
Use plain C# events (instance events) when you're interested in listening to only one object.
HSPEvents support ordering (`Before`, `After`) and blocking (`Blacklist`). See `HSP.HSPEventListenerAttribute` for more.

Raising HSPEvents:
```csharp
HSPEvent.EventManager.TryInvoke( event, data )
```

Listening to HSPEvents:
```csharp
const string LISTENER_ID = "33163fee-09f5-47ce-8d29-936571149b9e";

[HSPEventListener(HSPEvent_STARTUP_IMMEDIATELY.ID, LISTENER_ID)]
private void ListenerMethod()
{
    // Method that doesn't take event parameters.
    // ...
}

[HSPEventListener(HSPEvent_GAMEPLAY_SCENE_LOAD.ID, LISTENER_ID, After = new[] { DevUtilsGameplayManager.LOAD_PLACEHOLDER_CONTENT })]
private void ListenerMethod( data )
{
    // Method that takes event parameters.
    // ...
}
```
Some commonly used HSPEvents:
- `HSPEvent_STARTUP_IMMEDIATELY`
- `HSPEvent_<scene_name>_SCENE_LOAD`, `_UNLOAD`, `_ACTIVATE`, `_DEACTIVATE`
- `HSPEvent_ON_VESSEL_CREATED`
- `HSPEvent_ON_CELESTIAL_BODY_CREATED`

#### Asset Handling:
HSP manages assets using the `AssetRegistry` class. It's applicable for the vast majority of cases - instead of loading files directly, using `Resources.Load()`, or other.
- `AssetRegistry.Get<T>(assetID)` - Retrieve assets by ID.
- `AssetRegistry.Register(id, obj)` - Register runtime objects.
- `AssetRegistry.RegisterLazy(id, loader, isCacheable)` - Register lazy-loaded assets.
- Assets can be loaded from both in-project files (including `Resources/`) and `GameData/` mod directories.
GameData file asset loaders are in `HSP.Vanilla.Content.AssetLoaders`.

#### Serialization:
You don't have to like it, but please use `SerializationUnit.Serialize<T>( ... )` and `SerializationUnit.Deserialize<T>( ... )` instead of Newtonsoft.JSON, or - god forbid - Unity's builtin serializer.
Newtonsoft is very good, but UnityPlus also has some advantages:
- Time-budgeted serialization - saving and loading 'in the background' on any thread, without blocking the thread.
- Context-based serialization - change behaviour when specified.
- Native support for immutable types, non-default constructors, and removing members inherited from the base class.
- Native support for Unity types, including GameObjects.
- Robust support for reference handling and remapping references.
And a lot of things HSP uses can't be (easily or at all) serialized by the Unity's serializer.

**IMPORTANT**: Unfortunately, for every type you create must manually define a serialization mapping, or serialization will fail.

Use `ObjectContext`, `ArrayContext`, and `KeyValueContext` to seamlessly serialize e.g. references and assets. You can also create your own contexts if you need to.

Examples:
```csharp
[MapsInheritingFrom(typeof(MyClass))]
public static SerializationMapping MyClassMapping()
{
    return new MemberwiseSerializationMapping<MyClass>()
        .WithMember( "my_member", o => o._myMember );
}

[MapsInheritingFrom(typeof(MyGenericClass<,>))]
public static SerializationMapping MyGenericClassMapping<T1, T2>()
{
    return new MemberwiseSerializationMapping<MyGenericClass<T1, T2>>()
        .WithMember( "my_member1", o => o._myMember1 )
        .WithMember( "my_member2", o => o._myMember2 );
}

[MapsInheritingFrom(typeof(MyPartiallyImmutableClass))]
public static SerializationMapping MyPartiallyImmutableClassMapping()
{
    return new MemberwiseSerializationMapping<MyPartiallyImmutableClass>()
        .WithReadonlyMember( "immutable_member", o => o.immutableMember )
        .WithFactory<string>( ( immutableMember ) => new MyPartiallyImmutableClass( immutableMember ) ) // Order is important here.
        .WithMember( "my_member2", o => o._myMember2 );
}
```

In the future, HSP will also get a data manipulation system (and probably a Domain-Specific Language) that will operate on the `SerializedData` and perform 'patching'. As well as a managed object inspector/editor for mod making.

#### Input System:
Prefer the `HierarchicalInputManager` over raw Unity input system:
- Use `HierarchicalInputManager.BindInput(channelId, IInputBinding)` to bind inputs to channels.
- Use predefined bindings: `KeyDownBinding`, `KeyUpBinding`, `KeyHoldBinding`, `MouseClickBinding`, `AxisBinding`, etc.
- Enable/disable channels for context-sensitive input, and more.

#### Prefabs and GameObject creation:
Generally don't use prefabs outside of maybe debugging. This is mostly related to Unity serialization being unable to serialize a lot of common C# types.

There are several 'approved' ways to create GameObjects:
- Call `new GameObject()` and `AddComponent<T>()` manually.
- Use `SerializationUnit.Deserialize<GameObject>()` to load GameObjects from JSON.
- Use `AssetRegistry.Get<GameObject>()` for GameObjects that are assets.
- Remember to move root objects to the correct HSP scene - `HSPSceneManager.MoveGameObjectToScene()`.

#### ScriptableObjects:
Avoid using ScriptableObjects entirely, use plain C# objects loaded as assets from `GameData/` instead. They're exposed to the player as files and make it easier to mod/change the game.

#### Effects:
HSP has an `Effects` module, which implements configurable pooled effects (audio, particle, meshes, lights).
Use them where applicable instead the raw Unity particles, audio sources, etc.

#### HSP Scenes vs Unity Scenes:
HSPScenes are strongly-typed scenes with the type doubling as the scene manager for that particular scene.

Don't use Unity's `SceneManager` directly. Use `HSPSceneManager` instead:
- `HSPSceneManager.LoadScene<GameplaySceneM>()` - loads the gameplay scene
- `HSPSceneManager.SetAsBackground<MapSceneM>()`, `HSPSceneManager.SetAsForeground<MapSceneM>()`
- `HSPSceneManager.MoveGameObjectToScene<T>(gameObject)` - moves objects between scenes
- All HSP scenes inherit from `HSPScene<T>` and implement `OnLoad()`, `OnUnload()`, `OnActivate()`, `OnDeactivate()`

The `_AlwaysLoaded_` scene contains core services and should never be unloaded

#### Reference Frames vs Unity Transforms:
HSP uses 64-bit double precision reference frames to avoid floating-point precision issues.
- `IReferenceFrameTransform.AbsolutePosition` - 'absolute' coordinates
- `IReferenceFrameTransform.Position` - scene-space coordinates (Unity's 32-bit space)
There are appropriate 64-bit APIs - `Vector3Dbl`, `QuaternionDbl` for working with 64-bit transform math.

#### UI and Layout:
Use UnityPlus' `UILib` instead of using the UGUI elements (from the `UnityEngine.UI` namespace) directly.
The library is a wrapper around UGUI, greatly simplifying the process of creating UI elements through code. 

It uses modular and reusable controls for its UI elements, and also has a custom layout engine which is faster than the UGUI one.

#### F-Prefixed Components:
HSP uses F-prefixed components for 'vessel **f**unctionalities' (hence the F)
They're largely analogous to 'part modules' from KSP.

Examples:
- `FPart` - marker component, splits the hierarchy into parts
- `FRocketEngine` - propulsion systems
- `FPlayerInputAvionics` - flight control
- `FReactionControlController` - RCS systems
- `FSequencer` - analogus to staging
- `FAnchor` - ground-anchored objects
- `FAttachNode` - part connection points
- `FConstructionSite` - construction zones


*This document is intended to be a living reference. Feel free to propose additions/changes!*

*Last change: 2025/09/05*

