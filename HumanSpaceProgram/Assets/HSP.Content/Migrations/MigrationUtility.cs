using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace HSP.Content.Migrations
{
    public static class MigrationUtility
    {
        public static void RenameType( ref SerializedData data, string from, string to )
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<SerializedObject> FindObjectsWithType( ref SerializedData data, string typeName )
        {
            throw new NotImplementedException();
        }

        public static void RenameField( ref SerializedData data, string typeName, string fromField, string toField )
        {
#warning TODO - type might be implicit, this is dangerous without the schema for the previous save.
            throw new NotImplementedException();
        }
    }
}