
namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings_Assets
    {
        [MapsInheritingFrom( typeof( object ), Context = ObjectContext.Asset )]
        public static SerializationMapping ObjectAssetMapping<T>() where T : class
        {
            return new PrimitiveSerializationMapping<T>( skipHeader: true )
            {
                OnSave = ( o, s ) => s.RefMap.WriteAssetReference<T>( o ),
                OnLoad = ( data, l ) => l.ReadAssetReference<T>( data )
            };
        }
    }
}