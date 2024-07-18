Naming scheme broadly follows the idea of "what's more closely related".

**Choosing the appropriate namespace/subsystem:**

1. Choose a namespace based on the primary relationship of its contents.
    E.g. vessels are being included as content, `so HSP.Content.Vessels` is preferred over `HSP.Vessels.Content`, 
    as the namespace directly contains content, but only relates to vessels.

2. The `HSP.Vanilla.xyz` subnamespace is for concrete implementations. 
    The game should still compile and run if you remove everything inside `HSP.Vanilla.xyz`, and replace it by modded variants.

3. If a component is not directly related to the core functionality of the namespace, but is an implementation of it, it should probably be moved to `HSP.Vanilla.Components`.