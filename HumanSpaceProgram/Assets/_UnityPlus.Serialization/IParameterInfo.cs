using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Describes a parameter for a method.
    /// </summary>
    public interface IParameterInfo
    {
        string Name { get; }
        Type ParameterType { get; }
        IDescriptor TypeDescriptor { get; }
        object DefaultValue { get; }
    }
}