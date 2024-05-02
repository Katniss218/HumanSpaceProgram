using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public class ExtensionMap
    {
        private readonly TypeMap<Delegate> _map = new();

        private readonly string _methodName;

        private readonly Type _returnType;

        private readonly Type[] _parameterTypes; // parameters after the first (`this T self`) parameter

        public ExtensionMap( string methodName, Type returnType, params Type[] parameterTypes )
        {
            if( parameterTypes.Length > 15 )
            {
                throw new ArgumentOutOfRangeException( nameof( parameterTypes ), "Parameter count must be between 0 and 15." );
            }

            _methodName = methodName;
            _returnType = returnType;
            _parameterTypes = parameterTypes;
        }

        private static IEnumerable<Type> GetStaticTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany( a => a.GetTypes() )
                .Where( dt => dt.IsSealed && !dt.IsGenericType );
        }

        private static Type GetFuncType( int argCount )
        {
            return argCount switch
            {
                0 => typeof( Func<> ),
                1 => typeof( Func<,> ),
                2 => typeof( Func<,,> ),
                3 => typeof( Func<,,,> ),
                4 => typeof( Func<,,,,> ),
                5 => typeof( Func<,,,,,> ),
                6 => typeof( Func<,,,,,,> ),
                7 => typeof( Func<,,,,,,,> ),
                8 => typeof( Func<,,,,,,,,> ),
                9 => typeof( Func<,,,,,,,,,> ),
                10 => typeof( Func<,,,,,,,,,,> ),
                11 => typeof( Func<,,,,,,,,,,,> ),
                12 => typeof( Func<,,,,,,,,,,,,> ),
                13 => typeof( Func<,,,,,,,,,,,,,> ),
                14 => typeof( Func<,,,,,,,,,,,,,,> ),
                15 => typeof( Func<,,,,,,,,,,,,,,,> ),
                16 => typeof( Func<,,,,,,,,,,,,,,,,> ),
                _ => throw new ArgumentOutOfRangeException( nameof( argCount ), "Argument count must be between 0 and 16." )
            };
        }

        private static Type GetActionType( int argCount )
        {
            return argCount switch
            {
                0 => typeof( Action ),
                1 => typeof( Action<> ),
                2 => typeof( Action<,> ),
                3 => typeof( Action<,,> ),
                4 => typeof( Action<,,,> ),
                5 => typeof( Action<,,,,> ),
                6 => typeof( Action<,,,,,> ),
                7 => typeof( Action<,,,,,,> ),
                8 => typeof( Action<,,,,,,,> ),
                9 => typeof( Action<,,,,,,,,> ),
                10 => typeof( Action<,,,,,,,,,> ),
                11 => typeof( Action<,,,,,,,,,,> ),
                12 => typeof( Action<,,,,,,,,,,,> ),
                13 => typeof( Action<,,,,,,,,,,,,> ),
                14 => typeof( Action<,,,,,,,,,,,,,> ),
                15 => typeof( Action<,,,,,,,,,,,,,,> ),
                16 => typeof( Action<,,,,,,,,,,,,,,,> ),
                _ => throw new ArgumentOutOfRangeException( nameof( argCount ), "Argument count must be between 0 and 16." )
            };
        }

        public bool TryGetValue( Type type, out Delegate del )
        {
            bool b = type.IsInterface;

            return _map.TryGetClosest( type, out del );
        }

        public void Reload()
        {
            _map.Clear();

            foreach( var containingType in GetStaticTypes() )
            {
                MethodInfo[] methods = containingType.GetMethods( BindingFlags.Public | BindingFlags.Static );

                foreach( var method in methods )
                {
                    if( method.Name == _methodName )
                    {
                        ParameterInfo retParam = method.ReturnParameter;
                        ParameterInfo[] methodParams = method.GetParameters();

                        if( retParam.ParameterType == _returnType
                         && methodParams.Length == (_parameterTypes.Length + 1)
                         && methodParams[0].ParameterType != typeof( object ) // prevent infinite recursion and stack overflow when the method doesn't exist.
                         && methodParams.Skip( 1 ).Select( p => p.ParameterType ).SequenceEqual( _parameterTypes ) )
                        {
                            Type unconstructedDelegateType = _returnType == typeof( void )
                                ? GetActionType( methodParams.Length )
                                : GetFuncType( methodParams.Length );

                            Type[] full = (_returnType == typeof( void )
                                ? methodParams
                                    .Select( p => p.ParameterType )
                                : methodParams
                                    .Select( p => p.ParameterType )
                                    .Append( retParam.ParameterType )
                                ).ToArray();

                            Type methodType = unconstructedDelegateType.MakeGenericType( full );

                            Delegate del = Delegate.CreateDelegate( methodType, method );

                            _map.Set( methodParams[0].ParameterType, del );
                        }
                    }
                }
            }
        }
    }
}