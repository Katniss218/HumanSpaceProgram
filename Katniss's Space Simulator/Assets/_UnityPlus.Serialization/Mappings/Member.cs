using System;
using System.Linq.Expressions;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Serializes a member of type <typeparamref name="TMember"/>, that belongs to a type <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">The type that contains the member.</typeparam>
    /// <typeparam name="TMember">The type of the member (field/property/etc).</typeparam>
    public class Member<TSource, TMember> : MemberBase<TSource>
    {
        private readonly int _context = ObjectContext.Default;

        private readonly Getter<TSource, TMember> _getter;
        private readonly Setter<TSource, TMember> _setter;
        private readonly RefSetter<TSource, TMember> _structSetter;

        private readonly Expression<Func<TSource, TMember>> _memberAccessExpr;

        /// <summary>
        /// Checks if the member serialization represents a simple member access (object.member = value), as opposed to something more complicated.
        /// </summary>
        public bool IsSimpleAccess => _memberAccessExpr != null;

        /// <summary>
        /// Gets the serialization context that this member should use.
        /// </summary>
        public int Context => _context;

        private bool _hasCachedMapping;
        private SerializationMapping _cachedMapping;

        private void TryCacheMemberMapping()
        {
            Type type = typeof( TMember );
            if( type.IsValueType || (!type.IsInterface && type.BaseType == null) )
            {
                _hasCachedMapping = true;
                _cachedMapping = SerializationMappingRegistry.GetMappingOrEmpty( _context, typeof( TMember ) );
            }
        }

        // expression constructors

        /// <param name="member">Example: `o => o.position`.</param>
        public Member( Expression<Func<TSource, TMember>> member )
        {
            _memberAccessExpr = member;
            TryCacheMemberMapping();
            _getter = AccessorUtils.CreateGetter( member );

            if( typeof( TSource ).IsValueType )
                _structSetter = AccessorUtils.CreateStructSetter( member );
            else
                _setter = AccessorUtils.CreateSetter( member );
        }

        /// <param name="member">Example: `o => o.position`.</param>
        public Member( int context, Expression<Func<TSource, TMember>> member )
        {
            _memberAccessExpr = member;
            _context = context;
            TryCacheMemberMapping();
            _getter = AccessorUtils.CreateGetter( member );

            if( typeof( TSource ).IsValueType )
                _structSetter = AccessorUtils.CreateStructSetter( member );
            else
                _setter = AccessorUtils.CreateSetter( member );
        }

        // custom getter/setter constructors

        public Member( Getter<TSource, TMember> getter, Setter<TSource, TMember> setter )
        {
            if( typeof( TSource ).IsValueType )
                throw new InvalidOperationException( $"Member `{typeof( TSource ).FullName}` This constructor can only be used with a reference type TSource." );

            _memberAccessExpr = null;
            TryCacheMemberMapping();
            _getter = getter;
            _setter = setter;
        }

        public Member( int context, Getter<TSource, TMember> getter, Setter<TSource, TMember> setter )
        {
            if( typeof( TSource ).IsValueType )
                throw new InvalidOperationException( $"Member `{typeof( TSource ).FullName}` This constructor can only be used with a reference type TSource." );

            _memberAccessExpr = null;
            _context = context;
            TryCacheMemberMapping();
            _getter = getter;
            _setter = setter;
        }

        public Member( Getter<TSource, TMember> getter, RefSetter<TSource, TMember> setter )
        {
            if( !typeof( TSource ).IsValueType )
                throw new InvalidOperationException( $"Member `{typeof( TSource ).FullName}` This constructor can only be used with a value type TSource." );

            _memberAccessExpr = null;
            TryCacheMemberMapping();
            _getter = getter;
            _structSetter = setter;
        }

        public Member( int context, Getter<TSource, TMember> getter, RefSetter<TSource, TMember> setter )
        {
            if( !typeof( TSource ).IsValueType )
                throw new InvalidOperationException( $"Member `{typeof( TSource ).FullName}` This constructor can only be used with a value type TSource." );

            _memberAccessExpr = null;
            _context = context;
            TryCacheMemberMapping();
            _getter = getter;
            _structSetter = setter;
        }

        //
        //  Logic
        //

        public override SerializedData Save( TSource source, ISaver s )
        {
            var member = _getter.Invoke( source );

            var mapping = SerializationMappingRegistry.GetMappingOrDefault<TMember>( _context, member );

            if( mapping.SerializationStyle != SerializationStyle.None )
                return mapping.Save( member, s );
            return (SerializedData)null;
        }

        // The public-facing methods on the SerializationUnit are like a member,
        //   but the member itself can't be populated, only the end user may choose to do that on the root object(s).

        public override void Load( ref TSource source, SerializedData data, ILoader l )
        {
            Type memberType = typeof( TMember );
            if( data.TryGetValue( KeyNames.TYPE, out var type ) )
            {
                memberType = type.DeserializeType();
            }

            var mapping = _hasCachedMapping ? _cachedMapping : SerializationMappingRegistry.GetMappingOrDefault<TMember>( _context, memberType );

            TMember member = default;
            MappingHelper.DoLoad( mapping, ref member, data, l );

            if( _structSetter == null )
                _setter.Invoke( source, member );
            else
                _structSetter.Invoke( ref source, member );
        }

        public override void LoadReferences( ref TSource source, SerializedData data, ILoader l )
        {
            TMember member = _getter.Invoke( source );

            // This is needed, to reach the references nested inside objects that themselves don't contain any references.
            var mapping = SerializationMappingRegistry.GetMappingOrDefault<TMember>( _context, member );

            MappingHelper.DoLoadReferences( mapping, ref member, data, l );

            // This is needed, if the setter is custom (not auto-generated from field access (but NOT property access)) (look at LODGroup and its LOD[])
            // Basically, we don't have the guarantee that the class we have referenceequals the private state.
            if( _structSetter == null )
                _setter.Invoke( source, member );
            else
                _structSetter.Invoke( ref source, member );
        }
    }
}