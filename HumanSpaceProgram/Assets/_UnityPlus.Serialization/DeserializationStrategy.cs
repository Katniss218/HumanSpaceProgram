using System;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public class DeserializationStrategy : IOperationStrategy
    {
        public void InitializeRoot( Type declaredType, ContextKey context, object root, SerializedData rootData, SerializationState state )
        {
            MemberResolutionResult result = TryProcessMember( rootData, declaredType, context, null, state,
                out object actualValue, out IDescriptor actualDescriptor, out SerializedData unwrappedNode, out Guid? pendingId );

            if( result == MemberResolutionResult.Resolved )
            {
                state.RootResult = actualValue;
                return;
            }

            if( result == MemberResolutionResult.RequiresPush )
            {
                PushCursor( root, null, null, actualDescriptor, unwrappedNode, pendingId, false, state );
                return;
            }

            if( result == MemberResolutionResult.Failed )
            {
                throw new UPSMemberResolutionException( state.Context, $"Failed to resolve the root object.", "root", actualDescriptor, null, "Initialize Root", null );
            }

            throw new UPSSerializationException( state.Context, "Root object cannot be a reference to an unresolved object.", "root", actualDescriptor, null, "Initialize Root", null );
        }

        public SerializationCursorResult Process( ref SerializationCursor cursor, SerializationState state )
        {
            switch( cursor.Phase )
            {
                case SerializationCursorPhase.PreProcessing:
                    return PhasePreProcessing( ref cursor, state );
                case SerializationCursorPhase.Construction:
                    return PhaseConstruction( ref cursor, state );
                case SerializationCursorPhase.Instantiation:
                    return PhaseInstantiation( ref cursor, state );
                case SerializationCursorPhase.Population:
                    return PhasePopulation( ref cursor, state );
                case SerializationCursorPhase.PostProcessing:
                    return PhasePostProcessing( ref cursor, state );
                default:
                    throw new InvalidOperationException( $"Invalid cursor phase for deserialization: {cursor.Phase}" );
            }
        }

        public void OnCursorFinished( SerializationCursor cursor, SerializationState state )
        {
            if( cursor.TargetObj.IsRoot )
            {
                state.RootResult = cursor.TargetObj.Target;
            }
        }

        private static SerializationCursorResult PhasePreProcessing( ref SerializationCursor cursor, SerializationState state )
        {
            if( cursor.Descriptor is ICollectionDescriptor )
            {
#warning TODO - this causes incorrect handling of wrapped collections where expected = unwrapped. see the point lower about swallowing this silently for more details. The CreateInitialTarget is not even invoked.
                if( cursor.DataNode is SerializedArray arr )
                {
                    cursor.PopulationStepCount = arr.Count;
                }
                else
                {
                    cursor.PopulationStepCount = 0;
                }
            }
            else if( cursor.Descriptor is ICompositeDescriptor compDesc )
            {
                cursor.MemberEnumerator = compDesc.GetMemberEnumerator( cursor.TargetObj.Target );

                if( cursor.MemberEnumerator == null )
                {
                    cursor.ConstructionStepCount = compDesc.GetConstructionStepCount( cursor.TargetObj.Target );
                    cursor.PopulationStepCount = compDesc.GetStepCount( cursor.TargetObj.Target ) - cursor.ConstructionStepCount;
                }
            }

            cursor.StepIndex = 0;

            if( cursor.ConstructionStepCount > 0 )
            {
                cursor.ConstructionBuffer = new object[cursor.ConstructionStepCount];
                cursor.Phase = SerializationCursorPhase.Construction;
            }
            else
            {
                cursor.Phase = SerializationCursorPhase.Instantiation;
            }

            return SerializationCursorResult.Jump;
        }

        private static SerializationCursorResult PhaseConstruction( ref SerializationCursor cursor, SerializationState state )
        {
            if( cursor.StepIndex >= cursor.ConstructionStepCount )
            {
                cursor.Phase = SerializationCursorPhase.Instantiation;
                return SerializationCursorResult.Jump;
            }

            var parentDesc = (ICompositeDescriptor)cursor.Descriptor;
            IMemberInfo member = parentDesc.GetMemberInfo( cursor.StepIndex );

            if( member == null )
            {
                return SerializationCursorResult.Advance;
            }

            if( !TryGetDataNode( cursor.DataNode, member.Name, cursor.StepIndex, out SerializedData childNode ) )
            {
                return HandleMissingConstructionArgument( ref cursor, member, state );
            }

            ContextKey context = member.GetContext( cursor.ConstructionBuffer );

            MemberResolutionResult result = TryProcessMember( childNode, member.DeclaredType, context, member, state,
                out object actualValue, out IDescriptor actualDescriptor, out SerializedData unwrappedNode, out Guid? pendingId );

            if( result == MemberResolutionResult.Resolved )
            {
                object buffer = cursor.ConstructionBuffer;
                try
                {
                    member.SetValue( ref buffer, actualValue );
                }
                catch( Exception ex )
                {
                    WrapException( ex, state, "Setting Construction Argument", cursor.Descriptor, member );
                }
                return SerializationCursorResult.Advance;
            }

            if( result == MemberResolutionResult.RequiresPush )
            {
                PushChildCursor( ref cursor, member, actualDescriptor, unwrappedNode, pendingId, true, state );
                return SerializationCursorResult.Push;
            }

            if( result == MemberResolutionResult.Deferred )
            {
                state.Context.EnqueueDeferredConstruction( cursor.TargetObj.Parent, cursor.TargetObj.Member, cursor.DataNode, cursor.Descriptor, cursor.ConstructionBuffer, cursor.StepIndex );
                return SerializationCursorResult.Deferred;
            }

            Debug.LogWarning( $"Failed to resolve construction argument {member.Name}" );
            return SerializationCursorResult.Advance;
        }

        private static SerializationCursorResult HandleMissingConstructionArgument( ref SerializationCursor cursor, IMemberInfo memberInfo, SerializationState state )
        {
            object buffer = cursor.ConstructionBuffer;
            if( cursor.TargetObj.Target != null )
            {
                try
                {
                    object existingVal = memberInfo.GetValue( cursor.TargetObj.Target );
                    memberInfo.SetValue( ref buffer, existingVal );
                    return SerializationCursorResult.Advance;
                }
                catch { }
            }

            try
            {
                memberInfo.SetValue( ref buffer, null );
            }
            catch( Exception ex )
            {
                WrapException( ex, state, "Setting Default Construction Argument", cursor.Descriptor, memberInfo );
            }
            return SerializationCursorResult.Advance;
        }

        private static SerializationCursorResult PhaseInstantiation( ref SerializationCursor cursor, SerializationState state )
        {
            var compositeDesc = (ICompositeDescriptor)cursor.Descriptor;

            try
            {
                if( cursor.ConstructionBuffer != null )
                {
                    object newInstance = compositeDesc.Construct( cursor.ConstructionBuffer );
                    cursor.TargetObj = cursor.TargetObj.WithTarget( newInstance );
                }
                else if( cursor.TargetObj.Target == null )
                {
                    object newInstance = compositeDesc.CreateInitialTarget( cursor.DataNode, state.Context );
                    cursor.TargetObj = cursor.TargetObj.WithTarget( newInstance );
                }
            }
            catch( Exception ex )
            {
                WrapException( ex, state, (cursor.ConstructionBuffer != null) ? "Constructing Object" : "Creating Initial Target", cursor.Descriptor, null );
            }

            if( cursor.TargetObj.Target == null )
            {
                WrapException( new InvalidOperationException( "Factory returned null." ), state, "Constructing Object", cursor.Descriptor, null );
            }

            if( cursor.Descriptor is ICollectionDescriptor colDesc && cursor.DataNode is SerializedArray arr )
            {
                try
                {
                    object resized = colDesc.Resize( cursor.TargetObj.Target, arr.Count );
                    cursor.TargetObj = cursor.TargetObj.WithTarget( resized );
                }
                catch( Exception ex )
                {
                    WrapException( ex, state, "Resizing Collection", cursor.Descriptor, null );
                }
            }

            if( cursor.PendingID.HasValue )
                state.Context.ForwardMap.SetObj( cursor.PendingID.Value, cursor.TargetObj.Target );

            if( cursor.TargetObj.IsRoot && state.Stack.Count == 1 )
                state.RootResult = cursor.TargetObj.Target;

            if( cursor.Descriptor is ISerializationCallbackDescriptor callbackDesc )
            {
                callbackDesc.OnDeserializing( cursor.TargetObj.Target, state.Context );
            }

            cursor.Phase = SerializationCursorPhase.Population;
            cursor.StepIndex = 0;
            return SerializationCursorResult.Jump;
        }

        private static SerializationCursorResult PhasePopulation( ref SerializationCursor cursor, SerializationState state )
        {
            if( cursor.StepIndex >= cursor.PopulationStepCount ) // Finished.
            {
                cursor.Phase = SerializationCursorPhase.PostProcessing;
                return SerializationCursorResult.Jump;
            }

            int memberIndex = cursor.StepIndex + cursor.ConstructionStepCount;
            IMemberInfo member = ((ICompositeDescriptor)cursor.Descriptor).GetMemberInfo( memberIndex );

            if( member == null )
            {
                return SerializationCursorResult.Advance;
            }

            if( !TryGetDataNode( cursor.DataNode, member.Name, memberIndex, out SerializedData memberData ) )
            {
                return SerializationCursorResult.Advance;
            }

            ContextKey context = member.GetContext( cursor.TargetObj.Target );

            MemberResolutionResult result = TryProcessMember( memberData, member.DeclaredType, context, member, state,
                out object resolvedValue, out IDescriptor resolvedDescriptor, out SerializedData unwrappedNode, out Guid? pendingId );

            if( result == MemberResolutionResult.Resolved )
            {
                object target = cursor.TargetObj.Target;
                try
                {
                    member.SetValue( ref target, resolvedValue );
                }
                catch( Exception ex )
                {
                    WrapException( ex, state, "Setting Member Value", cursor.Descriptor, member );
                }

                cursor.TargetObj = cursor.TargetObj.WithTarget( target );
                return SerializationCursorResult.Advance;
            }

            if( result == MemberResolutionResult.RequiresPush )
            {
                PushChildCursor( ref cursor, member, resolvedDescriptor, unwrappedNode, pendingId, false, state );
                return SerializationCursorResult.Push;
            }

            if( result == MemberResolutionResult.Deferred )
            {
                state.Context.EnqueueDeferred( cursor.TargetObj.Target, member, memberData );
                return SerializationCursorResult.Advance;
            }
#warning TODO - handle members that are IDisposable / custom-disposable (e.g. gameobjects are disposable but do not implement IDisposable). automatic cleanup is nice QOL.

            return SerializationCursorResult.Advance;
        }

        private static SerializationCursorResult PhasePostProcessing( ref SerializationCursor cursor, SerializationState state )
        {
            if( cursor.Descriptor is ISerializationCallbackDescriptor callbackDesc )
            {
                callbackDesc.OnDeserialized( cursor.TargetObj.Target, state.Context );
            }
            return SerializationCursorResult.Finished;
        }



        private static MemberResolutionResult TryProcessMember(
            SerializedData node,
            Type declaredType,
            ContextKey context,
            IMemberInfo memberInfo,
            SerializationState state,
            out object actualValue,
            out IDescriptor actualDescriptor,
            out SerializedData data,
            out Guid? pendingId )
        {
            actualValue = null;
#warning TODO - optimize - only get if actual type differs
            actualDescriptor = TypeDescriptorRegistry.GetDescriptor( declaredType, context );
            data = node;
            pendingId = null;

            // 1. Null check.
            if( node == null )
            {
                return MemberResolutionResult.Resolved;
            }

            // 3. Type resolution.
            Type actualType = declaredType;

            if( node is SerializedObject typeObj )
            {
                if( Persistent_Guid.TryReadIdHeader( typeObj, out Guid id ) )
                {
                    pendingId = id;
                }

                if( Persistent_Type.TryReadTypeName( typeObj, out string typeName ) )
                {
                    Type resolvedType = state.Context.Config.TypeResolver.ResolveType( typeName );
                    if( resolvedType == null )
                    {
                        string path = state.Stack.BuildPath();
                        string msg = $"Unable to resolve type name '{typeName}' specified in data at '{path}'.";
                        throw new UPSTypeResolutionException( state.Context, msg, path, actualDescriptor, memberInfo, "Resolve Polymorphism", null );
                    }
                    if( !declaredType.IsAssignableFrom( resolvedType ) )
                    {
                        string path = state.Stack.BuildPath();
                        string msg = $"Data specifies type '{resolvedType.FullName}', which is not assignable to the expected type '{declaredType.AssemblyQualifiedName}'.";
                        throw new UPSTypeResolutionException( state.Context, msg, path, actualDescriptor, memberInfo, "Resolve Polymorphism", null );
                    }
                    actualType = resolvedType;
                }
            }

            actualDescriptor = TypeDescriptorRegistry.GetDescriptor( actualType, context );

            // 4. Structure determination & unwrapping.
            ObjectStructure expectedStructure = actualDescriptor.DetermineObjectStructure( declaredType, actualType, state.Context.Config, out _, out _ );

            SerializedData wrappedNode = null;
            if( node is SerializedObject wrapperObj )
            {
                bool flag = wrapperObj.TryGetValue( KeyNames.VALUE, out wrappedNode );
                if( expectedStructure == ObjectStructure.Wrapped && flag )
                {
                    data = wrappedNode;
                }
                else if( expectedStructure == ObjectStructure.InlineMetadata )
                {
                    data = wrapperObj;
                }
            }

            if( data == null ) // wrapped null.
            {
                return MemberResolutionResult.Resolved;
            }

            if( state.Context.Config.WrapperHandling == WrapperHandling.Strict )
            {
#warning TODO - case where expected = unwrapped, actual = wrapped is swallowed silently.
#warning TODO - especially deadly when the wrapper is missing a $type field, because then the actual type is "wrong", which can lead to bad deserializations. 
                // but that means the deserialization should still throw if there should be no wrapper.
                // the main issue is that we can't reliably distinguish a malformed wrapper from a valid unwrapped data.
                // we would have to look if the member has its own member named "value" or not, but then that's annoying and slow.
                if( expectedStructure == ObjectStructure.Wrapped && data == node )
                {
                    throw new UPSInvalidWrapperException( state.Context, $"Expected a wrapper data object with '{KeyNames.VALUE}' for type {actualType.FullName}, but found unwrapped data.", state.Stack.BuildPath(), actualDescriptor, memberInfo, "Processing Value", null );
                }
#warning INFO - kinda ugly "fix" but it works.
                if( expectedStructure == ObjectStructure.Unwrapped && actualDescriptor is ICollectionDescriptor && wrappedNode != null ) // wrappedNode implicitly checks for node is SerializedObject as well.
                {
                    throw new UPSInvalidWrapperException( state.Context, $"Expected unwrapped data for collection type {actualType.FullName}, but found wrapped data.", state.Stack.BuildPath(), actualDescriptor, memberInfo, "Processing Value", null );
                }
            }

            // 5. Deserialization.
            if( actualDescriptor is IPrimitiveDescriptor primitiveDesc )
            {
                DeserializationResult result = default;
                try
                {
#warning TODO - descriptor can "choose" to swallow exceptions and return a 'failure' state. we need to handle this.
                    result = primitiveDesc.DeserializeDirect( data, state.Context, out actualValue );
                }
                catch( Exception ex )
                {
                    actualValue = null;
                    WrapException( ex, state, "Deserializing Primitive", actualDescriptor, memberInfo );
                }

                if( result == DeserializationResult.Success )
                {
                    if( pendingId.HasValue && actualValue != null )
                    {
                        state.Context.ForwardMap.SetObj( pendingId.Value, actualValue );
                    }
                    return MemberResolutionResult.Resolved;
                }
                if( result == DeserializationResult.Deferred )
                    return MemberResolutionResult.Deferred;
                return MemberResolutionResult.Failed;
            }

            // 6. Handle auto-references.
            if( actualDescriptor is ICompositeDescriptor )
            {
                if( node is SerializedObject refObj && Persistent_Guid.TryReadRefHeader( refObj, out Guid refGuid ) )
                {
                    if( state.Context.ForwardMap.TryGetObj( refGuid, out object existingObj ) )
                    {
                        actualValue = existingObj;
                        return MemberResolutionResult.Resolved;
                    }
                    return MemberResolutionResult.Deferred;
                }

                if( data is SerializedPrimitive )
                {
                    throw new UPSDataMismatchException( state.Context, $"Expected either a {nameof( SerializedObject )} or a {nameof( SerializedArray )}  for type {actualType.FullName}, but found primitive.", state.Stack.BuildPath(), actualDescriptor, memberInfo, "Processing Value", null );
                }

                return MemberResolutionResult.RequiresPush;
            }

            throw new UPSSerializationException( state.Context, $"Unsupported descriptor type: {actualDescriptor.GetType().FullName}", state.Stack.BuildPath(), actualDescriptor, memberInfo, "Processing Value", null );
        }

        private static void PushCursor( object target, object parent, IMemberInfo memberInfo, IDescriptor descriptor, SerializedData dataNode, Guid? pendingId, bool writeBackOnPop, SerializationState state )
        {
            var cursor = new SerializationCursor()
            {
                TargetObj = new TrackedObject( target, parent, memberInfo ),
                Descriptor = descriptor,
                DataNode = dataNode,
                PendingID = pendingId,
                Phase = SerializationCursorPhase.PreProcessing,
                StepIndex = 0,
                WriteBackOnPop = writeBackOnPop
            };

            state.Stack.Push( cursor );
        }

        private static void PushChildCursor( ref SerializationCursor parentCursor, IMemberInfo memberInfo, IDescriptor resolvedDescriptor, SerializedData unwrappedNode, Guid? pendingId, bool isConstructionPhase, SerializationState state )
        {
            object parentObj = isConstructionPhase ? (object)parentCursor.ConstructionBuffer : parentCursor.TargetObj.Target;

            object existingChild = null;
            if( !isConstructionPhase && parentObj != null )
            {
                try
                {
                    existingChild = memberInfo.GetValue( parentObj );
                }
                catch { }
            }

            PushCursor( existingChild, parentObj, memberInfo, resolvedDescriptor, unwrappedNode, pendingId, true, state );
        }

        private static bool TryGetDataNode( SerializedData parent, string key, int index, out SerializedData data )
        {
            data = null;
            if( parent is SerializedObject obj && key != null )
                return obj.TryGetValue( key, out data );
            if( parent is SerializedArray arr && index >= 0 && index < arr.Count )
            {
                data = arr[index];
                return true;
            }
            return false;
        }

        private static void WrapException( Exception ex, SerializationState state, string operation, IDescriptor descriptor, IMemberInfo member )
        {
            if( ex is UPSSerializationException )
                throw ex;

            string path = state.Stack.BuildPath();
            string message = $"Deserialization Error: {operation} at '{path}': {ex.Message}";

            throw new UPSSerializationException( state.Context, message, path, descriptor, member, operation, ex );
        }
    }
}