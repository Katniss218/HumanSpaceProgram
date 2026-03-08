using System;

namespace UnityPlus.Serialization
{
    public class SerializationStrategy : IOperationStrategy
    {
        public void InitializeRoot( object root, IDescriptor rootDescriptor, SerializedData rootData, SerializationState state )
        {
            if( root == null )
            {
                SerializedData d = null;
                state.RootResult = d;
                return;
            }

            // [Primitive Root Support]
            if( rootDescriptor is IPrimitiveDescriptor primitiveRoot )
            {
                SerializedData d = null;
                try
                {
                    primitiveRoot.SerializeDirect( root, ref d, state.Context );
                }
                catch( Exception ex )
                {
                    WrapException( ex, state, "Serializing Primitive Root", rootDescriptor, null );
                }
                state.RootResult = d;
                return; // Stack remains empty, IsFinished = true
            }

            // Create Root Node if needed
            SerializedData createdNode = rootData;
            if( createdNode == null )
            {
                if( rootDescriptor is ICollectionDescriptor )
                {
                    createdNode = SerializationHelpers.CreateCollectionNode(
                        root,
                        state.Context.ReverseMap,
                        state.Context.Config.ForceStandardJson,
                        out SerializedArray arrayNode );

                    // For collections, the DataNode in the cursor tracks the array we are populating
                    // But we store the wrapper (if exists) in RootResult
                    state.RootResult = createdNode;

                    // If we created a wrapper, the cursor data node should be the inner array
                    if( createdNode != arrayNode )
                        createdNode = arrayNode;
                }
                else
                {
                    var objNode = new SerializedObject();
                    if( root != null && !rootDescriptor.MappedType.IsValueType )
                    {
                        Guid rootId = state.Context.ReverseMap.GetID( root );
                        Persistent_Guid.WriteIdHeader( objNode, rootId );
                    }
                    createdNode = objNode;
                    state.RootResult = createdNode;
                }
            }
            else
            {
                state.RootResult = rootData;
            }

            var cursor = new SerializationCursor
            {
                TargetObj = new TrackedObject( root ),
                Descriptor = rootDescriptor,
                StepIndex = 0,
                DataNode = createdNode,
                Phase = SerializationCursorPhase.PreProcessing,
                WriteBackOnPop = false // Serialization is read-only for the object graph
            };

            state.Stack.Push( cursor );
        }

        public SerializationCursorResult Process( ref SerializationCursor cursor, SerializationState state )
        {
            switch( cursor.Phase )
            {
                case SerializationCursorPhase.PreProcessing:
                    return PhasePreProcessing( ref cursor, state );
                case SerializationCursorPhase.Population:
                    return PhasePopulation( ref cursor, state );
                case SerializationCursorPhase.PostProcessing:
                    return SerializationCursorResult.Finished;
                default:
                    // Serialize doesn't use construction/instantiation
                    throw new Exception( $"Invalid cursor phase for serialization: {cursor.Phase}" );
            }
        }

        public void OnCursorFinished( SerializationCursor cursor, SerializationState state )
        {
        }

        private static SerializationCursorResult PhasePreProcessing( ref SerializationCursor cursor, SerializationState state )
        {
            if( cursor.TargetObj.Target != null )
            {
                // 1. Polymorphism
                if( cursor.DataNode is SerializedObject objNode )
                {
                    HandlePolymorphism( cursor.TargetObj.Target, cursor.Descriptor.MappedType, objNode, ref cursor.Descriptor );
                }

                // 2. Lifecycle Callback
                if( cursor.Descriptor is ICompositeDescriptor comp )
                {
                    comp.OnSerializing( cursor.TargetObj.Target, state.Context );
                }

                // 3. Cycle Tracking
                if( !cursor.Descriptor.MappedType.IsValueType )
                {
                    state.VisitedObjects.Add( cursor.TargetObj.Target );
                }

                // 4. Counts & Enumerators
                if( cursor.Descriptor is ICompositeDescriptor newComposite )
                {
                    cursor.MemberEnumerator = newComposite.GetMemberEnumerator( cursor.TargetObj.Target );
                    if( cursor.MemberEnumerator == null )
                    {
                        cursor.ConstructionStepCount = newComposite.GetConstructionStepCount( cursor.TargetObj.Target );
                        cursor.PopulationStepCount = newComposite.GetStepCount( cursor.TargetObj.Target ) - cursor.ConstructionStepCount;
                    }
                }
            }

            cursor.Phase = SerializationCursorPhase.Population;
            cursor.StepIndex = 0;
            return SerializationCursorResult.Jump; // Phase change
        }

