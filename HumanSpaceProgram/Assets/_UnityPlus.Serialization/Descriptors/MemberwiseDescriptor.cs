using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Base class for MemberwiseDescriptor to allow sharing members across generic instantiations.
    /// </summary>
    public abstract class MemberwiseDescriptorBase : CompositeDescriptor
    {
        internal readonly List<MemberDefinition> _members = new List<MemberDefinition>();
        internal readonly List<IMethodInfo> _methods = new List<IMethodInfo>();

        // Factories
        internal Func<object> _simpleFactory;
        internal Func<SerializedData, SerializationContext, object> _rawFactory;
        internal Delegate _constructor;
        internal (string name, Type type)[] _constructorParams;

        internal class MemberDefinition : IMemberInfo
        {
            public string Name { get; }
            public int Index => -1;
            public ContextKey Context { get; }
            public Func<object, object> Getter;
            public Action<object, object> Setter;
            public RefSetter<object, object> RefSetter;
            public Type MemberType { get; }
            public bool RequiresWriteBack => MemberType.IsValueType;

            public Predicate<object> ShouldSerialize;
            public Func<object, SerializationContext, bool> ShouldSerializeWithContext;

            private IDescriptor _cachedDescriptor;

            public IDescriptor TypeDescriptor
            {
                get
                {
                    if( _cachedDescriptor == null )
                    {
                        _cachedDescriptor = TypeDescriptorRegistry.GetDescriptor( MemberType, Context );
                    }
                    return _cachedDescriptor;
                }
            }

            public MemberDefinition( string name, ContextKey context, Func<object, object> getter, Action<object, object> setter, RefSetter<object, object> refSetter, Type memberType )
            {
                Name = name;
                Context = context;
                Getter = getter;
                Setter = setter;
                RefSetter = refSetter;
                MemberType = memberType;
            }

            public MemberDefinition Clone()
            {
                var clone = new MemberDefinition( Name, Context, Getter, Setter, RefSetter, MemberType );
                clone.ShouldSerialize = ShouldSerialize;
                clone.ShouldSerializeWithContext = ShouldSerializeWithContext;
                return clone;
            }

            public ContextKey GetContext( object target ) => Context;

            public object GetValue( object target ) => Getter( target );

            public void SetValue( ref object target, object value )
            {
                if( RefSetter != null )
                {
                    RefSetter( ref target, value );
                }
                else if( Setter != null )
                {
                    Setter( target, value );
                }
            }
        }
    }

    /// <summary>
    /// A concrete descriptor for a class or struct, composed of named members.
    /// </summary>
    /// <typeparam name="T">The type being described.</typeparam>
    public class MemberwiseDescriptor<T> : MemberwiseDescriptorBase
    {
        public readonly struct MemberModifier
        {
            private readonly MemberwiseDescriptor<T> _descriptor;
            private readonly int _index;

            public MemberModifier( MemberwiseDescriptor<T> descriptor, int index )
            {
                _descriptor = descriptor;
                _index = index;
            }

            /// <summary>
            /// Applies a condition to the selected member. The member will only be serialized/deserialized if the condition returns true.
            /// </summary>
            public MemberwiseDescriptor<T> When( Predicate<T> condition )
            {
                if( _index == -1 ) return _descriptor;
                var member = _descriptor._members[_index];

                // Chain predicates (AND logic)
                var old = member.ShouldSerialize;
                member.ShouldSerialize = old == null ? (o => condition( (T)o )) : (o => old( o ) && condition( (T)o ));

                return _descriptor;
            }

            /// <summary>
            /// Applies a context-aware condition to the selected member.
            /// </summary>
            public MemberwiseDescriptor<T> When( Func<T, SerializationContext, bool> condition )
            {
                if( _index == -1 ) return _descriptor;
                var member = _descriptor._members[_index];

                var old = member.ShouldSerializeWithContext;
                member.ShouldSerializeWithContext = old == null ? (( o, c ) => condition( (T)o, c )) : (( o, c ) => old( o, c ) && condition( (T)o, c ));

                return _descriptor;
            }

            /// <summary>
            /// Removes the selected member from the serialization descriptor.
            /// </summary>
            public MemberwiseDescriptor<T> Delete()
            {
                if( _index != -1 )
                {
                    _descriptor._members.RemoveAt( _index );
                }
                return _descriptor;
            }
        }

        public override Type MappedType => typeof( T );

        // Lifecycle
        private Action<T, SerializationContext> _onSerializing;
        private Action<T, SerializationContext> _onDeserialized;

        public MemberwiseDescriptor()
        {
            IncludeBaseMembers();
        }

        private void IncludeBaseMembers()
        {
            Type baseType = typeof( T ).BaseType;
            if( baseType == null || baseType == typeof( object ) || baseType == typeof( ValueType ) )
                return;

            // Resolve base descriptor
            var baseDesc = TypeDescriptorRegistry.GetDescriptor( baseType );

            if( baseDesc is MemberwiseDescriptorBase baseMemberwise )
            {
                // Copy members (Cloned to allow independent modification)
                foreach( var m in baseMemberwise._members )
                {
                    _members.Add( m.Clone() );
                }

                // Copy methods (Shared)
                _methods.AddRange( baseMemberwise._methods );

                // Copy Factory (Last one wins, so we take base's initially)
                _simpleFactory = baseMemberwise._simpleFactory;
                _rawFactory = baseMemberwise._rawFactory;
                _constructor = baseMemberwise._constructor;
                _constructorParams = baseMemberwise._constructorParams;
            }
        }

        // --- Fluent API: Member Modification ---

        /// <summary>
        /// Selects an existing member by name for modification (applying conditions, removing, etc).
        /// </summary>
        public MemberModifier Member( string name )
        {
            int index = _members.FindIndex( m => m.Name == name );
            if( index == -1 )
            {
                UnityEngine.Debug.LogError( $"Member '{name}' not found on type '{typeof( T ).Name}'." );
                return new MemberModifier( this, -1 );
            }
            return new MemberModifier( this, index );
        }

        /// <summary>
        /// Removes a member by name. Useful for excluding members inherited from base types.
        /// </summary>
        public MemberwiseDescriptor<T> WithoutMember( string name )
        {
            _members.RemoveAll( m => m.Name == name );
            return this;
        }

        // --- Fluent API: Conditionals (Last Member Shortcut) ---

        /// <summary>
        /// Applies a condition to the LAST added member.
        /// </summary>
        public MemberwiseDescriptor<T> When( Predicate<T> condition )
        {
            if( _members.Count == 0 ) throw new InvalidOperationException( "No members defined." );
            return new MemberModifier( this, _members.Count - 1 ).When( condition );
        }

        public MemberwiseDescriptor<T> When( Func<T, SerializationContext, bool> condition )
        {
            if( _members.Count == 0 ) throw new InvalidOperationException( "No members defined." );
            return new MemberModifier( this, _members.Count - 1 ).When( condition );
        }

        // --- Fluent API: Members (Expression Based) ---

        public MemberwiseDescriptor<T> WithMember<TMember>( string name, Expression<Func<T, TMember>> accessor )
        {
            return WithMember( name, ContextKey.Default, accessor );
        }

        public MemberwiseDescriptor<T> WithMember<TMember>( string name, Type contextType, Expression<Func<T, TMember>> accessor )
        {
            var contextId = ContextRegistry.GetID( contextType );
            return WithMember( name, contextId, accessor );
        }

        public MemberwiseDescriptor<T> WithMember<TMember>( string name, ContextKey context, Expression<Func<T, TMember>> accessor )
        {
            var getter = AccessorUtils.CreateGetter( accessor );
            Setter<T, TMember> setter = null;
            RefSetter<T, TMember> refSetter = null;

            if( typeof( T ).IsValueType )
                refSetter = AccessorUtils.CreateStructSetter( accessor );
            else
                setter = AccessorUtils.CreateSetter( accessor );

            return RegisterMember( name, context, getter, setter, refSetter );
        }

        // --- Fluent API: Members (Delegate/v3 Compatibility) ---

        public MemberwiseDescriptor<T> WithMember<TMember>( string name, Getter<T, TMember> getter, Setter<T, TMember> setter )
        {
            return WithMember( name, ContextKey.Default, getter, setter );
        }

        public MemberwiseDescriptor<T> WithMember<TMember>( string name, Type contextType, Getter<T, TMember> getter, Setter<T, TMember> setter )
        {
            return WithMember( name, ContextRegistry.GetID( contextType ), getter, setter );
        }

        public MemberwiseDescriptor<T> WithMember<TMember>( string name, ContextKey context, Getter<T, TMember> getter, Setter<T, TMember> setter )
        {
            if( typeof( T ).IsValueType )
                throw new InvalidOperationException( $"Cannot use Action<T, Member> setter for struct type {typeof( T )}. Use expressions or RefSetter." );

            return RegisterMember( name, context, getter, setter, null );
        }

        public MemberwiseDescriptor<T> WithMember<TMember>( string name, Getter<T, TMember> getter, RefSetter<T, TMember> refSetter )
        {
            return WithMember( name, ContextKey.Default, getter, refSetter );
        }

        public MemberwiseDescriptor<T> WithMember<TMember>( string name, Type contextType, Getter<T, TMember> getter, RefSetter<T, TMember> refSetter )
        {
            return WithMember( name, ContextRegistry.GetID( contextType ), getter, refSetter );
        }

        public MemberwiseDescriptor<T> WithMember<TMember>( string name, ContextKey context, Getter<T, TMember> getter, RefSetter<T, TMember> refSetter )
        {
            return RegisterMember( name, context, getter, null, refSetter );
        }

        // --- Internal Registration Helper ---

        private MemberwiseDescriptor<T> RegisterMember<TMember>( string name, ContextKey context, Getter<T, TMember> getter, Setter<T, TMember> setter, RefSetter<T, TMember> refSetter )
        {
            _members.Add( new MemberDefinition(
                name,
                context,
                t => (object)getter( (T)t ),
                setter != null ? ( t, v ) => setter( (T)t, (TMember)v ) : (Action<object, object>)null,
                refSetter != null ? ( ref object t, object v ) =>
                {
                    T typed = (T)t;
                    refSetter( ref typed, (TMember)v );
                    t = typed;
                }
            : (RefSetter<object, object>)null,
                typeof( TMember )
            ) );
            return this;
        }

        public MemberwiseDescriptor<T> WithReadonlyMember<TMember>( string name, Func<T, TMember> getter )
        {
            return WithReadonlyMember( name, ContextKey.Default, getter );
        }

        public MemberwiseDescriptor<T> WithReadonlyMember<TMember>( string name, Type contextType, Func<T, TMember> getter )
        {
            return WithReadonlyMember( name, ContextRegistry.GetID( contextType ), getter );
        }

        public MemberwiseDescriptor<T> WithReadonlyMember<TMember>( string name, ContextKey context, Func<T, TMember> getter )
        {
            _members.Add( new MemberDefinition(
                name,
                context,
                t => (object)getter( (T)t ),
                null,
                null,
                typeof( TMember )
            ) );
            return this;
        }

        // --- Fluent API: Construction & Factories ---

        /// <summary>
        /// Defines a factory that creates the object using parameters loaded from serialized data.
        /// </summary>
        /// <param name="constructor">The delegate to create the object.</param>
        /// <param name="parameters">The list of (name, type) pairs for the parameters, matching the order of the delegate.</param>
        public MemberwiseDescriptor<T> WithConstructor( Func<object[], T> constructor, params (string name, Type type)[] parameters )
        {
            _constructor = constructor;
            _constructorParams = parameters;
            return this;
        }

        /// <summary>
        /// Defines a factory that inspects the raw serialized data before creating the object.
        /// Useful for ScriptableObject/Prefab instantiation.
        /// </summary>
        public MemberwiseDescriptor<T> WithRawFactory( Func<SerializedData, SerializationContext, T> factory )
        {
            _rawFactory = ( d, c ) => factory( d, c );
            return this;
        }

        // --- Strongly Typed Factory Overloads ---

        /// <summary>
        /// Defines a simple parameterless factory.
        /// </summary>
        public MemberwiseDescriptor<T> WithFactory( Func<object> factory )
        {
            _simpleFactory = factory;
            return this;
        }

        public MemberwiseDescriptor<T> WithFactory<P1>( Func<P1, T> factory, string n1 )
        {
            return WithConstructor(
                args => factory( (P1)args[0] ),
                (n1, typeof( P1 ))
            );
        }

        public MemberwiseDescriptor<T> WithFactory<P1, P2>( Func<P1, P2, T> factory, string n1, string n2 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1] ),
                (n1, typeof( P1 )),
                (n2, typeof( P2 ))
            );
        }

        public MemberwiseDescriptor<T> WithFactory<P1, P2, P3>( Func<P1, P2, P3, T> factory, string n1, string n2, string n3 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2] ),
                (n1, typeof( P1 )),
                (n2, typeof( P2 )),
                (n3, typeof( P3 ))
            );
        }

        public MemberwiseDescriptor<T> WithFactory<P1, P2, P3, P4>( Func<P1, P2, P3, P4, T> factory, string n1, string n2, string n3, string n4 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2], (P4)args[3] ),
                (n1, typeof( P1 )),
                (n2, typeof( P2 )),
                (n3, typeof( P3 )),
                (n4, typeof( P4 ))
            );
        }

        public MemberwiseDescriptor<T> WithFactory<P1, P2, P3, P4, P5>( Func<P1, P2, P3, P4, P5, T> factory, string n1, string n2, string n3, string n4, string n5 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2], (P4)args[3], (P5)args[4] ),
                (n1, typeof( P1 )),
                (n2, typeof( P2 )),
                (n3, typeof( P3 )),
                (n4, typeof( P4 )),
                (n5, typeof( P5 ))
            );
        }

        public MemberwiseDescriptor<T> WithFactory<P1, P2, P3, P4, P5, P6>( Func<P1, P2, P3, P4, P5, P6, T> factory, string n1, string n2, string n3, string n4, string n5, string n6 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2], (P4)args[3], (P5)args[4], (P6)args[5] ),
                (n1, typeof( P1 )),
                (n2, typeof( P2 )),
                (n3, typeof( P3 )),
                (n4, typeof( P4 )),
                (n5, typeof( P5 )),
                (n6, typeof( P6 ))
            );
        }

        public MemberwiseDescriptor<T> WithFactory<P1, P2, P3, P4, P5, P6, P7>( Func<P1, P2, P3, P4, P5, P6, P7, T> factory, string n1, string n2, string n3, string n4, string n5, string n6, string n7 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2], (P4)args[3], (P5)args[4], (P6)args[5], (P7)args[6] ),
                (n1, typeof( P1 )), (n2, typeof( P2 )), (n3, typeof( P3 )), (n4, typeof( P4 )), (n5, typeof( P5 )), (n6, typeof( P6 )), (n7, typeof( P7 ))
            );
        }

        public MemberwiseDescriptor<T> WithFactory<P1, P2, P3, P4, P5, P6, P7, P8>( Func<P1, P2, P3, P4, P5, P6, P7, P8, T> factory, string n1, string n2, string n3, string n4, string n5, string n6, string n7, string n8 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2], (P4)args[3], (P5)args[4], (P6)args[5], (P7)args[6], (P8)args[7] ),
                (n1, typeof( P1 )), (n2, typeof( P2 )), (n3, typeof( P3 )), (n4, typeof( P4 )), (n5, typeof( P5 )), (n6, typeof( P6 )), (n7, typeof( P7 )), (n8, typeof( P8 ))
            );
        }

        public MemberwiseDescriptor<T> WithFactory<P1, P2, P3, P4, P5, P6, P7, P8, P9>( Func<P1, P2, P3, P4, P5, P6, P7, P8, P9, T> factory, string n1, string n2, string n3, string n4, string n5, string n6, string n7, string n8, string n9 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2], (P4)args[3], (P5)args[4], (P6)args[5], (P7)args[6], (P8)args[7], (P9)args[8] ),
                (n1, typeof( P1 )), (n2, typeof( P2 )), (n3, typeof( P3 )), (n4, typeof( P4 )), (n5, typeof( P5 )), (n6, typeof( P6 )), (n7, typeof( P7 )), (n8, typeof( P8 )), (n9, typeof( P9 ))
            );
        }

        public MemberwiseDescriptor<T> WithFactory<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>( Func<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, T> factory, string n1, string n2, string n3, string n4, string n5, string n6, string n7, string n8, string n9, string n10 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2], (P4)args[3], (P5)args[4], (P6)args[5], (P7)args[6], (P8)args[7], (P9)args[8], (P10)args[9] ),
                (n1, typeof( P1 )), (n2, typeof( P2 )), (n3, typeof( P3 )), (n4, typeof( P4 )), (n5, typeof( P5 )), (n6, typeof( P6 )), (n7, typeof( P7 )), (n8, typeof( P8 )), (n9, typeof( P9 )), (n10, typeof( P10 ))
            );
        }

        public MemberwiseDescriptor<T> WithFactory<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>( Func<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, T> factory, string n1, string n2, string n3, string n4, string n5, string n6, string n7, string n8, string n9, string n10, string n11 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2], (P4)args[3], (P5)args[4], (P6)args[5], (P7)args[6], (P8)args[7], (P9)args[8], (P10)args[9], (P11)args[10] ),
                (n1, typeof( P1 )), (n2, typeof( P2 )), (n3, typeof( P3 )), (n4, typeof( P4 )), (n5, typeof( P5 )), (n6, typeof( P6 )), (n7, typeof( P7 )), (n8, typeof( P8 )), (n9, typeof( P9 )), (n10, typeof( P10 )), (n11, typeof( P11 ))
            );
        }

        public MemberwiseDescriptor<T> WithFactory<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>( Func<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, T> factory, string n1, string n2, string n3, string n4, string n5, string n6, string n7, string n8, string n9, string n10, string n11, string n12 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2], (P4)args[3], (P5)args[4], (P6)args[5], (P7)args[6], (P8)args[7], (P9)args[8], (P10)args[9], (P11)args[10], (P12)args[11] ),
                (n1, typeof( P1 )), (n2, typeof( P2 )), (n3, typeof( P3 )), (n4, typeof( P4 )), (n5, typeof( P5 )), (n6, typeof( P6 )), (n7, typeof( P7 )), (n8, typeof( P8 )), (n9, typeof( P9 )), (n10, typeof( P10 )), (n11, typeof( P11 )), (n12, typeof( P12 ))
            );
        }

        public MemberwiseDescriptor<T> WithFactory<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>( Func<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, T> factory, string n1, string n2, string n3, string n4, string n5, string n6, string n7, string n8, string n9, string n10, string n11, string n12, string n13 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2], (P4)args[3], (P5)args[4], (P6)args[5], (P7)args[6], (P8)args[7], (P9)args[8], (P10)args[9], (P11)args[10], (P12)args[11], (P13)args[12] ),
                (n1, typeof( P1 )), (n2, typeof( P2 )), (n3, typeof( P3 )), (n4, typeof( P4 )), (n5, typeof( P5 )), (n6, typeof( P6 )), (n7, typeof( P7 )), (n8, typeof( P8 )), (n9, typeof( P9 )), (n10, typeof( P10 )), (n11, typeof( P11 )), (n12, typeof( P12 )), (n13, typeof( P13 ))
            );
        }

        public MemberwiseDescriptor<T> WithFactory<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>( Func<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, T> factory, string n1, string n2, string n3, string n4, string n5, string n6, string n7, string n8, string n9, string n10, string n11, string n12, string n13, string n14 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2], (P4)args[3], (P5)args[4], (P6)args[5], (P7)args[6], (P8)args[7], (P9)args[8], (P10)args[9], (P11)args[10], (P12)args[11], (P13)args[12], (P14)args[13] ),
                (n1, typeof( P1 )), (n2, typeof( P2 )), (n3, typeof( P3 )), (n4, typeof( P4 )), (n5, typeof( P5 )), (n6, typeof( P6 )), (n7, typeof( P7 )), (n8, typeof( P8 )), (n9, typeof( P9 )), (n10, typeof( P10 )), (n11, typeof( P11 )), (n12, typeof( P12 )), (n13, typeof( P13 )), (n14, typeof( P14 ))
            );
        }

        public MemberwiseDescriptor<T> WithFactory<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>( Func<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, T> factory, string n1, string n2, string n3, string n4, string n5, string n6, string n7, string n8, string n9, string n10, string n11, string n12, string n13, string n14, string n15 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2], (P4)args[3], (P5)args[4], (P6)args[5], (P7)args[6], (P8)args[7], (P9)args[8], (P10)args[9], (P11)args[10], (P12)args[11], (P13)args[12], (P14)args[13], (P15)args[14] ),
                (n1, typeof( P1 )), (n2, typeof( P2 )), (n3, typeof( P3 )), (n4, typeof( P4 )), (n5, typeof( P5 )), (n6, typeof( P6 )), (n7, typeof( P7 )), (n8, typeof( P8 )), (n9, typeof( P9 )), (n10, typeof( P10 )), (n11, typeof( P11 )), (n12, typeof( P12 )), (n13, typeof( P13 )), (n14, typeof( P14 )), (n15, typeof( P15 ))
            );
        }

        // --- Method / UI Support ---

        public MemberwiseDescriptor<T> WithMethod( string name, Action<T> action, string displayName = null )
        {
            _methods.Add( new ActionMethodInfo( name, displayName, action ) );
            return this;
        }

        // --- Fluent API: Lifecycle ---

        public MemberwiseDescriptor<T> OnSerializing( Action<T, SerializationContext> callback )
        {
            _onSerializing += callback;
            return this;
        }

        public MemberwiseDescriptor<T> OnDeserialized( Action<T, SerializationContext> callback )
        {
            _onDeserialized += callback;
            return this;
        }

        // --- ICompositeTypeDescriptor Implementation ---

        public override int GetConstructionStepCount( object target )
        {
            return _constructorParams?.Length ?? 0;
        }

        public override int GetStepCount( object target )
        {
            return GetConstructionStepCount( target ) + _members.Count;
        }

        public override IMemberInfo GetMemberInfo( int stepIndex )
        {
            int ctorCount = GetConstructionStepCount( null );

            if( stepIndex < ctorCount )
            {
                var (name, type) = _constructorParams[stepIndex];
                // Try to find context from registered members
                ContextKey context = ContextKey.Default;
                var memberDef = _members.Find( m => m.Name == name );
                if( memberDef != null )
                {
                    context = memberDef.Context;
                }

                IDescriptor typeDesc = TypeDescriptorRegistry.GetDescriptor( type, context );
                return new BufferMemberInfo( stepIndex, name, type, typeDesc );
            }

            int memberIndex = stepIndex - ctorCount;
            if( memberIndex < 0 || memberIndex >= _members.Count )
            {
                // This shouldn't happen if StepCount is correct, but safety first.
                return null;
            }

            var def = _members[memberIndex];
            return def;
        }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            int ctorCount = GetConstructionStepCount( null );

            if( ctorCount > 0 )
                return new object[ctorCount];

            if( _rawFactory != null )
                return _rawFactory( data, ctx );

            if( _simpleFactory != null )
                return _simpleFactory.Invoke();

            if( typeof( T ).IsValueType ) return Activator.CreateInstance<T>();
            try { return Activator.CreateInstance<T>(); }
            catch { return null; }
        }

        public override object Construct( object initialTarget )
        {
            int ctorCount = GetConstructionStepCount( initialTarget );

            if( ctorCount > 0 && _constructor != null && initialTarget is object[] )
            {
                return _constructor.DynamicInvoke( initialTarget );
            }
            return initialTarget;
        }

        public override void OnSerializing( object target, SerializationContext context )
        {
            _onSerializing?.Invoke( (T)target, context );
        }

        public override void OnDeserialized( object target, SerializationContext context )
        {
            _onDeserialized?.Invoke( (T)target, context );
        }

        public override int GetMethodCount() => _methods.Count;
        public override IMethodInfo GetMethodInfo( int methodIndex ) => _methods[methodIndex];

        // --- Internal Definitions ---

        /// <summary>
        /// Used when deserializing an object with constructor parameters. <br/>
        /// It writes to a buffer during deserialization.
        /// </summary>
        private readonly struct BufferMemberInfo : IMemberInfo
        {
            public string Name { get; }
            public int Index => -1; // Constructor args are named
            public Type MemberType { get; }
            public IDescriptor TypeDescriptor { get; }
            public bool RequiresWriteBack => MemberType.IsValueType;

            private readonly int _index;

            public BufferMemberInfo( int index, string name, Type type, IDescriptor desc )
            {
                _index = index;
                Name = name;
                MemberType = type;
                TypeDescriptor = desc;
            }

            public ContextKey GetContext( object target ) => default;

            public object GetValue( object target ) => ((object[])target)[_index];
            public void SetValue( ref object target, object value ) => ((object[])target)[_index] = value;
        }

        private class ActionMethodInfo : IMethodInfo
        {
            public string Name { get; }
            public string DisplayName { get; }
            public bool IsStatic => false;
            public bool IsGeneric => false;
            public string[] GenericTypeParameters => Array.Empty<string>();
            public IParameterInfo[] Parameters => Array.Empty<IParameterInfo>();

            private readonly Action<T> _action;

            public ActionMethodInfo( string name, string displayName, Action<T> action )
            {
                Name = name;
                DisplayName = displayName ?? name;
                _action = action;
            }

            public object Invoke( object target, object[] parameters )
            {
                _action( (T)target );
                return null;
            }
        }
    }
}
