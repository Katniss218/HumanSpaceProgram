namespace HSP.Vanilla.Content.AssetLoaders.OBJ
{
    /// <summary>
    /// Holds a (v,vt,vn) triplet.
    /// </summary>
    internal struct OBJVertex
    {
        /// <summary>
        /// The index into the vertex position array.
        /// </summary>
        public int v;
        /// <summary>
        /// The index into the texture coordinate (UV) array.
        /// </summary>
        public int vt;
        /// <summary>
        /// The index into the vertex normal array.
        /// </summary>
        public int vn;

        public override bool Equals( object obj )
        {
            if( obj is not OBJVertex other )
            {
                return false;
            }

            return v == other.v && vt == other.vt && vn == other.vn;
        }

        public override int GetHashCode()
        {
            return v.GetHashCode() ^ vt.GetHashCode() ^ vn.GetHashCode();
        }
    }
}
