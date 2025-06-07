using System.Collections.Generic;
using System.Linq;

namespace UnityPlus
{
    public static class OverridableEvent_Ex
    {
        /// <summary>
        /// Gets the objects that are not blacklisted by any of the specified objects.
        /// </summary>
        public static IEnumerable<IOverridable<T>> GetNonBlacklisted<T>( this IEnumerable<IOverridable<T>> values )
        {
            var fullBlacklist = values
                .Where( l => l.Blacklist != null )
                .SelectMany( l => l.Blacklist )
                .ToHashSet();

            return values
                .Where( l => !fullBlacklist.Contains( l.ID ) );
        }
    }
}