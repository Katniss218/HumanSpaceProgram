using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KSS.Control
{
    public static class ControlUtils
    {
        public static void CacheControlInOutTransforms( Transform root )
        {

        }

        // arrays are supported on groups, and inputs/outputs. This will display as multiple of the entry right under each other. Possibly with a number for easier matching.

        static Dictionary<Type, (FieldInfo field, NamedControlAttribute attr)[]> _cache = new();

        private static (FieldInfo field, NamedControlAttribute attr)[] GetControlsOrGroups<T>( object obj )
        {
            Type objType = obj.GetType();
            if( _cache.TryGetValue( objType, out var controls ) )
            {
                return controls;
            }
            else
            {
                FieldInfo[] controls2 = obj.GetType().GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

                List<(FieldInfo fi, NamedControlAttribute attr)> list = new();
                foreach( var control in controls2 )
                {
                    NamedControlAttribute attr = control.GetCustomAttribute<NamedControlAttribute>();
                    if( attr == null )
                        continue;

                    Type fieldType = control.FieldType;
                    if( !typeof( ControlGroup ).IsAssignableFrom( fieldType )
                     && !typeof( Control ).IsAssignableFrom( fieldType ) )
                        continue;

                    list.Add( (control, attr) );
                }

                controls = list.ToArray();
                if( controls.Any() )
                {
                    _cache.Add( objType, controls );
                }
            }

            return controls;
        }

        public static IEnumerable<(object control, NamedControlAttribute attr)> GetControls( object target )
        {
            foreach( var (field, attr) in GetControlsOrGroups<Control>( target ) )
            {
                var member = field.GetValue( target );
                yield return (member, attr);
            }
        }
    }
}