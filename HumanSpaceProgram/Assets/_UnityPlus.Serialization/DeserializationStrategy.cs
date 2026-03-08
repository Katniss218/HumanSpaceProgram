using System;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public class DeserializationStrategy : IOperationStrategy
    {
        public void InitializeRoot( object root, IDescriptor rootDescriptor, SerializedData rootData, SerializationState state )
        {
            if( rootData == null )
            {
                state.RootResult = null;
                return;
            }

            // Resolve Polymorphism for Root
            if( rootData is SerializedObject rootObj )
            {
                Type actualType = Persistent_Type.ReadTypeHeader( rootObj, state.Context.Config.TypeResolver );
                if( actualType != null && actualType != rootDescriptor.MappedType )
                {
                    if( !rootDescriptor.MappedType.IsAssignableFrom( actualType ) )
                    {
                        string msg = $"Polymorphic type mismatch: Data specifies type '{actualType.FullName}', which is not assignable to root expected type '{rootDescriptor.MappedType.FullName}'.";
                        throw new UPSSerializationException( state.Context, msg, "root", rootDescriptor, null, "Resolve Polymorphism", null );
                    }

                    if( root != null )
                    {
                        Type existingType = root.GetType();
                        if( actualType != existingType && !existingType.IsAssignableFrom( actualType ) )
                        {
                            string msg = $"Polymorphic type mismatch: Data specifies type '{actualType.FullName}', which is incompatible with existing root target of type '{existingType.FullName}'.";
                            throw new UPSSerializationException( state.Context, msg, "root", rootDescriptor, null, "Resolve Polymorphism", null );
                        }
                    }

                    rootDescriptor = TypeDescriptorRegistry.GetDescriptor( actualType, ContextKey.Default );
                }
            }

            // [Primitive Root]
            if( rootDescriptor is IPrimitiveDescriptor primitiveRoot )
            {
                if( primitiveRoot.DeserializeDirect( rootData, state.Context, out object res ) == DeserializationResult.Success )
                    state.RootResult = res;
                return;
            }

            var cursor = new SerializationCursor
            {
                TargetObj = new TrackedObject( root ), // root Parent is null
                Descriptor = rootDescriptor,
                StepIndex = 0,
                DataNode = rootData,
                Phase = SerializationCursorPhase.PreProcessing,
                WriteBackOnPop = false // Root has no parent to write back to
            };

            state.Stack.Push( cursor );
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
                    return SerializationCursorResult.Finished;
            }
        }

        public void OnCursorFinished( SerializationCursor cursor, SerializationState state )
        {
            // CRITICAL FIX: If the root object was a Value Type (struct) or the reference was swapped
            // (e.g. array resize, or re-boxing during population), the 'TargetObj.Target' inside
            // the finished cursor holds the FINAL version. We must update the state result.
            if( cursor.TargetObj.IsRoot )
            {
                state.RootResult = cursor.TargetObj.Target;
            }
        }

        private static SerializationCursorResult PhasePreProcessing( ref SerializationCursor cursor, SerializationState state )
        {
            // Capture ID before unwrapping collections (e.g. List wrapped in Object with $id)
            if( cursor.DataNode is SerializedObject objNode && Persistent_Guid.TryReadIdHeader( objNode, out Guid preId ) )
            {
                cursor.PendingID = preId;
            }

#warning TODO - potentially move this to the descriptor itself, since this is somewhat descriptor-specific (formatting).
            if( cursor.Descriptor is ICollectionDescriptor )
            {
                var valNode = SerializationHelpers.GetValueNode( cursor.DataNode, state.Context.Config.ForceStandardJson );
                if( valNode != null )
                    cursor.DataNode = valNode;
            }

            var compDesc = (ICompositeDescriptor)cursor.Descriptor;
            cursor.ConstructionStepCount = compDesc.GetConstructionStepCount( cursor.TargetObj.Target );

            if( cursor.Descriptor is ICollectionDescriptor && cursor.DataNode is SerializedArray arrNode )
            {
                cursor.PopulationStepCount = arrNode.Count;
            }
            else
            {
                cursor.PopulationStepCount = compDesc.GetStepCount( cursor.TargetObj.Target ) - cursor.ConstructionStepCount;
            }

            // Always enter construction phase if the type requires it (e.g. immutable), 
            // even if we have an existing target (Populate).
            if( cursor.ConstructionStepCount > 0 )
            {
                cursor.ConstructionBuffer = new object[cursor.ConstructionStepCount];
                cursor.Phase = SerializationCursorPhase.Construction;
            }
            else
            {
                cursor.Phase = SerializationCursorPhase.Instantiation;
            }

            cursor.StepIndex = 0;
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
            int activeIndex = cursor.StepIndex;
            IMemberInfo memberInfo = parentDesc.GetMemberInfo( activeIndex );

            if( memberInfo == null )
            {
                return SerializationCursorResult.Advance;
            }

#warning TODO - potentially replace call to construction buffer with just actual type.
            MemberResolutionResult result = TryResolveMember( memberInfo, cursor.ConstructionBuffer, cursor.DataNode, activeIndex, state, out object val );

            if( result == MemberResolutionResult.Resolved )
            {
                object buffer = cursor.ConstructionBuffer;
                try
                {
                    memberInfo.SetValue( ref buffer, val );
                }
                catch( Exception ex )
                {
                    WrapException( ex, state, "Setting Construction Argument", cursor.Descriptor, memberInfo );
                }
                return SerializationCursorResult.Advance;
            }
            else if( result == MemberResolutionResult.Missing )
            {
                // Try to read from existing target if available (Populate)
                if( cursor.TargetObj.Target != null )
                {
                    try
                    {
                        // Attempt to read the value from the existing instance to fill the construction buffer
                        object existingVal = memberInfo.GetValue( cursor.TargetObj.Target );
                        object buffer = cursor.ConstructionBuffer;
                        memberInfo.SetValue( ref buffer, existingVal );
                        return SerializationCursorResult.Advance;
                    }
                    catch { } // Ignore if descriptor doesn't support reading from instance
                }

                // Fallback to null/default
                object buf = cursor.ConstructionBuffer;
                try
                {
                    memberInfo.SetValue( ref buf, null );
                }
                catch( Exception ex )
                {
                    WrapException( ex, state, "Setting Default Construction Argument", cursor.Descriptor, memberInfo );
                }
                return SerializationCursorResult.Advance;
            }
            else if( result == MemberResolutionResult.RequiresPush )
            {
                PushChildCursor( ref cursor, memberInfo, activeIndex, true, state );
                return SerializationCursorResult.Push; // Driver handles parent increment
            }
            else if( result == MemberResolutionResult.Deferred )
            {
                state.Context.DeferredOperations.Enqueue( new DeferredOperation()
                {
                    Target = cursor.TargetObj.Parent,
                    Member = cursor.TargetObj.Member,
                    Data = cursor.DataNode,
                    Descriptor = cursor.Descriptor,
                    ConstructionBuffer = cursor.ConstructionBuffer,
                    ConstructionIndex = cursor.StepIndex
                } );
                return SerializationCursorResult.Deferred;
            }
            else
            {
                Debug.LogWarning( $"Failed to resolve construction argument {memberInfo.Name}" );
                return SerializationCursorResult.Advance;
            }
        }

        private static SerializationCursorResult PhaseInstantiation( ref SerializationCursor cursor, SerializationState state )
        {
            var compositeDesc = (ICompositeDescriptor)cursor.Descriptor;

            // If we constructed a new instance (or forced construction), use it.
            // If we skipped construction (Target != null && StepCount == 0), we keep existing target.
            if( cursor.ConstructionBuffer != null )
            {
                try
                {
                    object newInstance = compositeDesc.Construct( cursor.ConstructionBuffer );
                    cursor.TargetObj = cursor.TargetObj.WithTarget( newInstance );
                }
                catch( Exception ex )
                {
                    WrapException( ex, state, "Constructing Object", cursor.Descriptor, null );
                }
            }
            else if( cursor.TargetObj.Target == null )
            {
                try
                {
                    object newInstance = compositeDesc.CreateInitialTarget( cursor.DataNode, state.Context );
                    cursor.TargetObj = cursor.TargetObj.WithTarget( newInstance );
                }
                catch( Exception ex )
                {
                    WrapException( ex, state, "Creating Initial Target", cursor.Descriptor, null );
                }
            }

            if( cursor.TargetObj.Target == null )
            {
                WrapException( new InvalidOperationException( "Factory returned null." ), state, "Constructing Object", cursor.Descriptor, null );
            }

            // If we are populating an existing collection (Target != null), we need to prepare it.
            // Resize is intended to clear existing contents and prepare for population, but it also serves to resize arrays if needed.
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
            {
                state.Context.ForwardMap.SetObj( cursor.PendingID.Value, cursor.TargetObj.Target );
            }
            else if( cursor.DataNode is SerializedObject objNode && Persistent_Guid.TryReadIdHeader( objNode, out Guid guid ) )
            {
                state.Context.ForwardMap.SetObj( guid, cursor.TargetObj.Target );
            }

            if( cursor.TargetObj.IsRoot && state.Stack.Count == 1 )
            {
                state.RootResult = cursor.TargetObj.Target;
            }

            cursor.Phase = SerializationCursorPhase.Population;
            cursor.StepIndex = 0;
            return SerializationCursorResult.Jump;
        }

        private static SerializationCursorResult PhasePopulation( ref SerializationCursor cursor, SerializationState state )
        {
            if( cursor.TargetObj.Target == null )
            {
                cursor.Phase = SerializationCursorPhase.PostProcessing;
                return SerializationCursorResult.Jump;
            }

            if( cursor.StepIndex >= cursor.PopulationStepCount )
            {
                cursor.Phase = SerializationCursorPhase.PostProcessing;
                return SerializationCursorResult.Jump;
            }

            var parentDesc = (ICompositeDescriptor)cursor.Descriptor;
            int offset = cursor.ConstructionStepCount;
            int absoluteIndex = cursor.StepIndex + offset;

            IMemberInfo memberInfo = parentDesc.GetMemberInfo( absoluteIndex );

            if( memberInfo == null )
            {
                return SerializationCursorResult.Advance;
            }

            MemberResolutionResult result = TryResolveMember( memberInfo, cursor.TargetObj.Target, cursor.DataNode, absoluteIndex, state, out object val );

            if( result == MemberResolutionResult.Resolved )
            {
                object t = cursor.TargetObj.Target;
                try
                {
                    memberInfo.SetValue( ref t, val );
                }
                catch( Exception ex )
                {
                    WrapException( ex, state, "Setting Member Value", cursor.Descriptor, memberInfo );
                }

                // Update cursor target in case of value type replacement
                cursor.TargetObj = cursor.TargetObj.WithTarget( t );
                return SerializationCursorResult.Advance;
            }
            else if( result == MemberResolutionResult.Missing )
            {
                return SerializationCursorResult.Advance; // Ignore missing members (Populate behavior)
            }
            else if( result == MemberResolutionResult.RequiresPush )
            {
                PushChildCursor( ref cursor, memberInfo, absoluteIndex, false, state );
                return SerializationCursorResult.Push; // Driver increments
            }
            else if( result == MemberResolutionResult.Deferred )
            {
                // For population, we can't easily defer if we don't have the data node.
                // But TryResolveMember only returns Deferred if it found a ref.
                // If Missing, it returns Missing.

                // If we are here, we found a ref but it's not resolved yet.
                // We need the DataNode for the deferred operation.
                TryGetDataNode( cursor.DataNode, memberInfo.Name, absoluteIndex, out SerializedData failedData );

                state.Context.EnqueueDeferred( cursor.TargetObj.Target, memberInfo, failedData );
                return SerializationCursorResult.Advance; // Skip member and continue
            }
            else
            {
                return SerializationCursorResult.Advance;
            }
        }

        private static SerializationCursorResult PhasePostProcessing( ref SerializationCursor cursor, SerializationState state )
        {
            if( cursor.TargetObj.Target != null && cursor.Descriptor is ICompositeDescriptor comp )
            {
                comp.OnDeserialized( cursor.TargetObj.Target, state.Context );
            }
            return SerializationCursorResult.Finished;
        }

        private static MemberResolutionResult TryResolveMember( IMemberInfo memberInfo, object target, SerializedData parentData, int index, SerializationState state, out object value )
        {
            IDescriptor memberDesc = memberInfo.TypeDescriptor;
            if( memberDesc == null )
            {
                memberDesc = TypeDescriptorRegistry.GetDescriptor( memberInfo.MemberType, memberInfo.GetContext( target ) );
            }

            if( memberDesc is IPrimitiveDescriptor primitiveDesc )
            {
                if( !TryGetDataNode( parentData, memberInfo.Name, index, out SerializedData primitiveData ) )
                {
                    value = null;
                    return MemberResolutionResult.Missing;
                }

                DeserializationResult result = DeserializationResult.Success;
                try
                {
                    result = primitiveDesc.DeserializeDirect( primitiveData, state.Context, out value );
                }
                catch( Exception ex )
                {
                    value = null;
                    // Can't use WrapException easily here because we don't have the cursor/descriptor handy in the same way,
                    // but we can pass what we have.
                    // We need to re-throw or handle. Since this method returns a result, throwing is appropriate for fatal errors.
                    WrapException( ex, state, "Deserializing Primitive", memberDesc, memberInfo );
                }

                if( result == DeserializationResult.Success )
                    return MemberResolutionResult.Resolved;
                if( result == DeserializationResult.Deferred )
                    return MemberResolutionResult.Deferred;
                return MemberResolutionResult.Failed;
            }

            if( !TryGetDataNode( parentData, memberInfo.Name, index, out SerializedData childNode ) )
            {
                value = null;
                return MemberResolutionResult.Missing;
            }

            if( childNode == null )
            {
                value = null;
                return MemberResolutionResult.Resolved;
            }

            MemberResolutionResult refResult = TryResolveReference( childNode, state, out value );
            if( refResult == MemberResolutionResult.Resolved )
                return MemberResolutionResult.Resolved;
            if( refResult == MemberResolutionResult.Deferred )
                return MemberResolutionResult.Deferred;

            return MemberResolutionResult.RequiresPush;
        }

        private static MemberResolutionResult TryResolveReference( SerializedData data, SerializationState state, out object value )
        {
            value = null;
            if( data is SerializedObject refObj && refObj.TryGetValue( KeyNames.REF, out var refVal ) )
            {
                if( refVal is SerializedPrimitive refPrim && Guid.TryParse( (string)refPrim, out Guid refGuid ) )
                {
                    if( state.Context.ForwardMap.TryGetObj( refGuid, out object existingObj ) )
                    {
                        value = existingObj;
                        return MemberResolutionResult.Resolved;
                    }

                    // Optimistic Deferral: If we can't find it now, assume it's coming later.
                    // If it never shows up, the Driver's deadlock detector will catch it.
                    return MemberResolutionResult.Deferred;
                }
            }
            return MemberResolutionResult.Failed;
        }

        private static void PushChildCursor( ref SerializationCursor parentCursor, IMemberInfo memberInfo, int absoluteIndex, bool isConstructionPhase, SerializationState state )
        {
            TryGetDataNode( parentCursor.DataNode, memberInfo.Name, absoluteIndex, out SerializedData childNode );

            // Determine parent for the child. 
            // If construction phase, parent is the buffer. If population, parent is the actual target.
            object parentObj = isConstructionPhase ? (object)parentCursor.ConstructionBuffer : parentCursor.TargetObj.Target;

            // Resolve Polymorphism for Child
            IDescriptor memberDesc = memberInfo.TypeDescriptor;
            if( memberDesc == null )
            {
                memberDesc = TypeDescriptorRegistry.GetDescriptor( memberInfo.MemberType, memberInfo.GetContext( parentObj ) );
            }

            if( childNode is SerializedObject childObj )
            {
                Type actualType = Persistent_Type.ReadTypeHeader( childObj, state.Context.Config.TypeResolver );
                if( actualType != null && actualType != memberInfo.MemberType )
                {
                    if( !memberInfo.MemberType.IsAssignableFrom( actualType ) )
                    {
                        string path = state.Stack.BuildPath();
                        string msg = $"Polymorphic type mismatch: Data specifies type '{actualType.FullName}', which is not assignable to the member type '{memberInfo.MemberType.AssemblyQualifiedName}'.";
                        throw new UPSSerializationException( state.Context, msg, path, parentCursor.Descriptor, memberInfo, "Resolve Polymorphism", null );
                    }

                    memberDesc = TypeDescriptorRegistry.GetDescriptor( actualType, memberInfo.GetContext( parentObj ) );
                }
            }

            // Optimization: If not construction phase, and parent exists, try to get existing object (PopulateExisting)
            object existingChild = null;
            if( !isConstructionPhase && parentObj != null )
            {
                existingChild = memberInfo.GetValue( parentObj );
            }

            var childCursor = new SerializationCursor
            {
                TargetObj = new TrackedObject( existingChild, parentObj, memberInfo ),
                Descriptor = memberDesc,
                DataNode = childNode,
                Phase = SerializationCursorPhase.PreProcessing,
                StepIndex = 0,
                WriteBackOnPop = true // Always write back deserialized children to their parents
            };

            state.Stack.Push( childCursor );
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