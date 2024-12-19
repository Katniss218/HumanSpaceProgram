
namespace HSP.Vanilla.Content.AssetLoaders.DDS
{
    public enum D3D11Misc : uint
    {
        // https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_resource_misc_flag
        GENERATE_MIPS = 0x1u,
        SHARED = 0x2u,
        TEXTURECUBE = 0x4u,
        DRAWINDIRECT_ARGS = 0x10u,
        BUFFER_ALLOW_RAW_VIEWS = 0x20u,
        BUFFER_STRUCTURED = 0x40u,
        RESOURCE_CLAMP = 0x80u,
        SHARED_KEYEDMUTEX = 0x100u,
        GDI_COMPATIBLE = 0x200u,
        SHARED_NTHANDLE = 0x800u,
        RESTRICTED_CONTENT = 0x1000u,
        RESTRICT_SHARED_RESOURCE = 0x2000u,
        RESTRICT_SHARED_RESOURCE_DRIVER = 0x4000u,
        GUARDED = 0x8000u,
        TILE_POOL = 0x20000u,
        TILED = 0x40000u,
        HW_PROTECTED = 0x80000u,
    }
}