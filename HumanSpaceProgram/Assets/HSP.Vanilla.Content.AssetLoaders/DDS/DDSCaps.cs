using System;

namespace HSP.Vanilla.Content.AssetLoaders.DDS
{
    [Flags]
    public enum DDSCaps : uint
    {
        // https://github.com/JBontes/Life32/blob/master/DDraw.pas
        _3DDEVICE = 0x1u,
        ALPHA = 0x2u,
        BACKBUFFER = 0x4u,
        /// <summary>
        /// Optional. Must be used on any file that contains more than one surface (a mipmap, a cubic environment map, or mipmapped volume texture).
        /// </summary>
        COMPLEX = 0x8u,
        FLIP = 0x10u,
        FRONTBUFFER = 0x20u,
        OFFSCREENPLAIN = 0x40u,
        OVERLAY = 0x80u,
        PALETTE = 0x100u,
        PRIMARYSURFACE = 0x200u,
        PRIMARYSURFACELEFT = 0x400u,
        SYSTEMMEMORY = 0x800u,
        TEXTURE = 0x1000u,
        VIDEOMEMORY = 0x4000u,
        VISIBLE = 0x8000u,
        WRITEONLY = 0x10000u,
        ZBUFFER = 0x20000u,
        OWNDC = 0x40000u,
        LIVEVIDEO = 0x80000u,
        HWCODEC = 0x100000u,
        MODEX = 0x200000u,
        MIPMAP = 0x400000u,
        ALLOCONLOAD = 0x4000000u,
        VIDEOPORT = 0x8000000u,
        LOCALVIDMEM = 0x10000000u,
        NONLOCALVIDMEM = 0x20000000u,
        STANDARDVGAMODE = 0x40000000u,
        OPTIMIZED = 0x80000000u,
    }
}