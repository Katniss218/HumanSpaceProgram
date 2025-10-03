# Cheat Sheet - Graphics (Built-in RP)

## RenderTargetIdentifier:
It's a 'standard' handle that points to some specific GPU resource (like a texture/buffer/etc).
public struct RenderTargetIdentifier
- from a `Texture`
- from a `RenderTexture`
- from a `RenderBuffer`
- from a `BuiltinRenderTextureType`


## BuiltinRenderTextureType:
This is an alias to an engine-managed GPU resource.
These are not constant, expect them to point to different resources at different point in time.


PropertyName
BufferPtr
RenderTexture
BindableTexture

None

#### BuiltinRenderTextureType.CurrentActive
The last bound render target on the device context - "what's currently bound". Useful inside command buffers to refer to the target that was set previously.
Kinda sheit tbh.

#### BuiltinRenderTextureType.CameraTarget
The target texture for the currently rendering camera. If the camera renders to a RenderTexture - that RT, otherwise the backbuffer/swapchain surface.

// Camera depth/depth+normals texture
Depth
DepthNormals

//     Resolved depth buffer from deferred.
ResolvedDepth

//     Deferred lighting (normals+specular) G-buffer.
PrepassNormalsSpec

//     Deferred lighting light buffer.
PrepassLight

//     Deferred lighting HDR specular light buffer (Xbox 360 only).
PrepassLightSpec

//     Deferred shading G-buffer #0 (typically diffuse color).
GBuffer0

//     Deferred shading G-buffer #1 (typically specular + roughness).
GBuffer1

//     Deferred shading G-buffer #2 (typically normals).
GBuffer2

//     Deferred shading G-buffer #3 (typically emission/lighting).
GBuffer3

//     Reflections gathered from default reflection and reflections probes.
Reflections

//     Motion Vectors generated when the camera has motion vectors enabled.
MotionVectors

//     Deferred shading G-buffer #4 (typically occlusion mask for static lights if any).
GBuffer4

//     G-buffer #5 Available.
GBuffer5

//     G-buffer #6 Available.
GBuffer6

//     G-buffer #7 Available.
GBuffer7


`.Blit(...)` generates a fullscreen quad (or triangle, implementation may vary).
The coordinates are in world pos??
Store the active render target before you use Blit if you need to use it afterwards.
Blit binds the `source` as `_MainTex` in the blitting shader
Set `dest` to `null`. Unity now uses `Camera.main.targetTexture`
- sounds fragile

`GL.sRGBWrite` and linear color space


## RenderTextureFormat/TextureFormat vs GraphicsFormat
`RenderTextureFormat` and `TextureFormat` are both legacy, and `GraphicsFormat` is new, maps directly to DXGI (GPU) formats.
GraphicsFormat should be used. Constructors that accept the legacy formats seem to use a utility to convert to GraphicsFormat anyway.
DXT formats exist in the new enum too.

https://discussions.unity.com/t/render-texture-format-is-confusing/830175/2
> The `RenderTextureFormat` enum has been around for a long time. Over a decade now. It’s always been an incomplete list of “common” render texture formats, both in terms of those people might want to use and supported by GPUs. It also tried to be “human readable” for each of the render types in terms of the length of each enum. Over time it’s grown longer and longer as they’ve added more formats, both as they’ve needed them internally and as they’ve been requested by users, but it’s always been, and always will be an incomplete list.

> The `GraphicsFormat` enum is new, added for Unity 2018.2, and is part of the experimental APIs that were added when they were adding the Scriptable Render Pipeline related stuff. It seems to be intended to be a more complete list of all possible formats… though oddly it is still an incomplete list and missing some formats RenderTextureFormat has! It’s also shared between render textures and traditional (2D, 3D, Cube, 2DArray) textures, with most of the formats supported by both. Though all of the compressed formats at the end of the list (DXT, BC, PVRTC, ETC, EAC, & ASTC) cannot be used for a render texture and are for traditional textures only. I assume the goal is for the GraphicsFormat enum to eventually replace the RenderTextureFormat and TextureFormat enums entirely for script based texture creation. I would expect them to keep something like the later two around indefinitely for user facing UI lists, like for render texture assets and the texture importer, though the texture importer already uses an entirely custom list that’s even more limited than the internal TextureFormat enum. That plus it missing some of the special auto “formats” RenderTextureFormat has. Depth, Shadowmap, Default, & DefaultHDR specifically, which aren’t actually specific formats but change based on the rendering API, platform, and project settings.


## Premultiplied alpha

Premultiplied alpha refers to textures that have had their color channels "pre-multiplied" with the alpha channel at creation time.
So instead of the hole having color information, the hole is set to 0, and semitransparent edges around the hole set to the original albedo color * partial alpha

The most significant advantage of premultiplied alpha is that it allows for correct blending, interpolation, and filtering. Ordinary interpolation without premultiplied alpha leads to RGB information leaking out of fully transparent (A=0) regions

non-premultiplied alpha is useful for additive blending, since premultiplied will appear darker in semitransparent areas.


## Setting render targets:
```csharp
RenderTexture renderTex;
RenderBuffer[] colorBuffers;
RenderBuffer depthBuffer;

camera.SetTargetBuffers( renderTex.colorBuffer, renderTex.depthBuffer );
camera.SetTargetBuffers( colorBuffers, depthBuffer ); // Multiple Render Targets (MRT)
```

## Temporary Render Textures:

Get/release from within the command buffer. That way you don't need to recreate the buffer to get the new temporary.
```csharp
int id = Shader.PropertyToID( "_MyTempRT" ); // Or cache on startup.

//cmd.GetTemporaryRT( id, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.DefaultHDR );
cmd.GetTemporaryRT( id, new RenderTextureDescriptor( ... ) ); // RenderTextureDescriptor appears to be the least legacy one.
cmd.SetRenderTarget( id ) ; // or `new RenderTargetIdentifier( id )`
// Draw... Can use `Blit( id, ..., ... );` to bind the temp texture as _MainTex
cmd.ReleaseTemporaryRT( id );
```


```csharp
int id = Shader.PropertyToID( "_Input1Depth" ); // Or cache on startup.

material.SetTexture( id, depthRenderTex, RenderTextureSubElement.Depth );
commandBuffer.SetGlobalFloat( id, flag ? 1.0f : 0.0f );
```

## Draw Calls:

```csharp
// _MainTex - set to the 'source' parameter in Blit, like using SetGlobalTexture



// Draws a Renderer object, using some of that renderer's state.
cmd.DrawRenderer( (Renderer)(object)volumeMeshRenderer, material, subMeshIndex, pass );

cmd.SetRenderTarget( flag ? flipRaysRenderTextures : flopRaysRenderTextures, renderTex.depthBuffer );

```

RenderBufferLoadAction
RenderBufferStoreAction
- Resolve (MSAA) sounds interesting

Cull Off // turn off backface culling
Tags { "RenderType"="Opaque" }

Stencil
{
    Ref 1 // check for value `1`
    Comp Always
    Pass Replace
}  
Stencil
{
    Ref 1
    Comp Equal
    ReadMask 35
    Pass Keep
}