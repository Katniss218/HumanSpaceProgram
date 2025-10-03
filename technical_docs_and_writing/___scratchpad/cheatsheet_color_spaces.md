
in sRGB color space the GPU and rendering pipeline operate using sRGB values.

in linear color space, the GPU [...] using linear color samples.

this requires all textures to be marked appropriately.
If a texture is marked as sRGB it will be converted to linear when sampled by the shader.

This is done by selecting the appropriate `GraphicsFormat` for the texture.



## Deprecated utils:
can also be done by using the `bool linear` parameter in Texture2D constructors
internally it converts to graphicsformat anyway though.

Utils for converting to GraphicsFormat
`GraphicsFormatUtility.IsSRGBFormat()`
`GraphicsFormatUtility.GetSRGBFormat()`
`GraphicsFormatUtility.GetGraphicsFormat(textureformat, is_sRGB)`

## Texture Formats:

UNorm - Unsigned normalized integer. Stored as an unsigned int, mapped to [0, 1] when read in shaders.
SNorm - Signed normalized integer. Stored as signed int, mapped to [-1, 1].
UInt / SInt - Unsigned / signed integer. Interpreted as integers in shaders, not normalized.
SFloat / UFloat - Floating-point storage. SFloat is the normal IEEE754 float.
SRGB - like UNorm, but with values in sRGB space (gamma corrected)
SHalf

#### Integer Formats

Examples:

R32_SInt - 32-bit signed integer per pixel.

R32G32B32A32_UInt - 4×32-bit unsigned integers.

Used for compute shaders and exact data storage (not colors).

SFloat - Used for HDR rendering, physical simulation, high-precision data.





