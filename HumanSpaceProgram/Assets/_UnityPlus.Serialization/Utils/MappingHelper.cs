using System;
using System.Runtime.CompilerServices;

namespace UnityPlus.Serialization
{
    public static class MappingHelper
    {
        /// <summary>
        /// Checks if the specified variable type (member type) is eligible to have a type header added.
        /// i.e. if a variable type can't be assigned an object of a different type.
        /// </summary>
        /// <remarks>
        /// The type header depends entirely on what can be stored in the variable,
        /// the instance type should be always the same as the variable type here, otherwise you *do* need the type header.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsNonNullEligibleForTypeHeader<TMember>()
        {
            // Types *not* eligible for the header (variable types that can't be assigned an object of a different type) are:

            // - structs
            // - sealed classes
            // - enums
            // - delegates

            return !typeof( TMember ).IsValueType // returns `true` for enums.
                && !typeof( TMember ).IsSealed
                && !typeof( Delegate ).IsAssignableFrom( typeof( TMember ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Type GetSerializedType<T>( SerializedData data )
        {
            if( data == null )
                return typeof( T );

            if( data.TryGetValue( KeyNames.TYPE, out var type ) )
                return type.DeserializeType();

            return typeof( T );
        }
    }
}