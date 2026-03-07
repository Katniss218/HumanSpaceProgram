using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Handles serialization of Delegates (Action, Func, Events).
    /// Stores the invocation list as an array of entries.
    /// Uses dynamic construction steps to resolve Target objects via reference.
    /// </summary>
    public class DelegateDescriptor : CompositeDescriptor
    {
        public override Type MappedType => typeof( Delegate );

        public override int GetConstructionStepCount( object target )
        {
            // Deserialize: Target is buffer (object[] of length N) -> N steps.
            if( target is object[] buffer )
                return buffer.Length;

            // Serialize: Target is Delegate -> InvocationList.Length steps.
            if( target is Delegate del )
                return del.GetInvocationList().Length;

            return 0;
        }

        public override int GetStepCount( object target )
        {
            // All steps are construction steps because Delegates are immutable.
            return GetConstructionStepCount( target );
        }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            // If data is an array, we need a buffer of that size.
            if( data is SerializedArray arr )
            {
                return new object[arr.Count];
            }
            return new object[0];
        }

        public override IMemberInfo GetMemberInfo( int stepIndex )
        {
            // For both Serialize and Deserialize, we need a MemberInfo that points to the Target Object of the invocation.
            return new DelegateEntryMemberInfo( stepIndex );
        }

        public override object Construct( object initialTarget )
        {
            var buffer = (object[])initialTarget;
            var delegates = new List<Delegate>();

            foreach( var item in buffer )
            {
                if( item is DelegateEntry entry && entry.Method != null && entry.DelegateType != null )
                {
                    try
                    {
                        Delegate d = null;
                        if( entry.Method.IsStatic )
                        {
                            d = Delegate.CreateDelegate( entry.DelegateType, entry.Method );
                        }
                        else if( entry.Target != null )
                        {
                            d = Delegate.CreateDelegate( entry.DelegateType, entry.Target, entry.Method );
                        }

                        if( d != null ) delegates.Add( d );
                    }
                    catch( Exception ex )
                    {
                        UnityEngine.Debug.LogWarning( $"Failed to deserialize delegate: {ex.Message}" );
                    }
                }
            }

            if( delegates.Count == 0 ) return null;
            if( delegates.Count == 1 ) return delegates[0];
            return Delegate.Combine( delegates.ToArray() );
        }

        public struct DelegateEntry
        {
            public object Target;
            public MethodInfo Method;
            public Type DelegateType;
        }

        private struct DelegateEntryMemberInfo : IMemberInfo
        {
            private int _index;
            // We use a custom descriptor for the Entry struct, cached statically to avoid allocation
            private static readonly IDescriptor _entryDescriptor;

            static DelegateEntryMemberInfo()
            {
                _entryDescriptor = new MemberwiseDescriptor<DelegateEntry>()
                    .WithMember( "target", ContextRegistry.GetID( typeof( Ctx.Ref ) ), e => e.Target, ( ref DelegateEntry e, object v ) => e.Target = v )
                    .WithMember( "method", e => e.Method, ( ref DelegateEntry e, MethodInfo v ) => e.Method = v )
                    .WithMember( "type", e => e.DelegateType, ( ref DelegateEntry e, Type v ) => e.DelegateType = v );
            }

            public readonly string Name => null;
            public readonly int Index => _index;
            public readonly Type MemberType => typeof( DelegateEntry );
            public readonly IDescriptor TypeDescriptor => _entryDescriptor;
            public readonly bool RequiresWriteBack => true;

            public DelegateEntryMemberInfo( int index )
            {
                _index = index;
            }

            public ContextKey GetContext( object target ) => default;

            public object GetValue( object target )
            {
                if( target is Delegate del )
                {
                    var d = del.GetInvocationList()[_index];
                    return new DelegateEntry { Target = d.Target, Method = d.Method, DelegateType = del.GetType() };
                }
                return ((object[])target)[_index]; // Return boxed struct from buffer
            }

            public void SetValue( ref object target, object value )
            {
                if( target is object[] buffer )
                {
                    buffer[_index] = value;
                }
            }
        }
    }
}