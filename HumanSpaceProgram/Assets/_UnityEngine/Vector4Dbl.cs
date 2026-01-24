namespace UnityEngine
{
    public struct Vector4Dbl
    {
        public double x, y, z, w;
        public Vector4Dbl( double x, double y, double z, double w )
        {
            this.x = x; 
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public override string ToString() => $"({x}, {y}, {z}, {w})";
    }
}