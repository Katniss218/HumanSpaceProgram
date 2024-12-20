using System.IO;

namespace HSP.Vanilla.Content.AssetLoaders.DDS
{
    /// <summary>
    /// The 'pixel format' structure.
    /// </summary>
    internal struct DDPixelFormat
    {
        public uint dwSize;
        public DDPF dwFlags;
        public uint dwFourCC;
        public uint dwRGBBitCount;
        public uint dwRBitMask;
        public uint dwGBitMask;
        public uint dwBBitMask;
        public uint dwRGBAlphaBitMask;

        public DDPixelFormat( BinaryReader reader )
        {
            dwSize = reader.ReadUInt32();
            dwFlags = (DDPF)reader.ReadUInt32();
            dwFourCC = reader.ReadUInt32();
            dwRGBBitCount = reader.ReadUInt32();
            dwRBitMask = reader.ReadUInt32();
            dwGBitMask = reader.ReadUInt32();
            dwBBitMask = reader.ReadUInt32();
            dwRGBAlphaBitMask = reader.ReadUInt32();
        }
    }
}