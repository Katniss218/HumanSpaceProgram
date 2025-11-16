namespace HSP.ResourceFlow
{
    /// <summary>
    /// Represents an object that can hold resources (substances).
    /// </summary>
    public interface IResourceContainer : IHasMass
    {
#warning TODO - move to vanilla / somewhere. has no purpose in flow anymore. only used for UI.
        /// <summary>
        /// The maximum volumetric capacity of this container.
        /// </summary>
        float MaxVolume { get; }

        /// <summary>
        /// The current contents of this container.
        /// </summary>
        SubstanceStateCollection Contents { get; }
    }
}