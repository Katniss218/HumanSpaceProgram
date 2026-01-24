using System;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// Represents a temporary, read-only collection of substances, typically from a sampling operation.
    /// </summary>
    /// <remarks>
    /// This interface signals that the object is likely pooled and MUST be disposed via a 'using' block.
    /// </remarks>
    public interface ISampledSubstanceStateCollection : IReadonlySubstanceStateCollection, IDisposable
    {
    }
}