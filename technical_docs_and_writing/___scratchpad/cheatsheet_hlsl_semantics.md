# Cheat Sheet - HLSL Semantics

## List of semantics:
https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics

#### Vertex buffer elements (convention):
`POSITION`, `POSITION0` - type `float3/4`
- input mesh vertex position
`NORMAL`, `NORMAL0` - type `float3/4`
- input mesh vertex normal
`TANGENT` - type `float3/4`
- input mesh vertex tangents
`TEXCOORD0..7` type `float1/2/3/4`
- input mesh vertex uv
- Generic float interpolator (useful for passing arbitrary data between vertex and fragment shaders).
`COLOR`, `COLOR0` - type `float4`
- input mesh vertex color
bones and indices



`SV_Position` - type `float4`
- Special semantic. Tells the rasterizer where to rasterize the fragments (where the triangle vertices are).

`SV_Target` - type `fixed4`/`float4`
- Writes to the color buffer from the fragment shader (forward path).
`SV_Target0..7`
- MRT / Deferred
`SV_Depth`
- Writes to the depth buffer from the fragment shader.
`SV_DepthGreaterEqual`
`SV_DepthLessEqual`
- like SV_Depth. Using this disables Z-culling? Only writes depth if it is less than the value in the depth buffer

`SV_InstanceID` - type `uint`
- index of the object in instanced draws
`SV_VertexID` - type `uint`
- index of the vertex in the draw call/mesh (#pragma target 3.5)

`SV_ClipDistance` - type array?

`SV_IsFrontFace` - type `bool`
- input fragment face orientation (true = faces camera)

`SV_Coverage`
- related to VRS (variable rate shading)

`SV_Barycentrics` - type `float3`
- barycentric coordinates of the fragment within the pixel, DC12+


## Where to use:
Vertex shader input:
- `POSITION`, `NORMAL`, `SV_VertexID`, ...
Vertex Shader Output:
- `SV_Position`, ...

Fragment shader input:
- `SV_Position`, `SV_Barycentrics`, ...

Fragment shader output:
- `SV_Target`, `SV_Target0..7`, `SV_Depth`, ...
> Pixel shaders can only write to parameters with the SV_Depth and SV_Target system-value semantics.


## Caveats and info:

#### Old/obsolete semantics:
`VFACE` - type `fixed`
- USE `SV_IsFrontFace` instead.
`COLOR`
- use `SV_Target` instead.
`DEPTH`
- use `SV_Depth` instead.
`VPOS`
- use `SV_Position` instead.

## List of functions:
https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-intrinsic-functions


#### Interpolator count limits
- 8 - usually on mobile
- 32+ - desktop (#pragma target 4.0)

