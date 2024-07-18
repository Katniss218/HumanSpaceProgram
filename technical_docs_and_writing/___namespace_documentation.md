Naming scheme broadly follows the idea of "what's more closely related".

**Choosing the appropriate namespace/subsystem:**

1. When choosing between e.g. `HSP.Vessels.Content` and `HSP.Content.Vessels`, you should first ask yourself whether the objects your namespace contains are more related to vessels, or to assets. 
In this case, vessels are being included as content (the namespace directly contains content), so `HSP.Content.Vessels` is preferred.

2. Integrations of subsystems follow e.g. `HSP.UI.Vessels` - contains UIs (for vessels), if it contained vessels (for UIs lolwut) then the segments would've been flipped around.

3. If you can have many different types of a component, then it is said to be "softly tied" to the given namespace, and should be moved to its appropriate Vanilla namespace. E.g. `FResourceContainer` may have different types (spherical, cylindrical, CFD, etc).
The base namespace needs an interface in that case, which will specify what the derived objects need to do to function within the system.
If it doesn't make sense to have different types of a component, then it should remain inside the subsystem, e.g. `FPart` or `FAttachNode`.

4. The `HSP.Vanilla.XYZ` subnamespace is for concrete implementations of things from `HSP.XYZ`. 
Or more specifically - for implementations that can be overriden by mods.