using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Marks a static method as an extension for a specific Type Descriptor.
    /// The method must have the signature: <c>static void MethodName(ClassDescriptor&lt;T&gt; descriptor)</c>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = true, Inherited = false )]
    public sealed class ExtendsMappingOfAttribute : Attribute
    {
        public Type TargetType { get; }
        public int Context { get; set; } = ObjectContext.Default;

        public ExtendsMappingOfAttribute( Type targetType )
        {
            TargetType = targetType;
        }
    }
}