        private static SerializationCursorResult PhasePopulation( ref SerializationCursor cursor, SerializationState state )
        {
            IMemberInfo memberInfo;
            int activeStepIndex = cursor.StepIndex;

            if( cursor.MemberEnumerator != null )
            {
                if( !cursor.MemberEnumerator.MoveNext() )
                {
                    cursor.Phase = SerializationCursorPhase.PostProcessing;
                    return SerializationCursorResult.Jump;
                }
                memberInfo = cursor.MemberEnumerator.Current;
                // Note: For enumeration, we still increment StepIndex (Advance) to track count/progress, 
                // even though 'activeStepIndex' isn't used for lookups in this mode.
            }
            else
            {
                if( cursor.StepIndex >= cursor.PopulationStepCount )
                {
                    cursor.Phase = SerializationCursorPhase.PostProcessing;
                    return SerializationCursorResult.Jump;
                }

                var parentDesc = (ICompositeDescriptor)cursor.Descriptor; // Cast should be true because primitives are handled inline.
                activeStepIndex = cursor.StepIndex + cursor.ConstructionStepCount; // Ensures that we don't use buffers when serializing.
                memberInfo = parentDesc.GetMemberInfo( activeStepIndex );
            }

            if( memberInfo == null || (memberInfo.TypeDescriptor == null && TypeDescriptorRegistry.GetDescriptor( memberInfo.MemberType, memberInfo.GetContext( cursor.TargetObj.Target ) ) == null) ) // Skipped
            {
                return SerializationCursorResult.Advance;
            }

            MemberResolutionResult result = TryResolveMember( memberInfo, cursor.TargetObj.Target, cursor.DataNode, activeStepIndex, state, out object childTarget, out IDescriptor childDesc, out SerializedData childNode );

            if( result == MemberResolutionResult.Resolved )
            {
                return SerializationCursorResult.Advance;
            }
            else if( result == MemberResolutionResult.RequiresPush )
            {
                PushChildCursor( ref cursor, childTarget, childDesc, childNode, memberInfo, state );
                return SerializationCursorResult.Push; // Driver increments
            }

            return SerializationCursorResult.Advance;
        }

        private static MemberResolutionResult TryResolveMember( IMemberInfo memberInfo, object target, SerializedData parentData, int index, SerializationState state, out object childTarget, out IDescriptor childDesc, out SerializedData childNode )
        {
            childTarget = null;
            childDesc = null;
            childNode = null;

            // 1. Get Value
            object val = null;
            try
            {
                val = memberInfo.GetValue( target );
            }
            catch( Exception ex )
            {
                WrapException( ex, state, "Getting Member Value", null, memberInfo );
            }

            // 2. Null
            if( val == null )
            {
                LinkDataNode( parentData, memberInfo.Name, null, index );
                return MemberResolutionResult.Resolved;
            }

            // 3. Polymorphism
            IDescriptor descriptor = memberInfo.TypeDescriptor;
            Type declaredType = memberInfo.MemberType;
            Type actualType = val.GetType();

            if( declaredType != actualType )
            {
                descriptor = TypeDescriptorRegistry.GetDescriptor( actualType, memberInfo.GetContext( target ) );
            }
            if( descriptor == null ) // types match, but was not given from memberinfo.
            {
                descriptor = TypeDescriptorRegistry.GetDescriptor( memberInfo.MemberType, memberInfo.GetContext( target ) );
            }

            // 4. Primitive
            if( descriptor is IPrimitiveDescriptor primitiveDesc )
            {
                SerializedData primitiveData = null;
                try
                {
                    primitiveDesc.SerializeDirect( val, ref primitiveData, state.Context );
                }
                catch( Exception ex )
                {
                    WrapException( ex, state, "Serializing Primitive", descriptor, memberInfo );
                }
                LinkDataNode( parentData, memberInfo.Name, primitiveData, index );
                return MemberResolutionResult.Resolved;
            }

            // 5. Composite
            childTarget = val;
            childDesc = descriptor;

            bool isCollection = descriptor is ICollectionDescriptor;
            SerializedData nodeToLink;

            if( isCollection )
            {
                nodeToLink = SerializationHelpers.CreateCollectionNode(
                    val,
                    state.Context.ReverseMap,
                    state.Context.Config.ForceStandardJson,
                    out SerializedArray arrayNode );

                childNode = arrayNode;

                // Cycle detection for collection wrapper
                if( nodeToLink is SerializedObject wrapper )
                {
                    if( TryResolveReference( val, state, out SerializedData refNode ) )
                    {
                        LinkDataNode( parentData, memberInfo.Name, refNode, index );
                        return MemberResolutionResult.Resolved;
                    }
                    state.VisitedObjects.Add( val );
                }
            }
            else
            {
                var objNode = new SerializedObject();
                nodeToLink = objNode;
                childNode = objNode;

                // Cycle Check (Always for objects)
                if( !actualType.IsValueType )
                {
                    if( TryResolveReference( val, state, out SerializedData refNode ) )
                    {
                        LinkDataNode( parentData, memberInfo.Name, refNode, index );
                        return MemberResolutionResult.Resolved;
                    }
                    state.VisitedObjects.Add( val );

                    Guid id = state.Context.ReverseMap.GetID( val );
                    Persistent_Guid.WriteIdHeader( objNode, id );
                }

                // Write Type Header
                if( !declaredType.IsValueType && !declaredType.IsSealed && declaredType != actualType && !typeof( Delegate ).IsAssignableFrom( declaredType ) )
                {
                    Persistent_Type.WriteTypeHeader( objNode, actualType );
                }
            }

            LinkDataNode( parentData, memberInfo.Name, nodeToLink, index );
            return MemberResolutionResult.RequiresPush;
        }

