
namespace UnityPlus.Serialization
{
    // TODO - maybe use this in places other than the attribute?
    //public struct SerializationContext
    //{
    //    readonly int _value;
    //}

    // These must be `int` because attribute parameters can't be custom structs.

    /// <summary>
    /// General contexts applicable to any object type.
    /// </summary>
    public static class ObjectContext
    {
        public const int Default = 0;

        public const int Value = Default;
        public const int Ref = 536806356;
        public const int Asset = 271721118;
    }

    /// <summary>
    /// Also used for any sort of "flat" collection.
    /// </summary>
    public static class ArrayContext
    {
        public const int Default = ObjectContext.Default;

        public const int Values = Default;
        public const int Refs = 429303064;
        public const int Assets = -261997342;
    }

    /// <summary>
    /// Also used for any sort of key-value collection.
    /// </summary>
    public static class KeyValueContext
    {
        public const int Default = ObjectContext.Default;

        public const int ValueToValue = Default;
        public const int ValueToRef = 846497468;

        public const int RefToValue = 132031121;
        public const int RefToRef = 240733231;

        public const int ValueToAsset = 172983851;
        public const int RefToAsset = -526574830;
    }
}