using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HSP.Control
{
    /// <summary>
    /// Contains a set of utility methods for getting control-related fields from objects.
    /// </summary>
    public static class ControlUtils
    {
        private static Dictionary<Type, (FieldInfo field, NamedControlAttribute attr)[]> _cache = new();

        public static bool IsSubtypeOf( Type fieldType, Type baseUnconstructedType, Type typeParameter )
        {
            return fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == baseUnconstructedType && typeParameter.IsAssignableFrom( fieldType.GetGenericArguments()[0] );
        }

        private static (FieldInfo field, NamedControlAttribute attr)[] GetControlsAndGroupsInternal( object obj )
        {
            Type objType = obj.GetType();
            if( _cache.TryGetValue( objType, out var controls ) )
            {
                return controls;
            }
            else
            {
                FieldInfo[] fields = obj.GetType().GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

                List<(FieldInfo, NamedControlAttribute)> controlsFoundOnTarget = new();

                foreach( var field in fields )
                {
                    NamedControlAttribute attr = field.GetCustomAttribute<NamedControlAttribute>();

                    if( attr == null )
                        continue;

                    // Only get valid types.
                    Type fieldType = field.FieldType;
                    if( !typeof( ControlGroup ).IsAssignableFrom( fieldType )
                     && !typeof( ControlGroup[] ).IsAssignableFrom( fieldType )
                     && !IsSubtypeOf( fieldType, typeof( List<> ), typeof( ControlGroup ) )
                     && !typeof( Control ).IsAssignableFrom( fieldType )
                     && !typeof( Control[] ).IsAssignableFrom( fieldType )
                     && !IsSubtypeOf( fieldType, typeof( List<> ), typeof( Control ) )
                     )
                    {
                        Debug.LogWarning( $"GetControlGroupsAndControls - {objType.Name} - {nameof( NamedControlAttribute )} attribute found on incompatible field `{fieldType.AssemblyQualifiedName}`." );
                        continue;
                    }

                    controlsFoundOnTarget.Add( (field, attr) );
                }

                controls = controlsFoundOnTarget.ToArray();
                if( controls.Any() )
                {
                    _cache.Add( objType, controls );
                }
            }

            return controls;
        }

        /// <summary>
        /// Gets every control input/output and control group present directly on the specified object (as a field).
        /// </summary>
        /// <remarks>
        /// The fields are cached for subsequent calls, but their values are not. <br/>
        /// Valid control types are those derived from <see cref="Control"/>, <see cref="ControlGroup"/>, and arrays of those. <br/>
        /// They also must be marked with <see cref="NamedControlAttribute"/>.
        /// </remarks>
        public static IEnumerable<(object control, NamedControlAttribute attr)> GetControlsAndGroups( object target )
        {
            foreach( var (field, attr) in GetControlsAndGroupsInternal( target ) )
            {
                object member = field.GetValue( target );

                if( member.IsUnityNull() )
                    continue;

                yield return (member, attr);
            }
        }

        /// <summary>
        /// Checks if there are any control inputs/outputs or control groups directly on the specified object (as a field).
        /// </summary>
        /// <remarks>
        /// Valid control types are those derived from <see cref="Control"/>, <see cref="ControlGroup"/>, and arrays of those. <br/>
        /// They also must be marked with <see cref="NamedControlAttribute"/>.
        /// </remarks>
        public static bool HasControlsOrGroups( object target )
        {
            return GetControlsAndGroupsInternal( target ).Any(); // TODO - optimize - don't actually download the full array and cache when the array is empty.
        }
    }
}