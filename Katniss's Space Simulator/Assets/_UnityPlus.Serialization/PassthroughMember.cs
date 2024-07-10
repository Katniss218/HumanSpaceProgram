using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// This is used when you need to store a member of the base type inside the memberwise mapping.
    /// </summary>
    internal class PassthroughMember<TSource, TSourceBase> : MemberBase<TSource> where TSourceBase : class
    {
        MemberBase<TSourceBase> _member;

        internal static PassthroughMember<TSource, TSourceBase> Create( MemberBase<TSourceBase> member )
        {
            return new PassthroughMember<TSource, TSourceBase>()
            {
                _member = member,
            };
        }

        public override SerializedData Save( TSource source, ISaver s )
        {
            return _member.Save( source as TSourceBase, s );
        }

        public override void Load( ref TSource source, SerializedData data, ILoader l )
        {
            TSourceBase src = source as TSourceBase; // won't work for structs, but structs aren't inheritable anyway.

            _member.Load( ref src, data, l );
        }

        public override void LoadReferences( ref TSource source, SerializedData data, ILoader l )
        {
            TSourceBase src = source as TSourceBase; // won't work for structs, but structs aren't inheritable anyway.

            _member.LoadReferences( ref src, data, l );
        }
    }
}