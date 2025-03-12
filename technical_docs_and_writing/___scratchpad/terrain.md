# Terrain

    This document is not strictly documentation. It hopefully should be close, but I'm not perfect and sometimes can't be bothered to update every doc.
#

## LODQuadSphere

The LODQuadSphere is the component that, when added to a celestial body, creates a deformable surface around it. Conceptually analogous to KSP's PQS system.

A LODQuadSphere can serve as a visual surface, a collider, or both, depending on its configuration.

## Computational Backend:

### Modifiers:

Mesh generation for the LOD Quad Sphere is driven by a sequence of Modifiers (which are conceptually analogous to KSP's PQS Mods).
These modifiers are executed off the main thread, leveraging the power of parallel processing.

Modifiers are executed in distinct stages, with each stage taking one frame to complete.
The system ensures synchronization between quads across stages, enabling each stage to utilize data from neighboring quads produced in the preceding stage. 
This approach is what enables the seamless quad borders, among other things.

### LOD detail:

The LODQuadTree functions as the backend computational structure underlying the terrain system.
The LOD adjustment calculations are performed on it, the results of which (accessible through the LODQuadTreeChanges structure) are then passed to the LODQuadRebuilder.

The rebuilder is what actually executes the 'modifiers' to generate the meshes used by the renderers and colliders.

## Result Frontend:

Each built quad (mesh) has a corresponding LODQuad monobehaviour which serves as an interface and encapsulates it.
It can be toggled on or off, allowing caching of the meshes if desired.

The quads are parented to an object that is always positioned near the scene origin.
This ensures that the colliders work correctly and the quads don't flicker due to lack of floating-point precision.


### TODOS:

- Triangle splits based on the normal angles
- Discard triangles (and vertices?) based on some condition (ultimately texture-based?)
        this will be useful for 'caves' and for removing the ocean from under the landmasses.



## CB assets

having JSON specified assets would be nice (also basically required for a lot of things).
load multiple textures as a cubemap
load textures as sprites (using additional json)

it would be nice if the textures were unloaded when the cb is too far away to be rendered, or if they were even swapped out for lighter weight ones or something

asset is registered if the filename suffix matches one of the known suffixes

cubemaps need to match 6 sides to register.
the json-defined asset is registered under the name of the suffixed file (i.e. e.g. "vanilla::assets/earth_color_cubemap")
the 6 faces are still registered separately under "vanilla::assets/earth_color_xn" etc.
the cubemap asset could be then assigned to a 'meta' asset that depends on the quality level or something, meaning you could dynamically switch texture resolutions in the settings.


1 problem remains however. the type of the texture is not valid for materials. I.e. they can't be deserialized in-place like that.
but that doesn't really fit, as the thing being deserialized wouldn't be a material, since it has 6 materials not 1. kinda weird.

the names of the properties can't be hardcoded, because I want the material to be customizable.

**so basically, the deserializer will need to create an array of 6 materials and assign the same values to each, except for entries that can deserialize as cubemaps?**

the material itself can be inline or a standalone json asset.


```csharp
        [MapsInheritingFrom( typeof( Material[] ), Context = CUBE_MATERIAL )]
        public static SerializationMapping CubeMaterialMapping()
        {
            return new MemberwiseSerializationMapping<Material[]>()
            {
                ("floats", new Member<Material[], Dictionary<string, float>>( KeyValueContext.ValueToValue,
                    o => o[0].shader.pro,
                    (o, value) => 
                    {
                        foreach( var oo in o ) 
                            foreach( var kvp in value )
                                oo.SetFloat( Shader.PropertyToID( kvp.Key ), kvp.Value ); 
                    }
                    ) ),
#warning TODO - add ability to do unknown parameter types?
            };
        }
```


```json
{
    "$type": "UnityEngine.Material[], UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
    "textures":
    {
        "_MainTex": { "$assetref": "Vanilla::Assets/earth_color" },
        "_NormalTex": { "$assetref": "Vanilla::Assets/earth_normal" }
    },
    "floats":
    {
        "_Glossiness": 0.05,
        "_NormalStrength": 0.5
    }
}
```