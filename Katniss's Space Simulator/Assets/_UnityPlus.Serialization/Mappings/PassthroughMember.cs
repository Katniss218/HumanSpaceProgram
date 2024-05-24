using System;

namespace UnityPlus.Serialization
{
    internal class PassthroughMember<TSource, TSourceBase> : MemberBase<TSource>, IMappedMember<TSource>, IMappedReferenceMember<TSource> where TSourceBase : class
    {
        IMappedMember<TSourceBase> _member;
        IMappedReferenceMember<TSourceBase> _refmember;

        internal static PassthroughMember<TSource, TSourceBase> Create( MemberBase<TSourceBase> member )
        {
            var m = new PassthroughMember<TSource, TSourceBase>()
            {
                _member = member as IMappedMember<TSourceBase>,
                _refmember = member as IMappedReferenceMember<TSourceBase>
            };
            return m;
        }

        public SerializedData Save( TSource source, IReverseReferenceMap s )
        {
            if( _refmember == null )
                return _member?.Save( source as TSourceBase, s ) ?? null;
            else
                return _refmember?.Save( source as TSourceBase, s ) ?? null;
        }

        public void Load( ref TSource source, SerializedData data, IForwardReferenceMap l )
        {
            TSourceBase src = source as TSourceBase; // won't work for structs, but structs aren't inheritable anyway.

            if( _member != null )
                _member?.Load( ref src, data, l );
        }

        public void LoadReferences( ref TSource source, SerializedData data, IForwardReferenceMap l )
        {
            TSourceBase src = source as TSourceBase; // won't work for structs, but structs aren't inheritable anyway.

            if( _refmember != null )
                _refmember?.LoadReferences( ref src, data, l );
        }
    }
}