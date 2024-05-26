using UnityPlus.Serialization;

namespace KSS.Core.Mods
{
    public static class NamespacedIdentifier_Serialization
    {
        [SerializationMappingProvider( typeof( NamespacedIdentifier ) )]
        public static SerializationMapping NamespacedIdentifierMapping()
        {
            return new PrimitiveStructSerializationMapping<NamespacedIdentifier>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString(),
                OnInstantiate = ( data, l ) => NamespacedIdentifier.Parse( (string)data )
            };
        }
    }
}