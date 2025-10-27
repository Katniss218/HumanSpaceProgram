using System;
using System.Linq;
using UnityPlus.Serialization;

namespace HSP.Content.Migrations
{
    public static class MigrationUtility
    {
        public static void RenameType( ref SerializedData data, string from, Type to )
        {
            if( data is SerializedObject dict )
            {
                if( dict.TryGetValue( "$type", out var typeEntry ) && typeEntry.Equals( from ) )
                {
                    dict["$type"] = to.AssemblyQualifiedName;
                }

                foreach( var key in dict.Keys.ToList() )
                {
                    var entry = dict[key];
                    RenameType( ref entry, from, to );
                    dict[key] = entry;
                }
            }
            else if( data is SerializedArray list )
            {
                for( int i = 0; i < list.Count; i++ )
                {
                    var entry = list[i];
                    RenameType( ref entry, from, to );
                    list[i] = entry;
                }
            }
        }

        [Obsolete( "not implemented yet" )]
        public static void RenameField( ref SerializedData data, string typeName, string fromField, string toField )
        {
#warning TODO - type might be implicit, this is dangerous without the schema for the previous save.
            // a schema for a version is the same as a serialization mapping set for those versions.

            // "schema migrations"? (take existing serialization mapping and transform it back until we have the mapping that was used for the version we want to migrate from)

            // alternatively, allow providing custom full schemas?

            // alternatively (best?) use a fluent DSL-like thing similar to the patching DSL I designed.


            throw new NotImplementedException();
        }
    }
}