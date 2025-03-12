# Celestial System

    This document is not strictly documentation. It hopefully should be close, but I'm not perfect and sometimes can't be bothered to update every doc.
#


cb's are loaded from json as gameobjects in saves

when starting a new save something needs to make the system



celestial bodies keplerian orbits need to work



surface colliders need a separate origin



celestial bodies are spawned using the factory

on creation a trajectory transform component is added.

celestial bodies are deserialized the same as vessels, as normal gameobjects.
- their surfaces are instantiated later using the data from the deserialized stuff.


vessels are spawned using factory

on creation, a trajectory transform component is added.


we can use after created event to assign the trajectory of the vessel based on its pos/vel

for celestialbodies, we basically want it reversed


celestials make sense to use trajectory because trajectory is what specifies the initial stuff

celestials would also make sense to be hierarchical though...
even if they don't use their parent body for anything after the fact, it makes at least some sense to define it.
newtonian orbits could then specify the initial pos/vel in parent space, as this would be a lot easier as well.



do I want the solar system definition to match the serialized cbs? probably...

vessels at the start should probably specify their position relative to a cb as well


basically I would split it into 2 parts

the actual SAVE that is loaded when you start a game

the script that creates that save.


if you want to make a new celestial system, you should also provide a starting point that uses it, otherwise the game won't know what to do with it.


adding bodies in order kind of makes sense, but the user should ensure that order.
- that would also ensure that the position is correct, as parent would be processed before child

**So I'm gonna just make sure the cb's are spawned in the right order during whatever that creates the starting scenario**


we want to support cb creation at runtime


cb's should rotate

there should be a directory for the starting saves, inside gamedata





--------------

pinned:


















