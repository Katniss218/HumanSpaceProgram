using System.IO;

namespace HSP.Vanilla.Content.AssetLoaders.DDS
{
    /// <summary>
    /// DX10 header extension
    /// </summary>
    public struct DDSHeaderDX10
    {
        public DXGIFormat dxgiFormat;
        public D3D11ResourceDimension resourceDimension;
        public D3D11Misc miscFlag; // see D3D11_RESOURCE_MISC_FLAG
        public uint arraySize;
        public D3D11Misc2 miscFlags2; // see DDS_MISC_FLAGS2

        public DDSHeaderDX10( BinaryReader reader )
        {
            dxgiFormat = (DXGIFormat)reader.ReadUInt32();
            resourceDimension = (D3D11ResourceDimension)reader.ReadUInt32();
            miscFlag = (D3D11Misc)reader.ReadUInt32();
            arraySize = reader.ReadUInt32();
            miscFlags2 = (D3D11Misc2)reader.ReadUInt32();
        }
    };
}