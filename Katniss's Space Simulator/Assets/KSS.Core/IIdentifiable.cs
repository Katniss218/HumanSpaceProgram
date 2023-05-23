using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSS.Core
{
    /// <summary>
    /// Anything that is loaded to the data registry.
    /// </summary>
    public interface IIdentifiable
    {
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