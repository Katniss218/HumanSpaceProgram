using System;

namespace HSP.Vanilla.Content.AssetLoaders.DDS
{
    [Flags]
    public enum DDPF : uint
    {
        // https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dds-pixelformat
        // https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/ksmedia/ns-ksmedia-_ddpixelformat
        // https://www.pilotlogic.com/codetyphon/webhelp/pl_asphyrepxl/pl_asphyrepxl/pxl.windows.ddraw/index-8.html
        /// <summary>
        /// The surface has alpha channel information in the pixel format.
        /// </summary>
        ALPHAPIXELS = 0x1u,
        /// <summary>
        /// The pixel format describes an alpha-only surface.
        /// </summary>
        ALPHA = 0x2u,
        FOURCC = 0x4u,
        PALETTEINDEXED1 = 0x800u,
        PALETTEINDEXED2 = 0x1000u,
        PALETTEINDEXED4 = 0x8u,
        PALETTEINDEXED8 = 0x20u,
        PALETTEINDEXED8A = PALETTEINDEXED8 | ALPHAPIXELS,
        PALETTEINDEXEDTO8 = 0x10u,
        COMPRESSED = 0x80,
        ZBUFFER = 0x400u,
        DDPF_STENCILBUFFER = 0x4000u,
        DDPF_ALPHAPREMULT = 0x8000u,
        DDPF_ZPIXELS = 0x2000,
        RGB = 0x40u,
        RGBA = RGB | ALPHAPIXELS,
        YUV = 0x200u,
        RGBTOYUV = 0x100u,
        LUMINANCE = 0x20000u,
        LUMINANCEA = LUMINANCE | ALPHAPIXELS,
        BUMPLUMINANCE = 0x40000u,
        BUMPDUDV = 0x80000u,
        BUMPDUDVA = BUMPDUDV | ALPHAPIXELS,
        DDPF_D3DFORMAT = 0x200000u,
    }
}