# Common Mistakes

Or how to avoid unnecessary changes after a code review...

## Unity stuff:

#### 1. Material vs SharedMaterial:
Use `.sharedMaterial` instead of `.material` (unless you know what you're doing).
The `.material` accessor creates a copy of the Material instance (even if you're just reading its value!), making the game run slower compared to when using `.sharedMaterial`.


## HSP-specific stuff:

#### Singletons:
Use `SingletonMonoBehaviour<T>` instead of reimplementing your own.
Example:
```csharp
public class MyManager : SingletonMonoBehaviour<MyManager>
{
    // Can use `.instance` and `.instanceExists` inherited static members.
    // ...
}
```

#### HSPEvent:
The `HSPEvent` lies at the core of Human Space Program. It binds a lot of the systems together and lets the game consist of mostly self-contained modules.
Use HSPEvents if you want to respond to all (or most of) the event invocations, and use instance events (plain C# events) when you're interested in following only that object.
HSPEvent listeners can specify to run before/after a list of other listeners, and can also block listeners from executing. See `HSP.HSPEventListenerAttribute` for more.
Raising HSPEvents: 
```csharp
HSPEvent.EventManager.TryInvoke( event, data )
```
Listening to HSPEvents:
```csharp
[HSPEventListener(event, ...)]
private void ListenerMethod()
{
    // Method that doesn't take event parameters.
    // ...
}

[HSPEventListener(event, ...)]
private void ListenerMethod( data )
{
    // Method that takes event parameters.
    // ...
}
```
Commonly used events:
`HSPEvent_STARTUP_IMMEDIATELY`, `HSPEvent_<scene_name>_SCENE_LOAD`, etc.

#### Asset Handling:
HSP uses the `AssetRegistry` class for managing assets. Use it instead of trying to load the files by hand, or using Resources.Load, or other Unity methods.
The `AssetRegistry.Get<T>` can return both in-project assets (including outside the Resources directory) and file assets inside GameData.
GameData file asset loaders are in `HSP.Vanilla.Content.AssetLoaders`.

#### Prefabs:
Generally don't use prefabs outside of debugging purposes. 
A lot of the things HSP needs can't be (easily or at all) serialized by the Unity's serializer.
In the HSP-land, either create the gameobject yourself (using `var go = new Gameobject()`), or - if it's something that can be configurable - load it from json using `SerializationUnit.Deserialize<GameObject>( ... )` or `AssetRegistry.Get<GameObject>( ... )`.

#### ScriptableObjects:
Avoid using these altogether, use json assets in GameData instead. These can be exposed to the player and make it easier to mod the game.

#### Serialization:
Use `SerializationUnit.Serialize<T>( ... )` and `SerializationUnit.Deserialize<T>( ... )` instead of newtonsoft, Unity's builtin serialization, or other.
They can handle pausing and resuming mid-serialization (progress bars, etc), serializing gameobjects, etc.

#### UI and Layout:
Don't use the UGUI elements directly.
HSP uses UnityPlus' UILib to simplify creating UGUI UIs using code. The library uses modular and reusable controls for its elements, and also has a custom layout engine.



*This document is intended to be a living reference. Feel free to propose additions/changes!*

