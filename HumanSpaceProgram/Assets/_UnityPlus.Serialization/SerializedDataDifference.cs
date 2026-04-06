namespace UnityPlus.Serialization
{
    /// <summary>
    /// Represents the kind of difference found between two <see cref="SerializedData"/> nodes.
    /// </summary>
    public enum SerializedDataDifferenceKind
    {
        /// <summary>
        /// The values of two <see cref="SerializedPrimitive"/> nodes do not match.
        /// </summary>
        ValueMismatch,
        /// <summary>
        /// The types of the two <see cref="SerializedData"/> nodes do not match (e.g. one is an object, the other is an array).
        /// </summary>
        TypeMismatch,
        /// <summary>
        /// A child element exists in the first tree (A) but is missing in the second (B).
        /// </summary>
        MissingInB,
        /// <summary>
        /// A child element exists in the second tree (B) but is missing in the first (A).
        /// </summary>
        MissingInA
    }

    /// <summary>
    /// Represents a single difference found between two <see cref="SerializedData"/> trees.
    /// </summary>
    public sealed class SerializedDataDifference
    {
        /// <summary>
        /// The path to the node where the difference was found.
        /// </summary>
        public SerializedDataPath Path { get; }
        /// <summary>
        /// The node in the first tree (A).
        /// </summary>
        public SerializedData ValueA { get; }
        /// <summary>
        /// The node in the second tree (B).
        /// </summary>
        public SerializedData ValueB { get; }
        /// <summary>
        /// The kind of difference.
        /// </summary>
        public SerializedDataDifferenceKind Kind { get; }

        public SerializedDataDifference( SerializedDataPath path, SerializedData valueA, SerializedData valueB, SerializedDataDifferenceKind kind )
        {
            Path = path;
            ValueA = valueA;
            ValueB = valueB;
            Kind = kind;
        }

        public override string ToString()
        {
            return $"[{Kind}] at '{Path}': '{ValueA?.ToString() ?? "null"}' vs '{ValueB?.ToString() ?? "null"}'";
        }
    }
}