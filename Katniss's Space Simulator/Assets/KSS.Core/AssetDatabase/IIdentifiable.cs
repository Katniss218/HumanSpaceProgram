using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSS.Core.AssetDatabase
{
    /// <summary>
    /// Anything that is loaded to the asset database.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// Returns the identifier of the object that can be used to look it up in the asset database.
        /// </summary>
        string ID { get; }
    }

    public static class IIdentifiable_Ex
    {
        public static bool IdentityEquals( this IIdentifiable lhs, IIdentifiable rhs )
        {
            if( lhs.ID != rhs.ID )
                return false;

            // maybe type checking later.

            return true;
        }
    }
}