using System;
using System.Reflection;

namespace UnityPlus.Serialization.Descriptors
{
    /// <summary>
    /// An IMethodInfo implementation that wraps a <see cref="MethodInfo"/>.
    /// </summary>
    public class ReflectionMethodInfo : IMethodInfo
    {
        public string DisplayName { get; }
        public bool IsStatic => _method.IsStatic;
        public bool IsGeneric => _method.IsGenericMethodDefinition;
        public string[] GenericTypeParameters => IsGeneric ? Array.ConvertAll( _method.GetGenericArguments(), t => t.Name ) : Array.Empty<string>();
        public IParameterInfo[] Parameters { get; }

        private MethodInfo _method;

        public ReflectionMethodInfo( MethodInfo method )
        {
            _method = method;

            var contextMenu = method.GetCustomAttribute<UnityEngine.ContextMenu>();
            if( contextMenu != null && !string.IsNullOrEmpty( contextMenu.menuItem ) )
            {
                DisplayName = contextMenu.menuItem;
            }
            else
            {
                DisplayName = method.Name;
            }

            var parameters = method.GetParameters();
            Parameters = new IParameterInfo[parameters.Length];
            for( int i = 0; i < parameters.Length; i++ )
            {
                Parameters[i] = new ReflectionParameterInfo( parameters[i] );
            }
        }

        public object Invoke( object target, object[] parameters )
        {
            return _method.Invoke( target, parameters );
        }
    }

    /// <summary>
    /// An IParameterInfo implementation that wraps a <see cref="ParameterInfo"/>.
    /// </summary>
    public class ReflectionParameterInfo : IParameterInfo
    {
        private ParameterInfo _info;
        public string Name => _info.Name;
        public Type ParameterType => _info.ParameterType;
        public object DefaultValue => _info.HasDefaultValue ? _info.DefaultValue : (ParameterType.IsValueType ? Activator.CreateInstance( ParameterType ) : null);

        public IDescriptor TypeDescriptor => TypeDescriptorRegistry.GetDescriptor( ParameterType, 0 );

        public ReflectionParameterInfo( ParameterInfo info ) { _info = info; }
    }
}