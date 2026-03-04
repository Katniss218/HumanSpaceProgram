namespace UnityPlus.Serialization
{
    /// <summary>
    /// Determines the serialization context for a child element based on its properties.
    /// </summary>
    public interface IContextSelector
    {
        ContextKey Select( ContextSelectionArgs args );
    }
}