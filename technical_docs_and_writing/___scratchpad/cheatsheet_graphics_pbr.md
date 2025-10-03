# Cheat Sheet - Graphics - PBR

textures are either rgb mostly for percentage-based values, or HDR for luminances

RGB albedo is the percentage of each (R, G, and B) color being reflected. not luminance.

emissives are HDR

BSDF = BRDF + BTDF

BRDFs:

Diffuse:
- Lambert
- Oren-Nayar
- Burley (disney)

Specular:
- Phong
- Blinn-Phong
- Cook-Torrance (microfacets) - uses a microfacet distribution function, like GGX


https://schuttejoe.github.io/post/disneybsdf/
https://github.com/GarrettGunnell/Disney-PBR

https://github.com/wdas/brdf




