using System;
using System.Collections.Generic;
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

                    var member = control.FieldType( obj );
                        continue;

                    list.Add( (control, attr) );
                }

                controls = list.ToArray();
                _cache.Add( objType, controls );
            }

            return controls;
        }

        public static IEnumerable<(object control, NamedControlAttribute attr)> GetControls( Component component )
        {
            foreach( var (field, attr) in GetControlsOrGroups<Control>( component ) )
            {
                var member = field.GetValue( component );
                yield return (member, attr);
            }
        }

        public static IEnumerable<(object control, NamedControlAttribute attr)> GetControls( ControlGroup group )
        {
            foreach( var (field, attr) in GetControlsOrGroups<Control>( group ) )
            {
                var member = field.GetValue( group );
                yield return (member, attr);
            }
        }
    }
}