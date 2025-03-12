# Neoserialization v3

    This document is not strictly documentation. It hopefully should be close, but I'm not perfect and sometimes can't be bothered to update every doc.
#

Serialization operates on Serialization Units, each of which can contain an arbitrary number of objects to serialize/deserialize.
Each unit has an underlying type that all objects being saved/loaded must derive from.

The serialization itself is performed by a loader/saver in conjunction with 'mappings'
A mapping describes how to convert an object to serializeddata and vice versa.

The loader's job is to store the reference map, and invoke each mapping until every object returns Finished, or the max number of iterations has been achieved.

## Serialization Units:

The serialization unit represents a 'package' of data that is serialized/deserialized together.
Objects within the serialization unit can always reference each other.
Different serialization units technically can reuse the same reference map, but this is discouraged, unless the map is preserved and reused for the lifetime of the application.

When `SerializationUnit.FromObjects( ... )`, `FromData( ... )` or any of their shorthands is invoked, the objects/data passed in the arguments form the serialization unit.

## Serialization Mappings:

The serialization mapping is what actually controls how a type is serialized/deserialized.
There are 4 types of mappings:
1. Primitive
    The object is serialized in one go, and either succeeds, or fails.
2. Memberwise
    The object is serialized as a sum of its members. Each member may fail or succeed individually. This is iterated on members that failed until the entire object is constructed.
3. Indexed
    Like memberwise, but for types that are indexable collections (arrays/lists/etc)
4. Enumerated
    Like indexed, but for collections whoose elements aren't accessible via an index.

#### Primitive:

Primitive mappings can produce mappings of any arbitrary form.

#### Memberwise:

Memberwise mappings produce objects of the following form:
```json
{
  "$id": "00000000-0000-0000-000000000000",
  "$type": "assembly_qualified_type",
  "member1_name": "member1_value",
  "member2_name": "member2_value"
  // ...
}
```

#### Indexed/Enumerated:

Indexed and enumerated mappings produce objects of the following form:
```json
{
  "$id": "00000000-0000-0000-000000000000",
  "$type": "assembly_qualified_type",
  "value": 
  [
    "element1_value",
    "element2_value"
    // ...
  ]
}
```

## Serialization Mapping Providers:

They're used to tell the mapping registry which types a given mapping should be used for.

## Contexts:

There may be multiple mappings applicable to a given type (and defined **for** the given type).
To tell the serializer which one you want to use, you can specify an integer 'context'.
The default context (if not specified) is always `0`.

## Reference Maps:

Reference maps are used to resolve references between objects.

## Data Handlers:

Data Handlers are used to translate SerializedData into a file.

## Error Handling:

When a type is missing (not found in any of the loaded assemblies), the member/element/etc which the type refers to will be left default (usually null or 0)
This also extends to GameObjects - components of missing type will be omitted.

==================
==================
==================
==================

# Key points to improve in neoser 3.0:

- ASYNC ASYNC ASYNC - each type's serialization has to be interruptible. memberwise might stop between members and primitives will have to accomodate directly.
- immutable types requiring a manual serialization func
- generic types with immutable type arguments are also annoying.
- de/serializing members inside manual serialization being clunky.

- there are missing mappings and stuff.


# PROTOTYPING:

for manual mappings, add a way to convert an object to a data and vice versa in a single line.



'Primitive' mappings are now the mappings that can't yield in the middle. They generally would also be lower level.
- all generic types should be yieldable, since the generic member can be a massive monster.

## IDEA: Load members of any object in any order.

If Load fails on some member, store that member and retry it the next time Load is called.
- In theory it should never fail circularly (requirement is: A refs B, B refs A, both are immutable)
- A member can't be a reference to itself, since a reference is always a leaf.


if nothing changed or everything was fully completed, we can stop loading.
- the system needs to determine whether the reference is missing completely or not instantiated yet.


## TODOs:

add mappings for the rest of the types

error surfacing

handle aborts when deserializing types that are disposable (e.g. gameobjects)
- by disposable I mean types that can't just be forgotten about and left for the GC to clean up
- loader may store information if any errors occurred, and/or how to handle them.
  - a list of all 'errors' as IError instances, that can be shown to the user, or handled, somehow.
- maybe a callback in the mapping for 'onfailure' or 'cleanup' or something




