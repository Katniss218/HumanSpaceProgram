using System;

namespace HSP.Vanilla.Content.AssetLoaders.DDS
{
    /// <summary>
    /// Flags to indicate which members contain valid data.
    /// </summary>
    [Flags]
    public enum DDSD : uint
    {
        // https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dds-header
        CAPS = 0x1u,
        HEIGHT = 0x2u,
        WIDTH = 0x4u,
        PITCH = 0x8u,
        BACKBUFFERCOUNT = 0x20u,
        ZBUFFERBITDEPTH = 0x40u,
        ALPHABITDEPTH = 0x80u,
        LPSURFACE = 0x800u,
        PIXELFORMAT = 0x1000u,
        CKDESTOVERLAY = 0x2000u,
        CKDESTBLT = 0x4000u,
        CKSRCOVERLAY = 0x8000u,
        CKSRCBLT = 0x10000u,
        MIPMAPCOUNT = 0x20000u,
        LINEARSIZE = 0x80000u, // Required when pitch is provided for a compressed texture.
        DEPTH = 0x800000u,

        TEXTURE = CAPS | HEIGHT | WIDTH | PIXELFORMAT
    }
}