        private static bool TryResolveReference( object target, SerializationState state, out SerializedData refNode )
        {
            refNode = null;
            if( state.VisitedObjects.Contains( target ) )
            {
                Guid id = state.Context.ReverseMap.GetID( target );
                refNode = new SerializedObject { { KeyNames.REF, id.SerializeGuid() } };
                return true;
            }
            return false;
        }

        private static void PushChildCursor( ref SerializationCursor parentCursor, object childTarget, IDescriptor childDesc, SerializedData childNode, IMemberInfo memberInfo, SerializationState state )
        {
            var childCursor = new SerializationCursor
            {
                TargetObj = new TrackedObject( childTarget, parentCursor.TargetObj.Target, memberInfo ),
                Descriptor = childDesc,
                DataNode = childNode,
                StepIndex = 0,
                PopulationStepCount = (childDesc as ICompositeDescriptor)?.GetStepCount( childTarget ) ?? 0,
                Phase = SerializationCursorPhase.PreProcessing,
                WriteBackOnPop = false // Serializer is read-only
            };
            state.Stack.Push( childCursor );
        }

        private static void LinkDataNode( SerializedData parent, string key, SerializedData child, int index )
        {
            if( parent is SerializedObject obj && key != null )
                obj[key] = child;
            else if( parent is SerializedArray arr )
            {
                if( index >= arr.Count )
                    arr.Add( child );
                else
                    arr[index] = child;
            }
        }

        private static void HandlePolymorphism( object target, Type declaredType, SerializedObject dataNode, ref IDescriptor descriptor )
        {
            if( target == null )
                return;
            Type actualType = target.GetType();

            if( actualType != declaredType )
            {
                descriptor = TypeDescriptorRegistry.GetDescriptor( actualType );
            }

            if( descriptor is IPrimitiveDescriptor )
                return;
            if( declaredType.IsValueType || declaredType.IsSealed )
                return;
            if( declaredType == actualType )
                return;
            if( typeof( Delegate ).IsAssignableFrom( declaredType ) )
                return;

            Persistent_Type.WriteTypeHeader( dataNode, actualType );
        }

        private static void WrapException( Exception ex, SerializationState state, string operation, IDescriptor descriptor, IMemberInfo member )
        {
            if( ex is UPSSerializationException )
                throw ex;

            string path = state.Stack.BuildPath();
            string message = $"Serialization Error: {operation} at '{path}': {ex.Message}";

            throw new UPSSerializationException( state.Context, message, path, descriptor, member, operation, ex );
        }
    }
}