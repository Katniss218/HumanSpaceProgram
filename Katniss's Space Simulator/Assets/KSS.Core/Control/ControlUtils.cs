using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KSS.Control
{
    public static class ControlUtils
    {
        // arrays are supported on groups, and inputs/outputs. This will display as multiple of the entry right under each other. Possibly with a number for easier matching.

        static Dictionary<Type, (FieldInfo field, NamedControlAttribute attr)[]> _cache = new();

        private static (FieldInfo field, NamedControlAttribute attr)[] GetControlsOrGroups( object obj )
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

                    Type fieldType = field.FieldType;
                    if( !typeof( ControlGroup ).IsAssignableFrom( fieldType ) && !typeof( ControlGroup[] ).IsAssignableFrom( fieldType )
                     && !typeof( Control ).IsAssignableFrom( fieldType ) && !typeof( Control[] ).IsAssignableFrom( fieldType ) )
                        continue;

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

        public static IEnumerable<(object control, NamedControlAttribute attr)> GetControls( object target )
        {
            foreach( var (field, attr) in GetControlsOrGroups( target ) )
            {
                var member = field.GetValue( target );

                if( member.IsUnityNull() )
                    continue;

                yield return (member, attr);
            }
        }

        public static bool HasControls( object target )
        {
            return GetControlsOrGroups( target ).Any(); // TODO - optimize - don't actually download the full array and cache when the array is empty.
        }
    }
}