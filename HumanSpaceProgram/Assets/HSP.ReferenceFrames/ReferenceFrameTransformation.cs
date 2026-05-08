namespace HSP.ReferenceFrames
{
    /// <summary>
    /// Represents a cached or reusable chain of transformations between two reference frames.
    /// This resolves the Lowest Common Ancestor (LCA) and traces up the hierarchy and back down.
    /// </summary>
    public struct ReferenceFrameTransformation
    {
        public bool IsValid => true; // TODO: Implement version validation

        public KinematicState Apply( in KinematicState localState )
        {
            // TODO: Execute LCA chain transforms
            return localState;
        }

        public static ReferenceFrameTransformation GetTransformation( IReferenceFrame from, IReferenceFrame to )
        {
            // TODO: Generate sequence tracing up to LCA and down to target
            return new ReferenceFrameTransformation();
        }
    }
}
