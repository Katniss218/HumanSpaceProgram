namespace UnityPlus.Serialization
{
    /// <summary>
    /// Describes a callable method on a type.
    /// </summary>
    public interface IMethodInfo
    {
#warning TODO - duplicated names. Not necessary/or differs from MemberInfo.
        string Name { get; }
        string DisplayName { get; }
        bool IsStatic { get; }
        bool IsGeneric { get; }
        string[] GenericTypeParameters { get; }
        IParameterInfo[] Parameters { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">Null to invoke a static method.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object Invoke( object target, object[] parameters );
    }
}