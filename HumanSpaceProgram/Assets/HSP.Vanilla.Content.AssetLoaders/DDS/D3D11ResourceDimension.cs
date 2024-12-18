namespace HSP.Vanilla.Content.AssetLoaders.DDS
{
    public enum D3D11ResourceDimension : uint
    {
        // https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_resource_dimension
        UNKNOWN = 0u,
        BUFFER = 1u,
        TEXTURE1D = 2u,
        TEXTURE2D = 3u,
        TEXTURE3D = 4u
    }
}