using KSS.Control;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Control
{
    public static class ControlUtils
    {
        public static void CacheControlInOutTransforms( Transform root )
        {

        }

        // arrays are supported on groups, and inputs/outputs. This will display as multiple of the entry right under each other. Possibly with a number for easier matching.

        static Dictionary<Type, (FieldInfo fi, NamedControlAttribute attr)[]> _cache = new();

        public static IEnumerable<(Control member, NamedControlAttribute attr)> GetControls( Component component )
        {
            Type type = component.GetType();
            if( _cache.TryGetValue( type, out var controls ) )
            {
                controls.Select( m => ((Control)m.fi.GetValue( component ), m.attr) ); // Use compiled expression?
            }
            else
            {
                FieldInfo[] controls2 = component.GetType().GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

                // cache of any.

               // return controls2.Select( m => (m, m.GetCustomAttribute<NamedControlAttribute>()) ).Where( m => m.Item2 != null );
            }
            throw new NotImplementedException();
        }

        public static IEnumerable<(ControlGroup controlGroup, NamedControlAttribute attr)> GetControlGroups( Component component )
        {
            FieldInfo[] controlGroups = component.GetType().GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

            throw new NotImplementedException();
        }

        public static IEnumerable<(Control control, NamedControlAttribute attr)> GetControls( ControlGroup group )
        {
            FieldInfo[] controls = group.GetType().GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

            throw new NotImplementedException();
        }
    }
}