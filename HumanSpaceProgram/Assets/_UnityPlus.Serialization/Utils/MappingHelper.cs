using System;
using System.Runtime.CompilerServices;

namespace UnityPlus.Serialization
{
    public static class MappingHelper
    {
        /// <summary>
        /// Determines which fields (if any) should be added to the object header for a variable (not an object instance) containing a non-null value.
        /// </summary>
        /// <remarks>
        /// The type TMember should be the same as mapping's TSource when calling this, otherwise you *do* need the type header, except in very specific user-defined circumstances. <br/>
        /// See also: <see cref="ShouldAddHeader{TSource, TMember}(ObjectHeaderSkipMode, TMember, SerializedData)"/>
        /// </remarks>
        /// <typeparam name="TMember">The type of the field/variable (member) that is storing the TSource object.</typeparam>
        /// <returns>
        /// A value indicating which fields (if any) should be added to the object header.
        /// </returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ObjectHeaderStyle IsNonNullEligibleForTypeHeader<TMember>()
        {
            ObjectHeaderStyle returnVal = ObjectHeaderStyle.None;

            // For reference types with value semantics (like String, Version, etc.),
            //   use the `skipHeader` mapping constructor parameter instead of adding them here.
            // This method doesn't have enough context to decide correctly.

            Type memberType = typeof( TMember );
            // Checking types like that, at least in a generic way, seems to be quite fast.
            // Not blazing fast, but nowhere near as slow as one would expect.
            if( !memberType.IsValueType // returns `true` for enums.
             && !memberType.IsSealed
             && !typeof( Delegate ).IsAssignableFrom( memberType ) )
            {
                returnVal |= ObjectHeaderStyle.TypeField | ObjectHeaderStyle.IDField;
            }
            else if( !memberType.IsValueType )
            {
                returnVal |= ObjectHeaderStyle.IDField;
            }

            return returnVal;
        }

        /// <summary>
        /// Checks if an object (instance) of type TMember is eligible to have an object header added. <br/>
        /// Supports every edge case internally.
        /// </summary>
        /// <typeparam name="TSource">The type of the mapping, equal to the type of the instance being checked.</typeparam>
        /// <typeparam name="TMember">The type of the field/variable (member) that is storing the TSource object.</typeparam>
        /// <param name="skipHeader">Determines whether or not to skip the header under specific circumstances.</param>
        /// <param name="obj">The object instance being de/serialized. Pass in default(TMember) when checking Load.</param>
        /// <param name="data">The data being de/serialized. Pass in null when checking Save.</param>
        public static ObjectHeaderStyle ShouldAddHeader<TSource, TMember>( ObjectHeaderSkipMode skipHeader, TMember obj, SerializedData data )
        {
            if( skipHeader == ObjectHeaderSkipMode.Always )
            {
                return ObjectHeaderStyle.None;
            }

            if( obj == null && data == null )
            {
                return ObjectHeaderStyle.None;
            }

            // Running this check first and adding the skip to structs seems to be a few % faster than doing the IsNonNullEligibleForTypeHeader check.
            if( skipHeader == ObjectHeaderSkipMode.WhenTypesMatch && (typeof( TSource ) == typeof( TMember )) )
                return ObjectHeaderStyle.None;

            ObjectHeaderStyle headerStyle = MappingHelper.IsNonNullEligibleForTypeHeader<TMember>();
            return headerStyle;
        }


        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Type GetSerializedType<TMember>( SerializedData data )
        {
            if( data == null )
                return typeof( TMember );

            if( data.TryGetValue( KeyNames.TYPE, out var type ) )
                return type.DeserializeType();

            return typeof( TMember );
        }
    }
}