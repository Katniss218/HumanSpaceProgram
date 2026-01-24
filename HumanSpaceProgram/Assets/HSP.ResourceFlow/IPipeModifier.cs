namespace HSP.ResourceFlow
{
    /// <summary>
    /// Represents a modification that can be applied to a FlowPipe's properties during the SynchronizeState step.
    /// </summary>
    public interface IPipeModifier
    {
        /// <summary>
        /// Applies the modification to the given pipe.
        /// </summary>
        /// <param name="pipe">The simulation pipe object to modify.</param>
        void Apply( FlowPipe pipe );
    }
}