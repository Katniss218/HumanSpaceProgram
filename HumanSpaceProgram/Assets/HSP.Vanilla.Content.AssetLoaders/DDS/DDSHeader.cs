using System.IO;

namespace HSP.Vanilla.Content.AssetLoaders.DDS
{
    /// <summary>
    /// The DDS header.
    /// </summary>
    internal struct DDSHeader
    {
        // https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dds-header

        public uint dwSize;
        public DDSD dwFlags;
        public uint dwHeight;
        public uint dwWidth;
        public uint dwPitchOrLinearSize;
        public uint dwDepth;
        public uint dwMipMapCount;
        public uint[] dwReserved1; // 11 uints long
        public DDPixelFormat ddpfPixelFormat;
        public DDCaps ddsCaps;
        public uint dwReserved2;

        public DDSHeaderDX10? dx10;

        public DDSHeader( BinaryReader reader )
        {
            dwSize = reader.ReadUInt32();
            dwFlags = (DDSD)reader.ReadUInt32();
            dwHeight = reader.ReadUInt32();
            dwWidth = reader.ReadUInt32();
            dwPitchOrLinearSize = reader.ReadUInt32();
            dwDepth = reader.ReadUInt32();
            dwMipMapCount = reader.ReadUInt32();
            dwReserved1 = new uint[11];
            for( int i = 0; i < 11; i++ )
            {
                dwReserved1[i] = reader.ReadUInt32();
            }
            ddpfPixelFormat = new DDPixelFormat( reader );
            ddsCaps = new DDCaps( reader );
            dwReserved2 = reader.ReadUInt32();
            dx10 = ddpfPixelFormat.dwFourCC == FourCC.DX10 ? new DDSHeaderDX10( reader ) : null;
        }
    }
}