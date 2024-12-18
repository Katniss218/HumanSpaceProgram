using System.IO;

namespace HSP.Vanilla.Content.AssetLoaders.DDS
{
    /// <summary>
    /// The 'capabilities' structure.
    /// </summary>
    internal struct DDCaps
    {
        public DDSCaps dwCaps;
        public DDSCaps2 dwCaps2;
        public uint dwCaps3;
        public uint dwCaps4;

        public DDCaps( BinaryReader reader )
        {
            dwCaps = (DDSCaps)reader.ReadUInt32();
            dwCaps2 = (DDSCaps2)reader.ReadUInt32();
            dwCaps3 = reader.ReadUInt32();
            dwCaps4 = reader.ReadUInt32();
        }
    }
}