using System;

namespace UnityPlus.Serialization
{
    public class SerializationStrategy : IOperationStrategy
    {
        public void InitializeRoot(  Type declaredType, ContextKey context, object root, SerializedData rootData, SerializationState state )
        {
            MemberResolutionResult result = TryProcessMember( root, declaredType, context, state, out SerializedData node, out IDescriptor actualDescriptor, out ObjectStructure structure );

            state.RootResult = node;

            if( result == MemberResolutionResult.RequiresPush )
            {
                PushCursor( root, null, null, actualDescriptor, structure, node, state );
            }
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
                    return PhasePostProcessing( ref cursor, state );
                default:
                    throw new InvalidOperationException( $"Invalid cursor phase for serialization: {cursor.Phase}" );
            }
        }

        public void OnCursorFinished( SerializationCursor cursor, SerializationState state )
        {
        }

        private static SerializationCursorResult PhasePreProcessing( ref SerializationCursor cursor, SerializationState state )
        {
            if( cursor.Descriptor is ISerializationCallbackDescriptor callbackDesc )
            {
                callbackDesc.OnSerializing( cursor.TargetObj.Target, state.Context );
            }

            if( cursor.Descriptor is ICompositeDescriptor compDesc )
            {
                cursor.MemberEnumerator = compDesc.GetMemberEnumerator( cursor.TargetObj.Target );
                if( cursor.MemberEnumerator == null )
                {
                    cursor.ConstructionStepCount = compDesc.GetConstructionStepCount( cursor.TargetObj.Target );
                    cursor.PopulationStepCount = compDesc.GetStepCount( cursor.TargetObj.Target ) - cursor.ConstructionStepCount;
                }
            }

            cursor.Phase = SerializationCursorPhase.Population;
            cursor.StepIndex = 0;
            return SerializationCursorResult.Jump;
        }

        private static SerializationCursorResult PhasePopulation( ref SerializationCursor cursor, SerializationState state )
        {
            IMemberInfo member;
            int memberIndex;

            if( cursor.MemberEnumerator != null )
            {
                if( !cursor.MemberEnumerator.MoveNext() )
                {
                    cursor.Phase = SerializationCursorPhase.PostProcessing;
                    return SerializationCursorResult.Jump;
                }

                memberIndex = cursor.StepIndex;
                member = cursor.MemberEnumerator.Current;
            }
            else
            {
                if( cursor.StepIndex >= cursor.PopulationStepCount )
                {
                    cursor.Phase = SerializationCursorPhase.PostProcessing;
                    return SerializationCursorResult.Jump;
                }

                memberIndex = cursor.StepIndex + cursor.ConstructionStepCount;
                member = ((ICompositeDescriptor)cursor.Descriptor).GetMemberInfo( memberIndex );
            }

            if( member == null )
            {
                return SerializationCursorResult.Advance;
            }

            object value = null;

            try
            {
                value = member.GetValue( cursor.TargetObj.Target );
            }
            catch( Exception ex )
            {
                WrapException( ex, state, "Getting Member Value", cursor.Descriptor, member );
            }

            ContextKey context = member.GetContext( cursor.TargetObj.Target );

            MemberResolutionResult result =
                TryProcessMember( value, member.DeclaredType, context, state, out SerializedData memberData, out IDescriptor childDesc, out ObjectStructure structure );

            LinkDataNode( cursor.DataNode, member.Name, memberData, memberIndex );

            if( result == MemberResolutionResult.RequiresPush )
            {
                PushCursor( value, cursor.TargetObj.Target, member, childDesc, structure, memberData, state );
                return SerializationCursorResult.Push;
            }

            return SerializationCursorResult.Advance;
        }

        private static SerializationCursorResult PhasePostProcessing( ref SerializationCursor cursor, SerializationState state )
        {
            if( cursor.Descriptor is ISerializationCallbackDescriptor callbackDesc )
            {
                callbackDesc.OnSerialized( cursor.TargetObj.Target, state.Context );
            }
            return SerializationCursorResult.Finished;
        }



        public static MemberResolutionResult TryProcessMember(
            object value,
            Type declaredType,
            ContextKey context,
            SerializationState state,
            out SerializedData data,
            out IDescriptor actualDescriptor,
            out ObjectStructure structure )
        {
            data = null;
            actualDescriptor = null;
            structure = ObjectStructure.Unwrapped;

            // 1. Null check.
            if( value == null )
            {
                return MemberResolutionResult.Resolved;
            }

            Type actualType = value.GetType();

            // 2. Type resolution.
            actualDescriptor = TypeDescriptorRegistry.GetDescriptor( actualType, context );

            // 3. Metadata flags.
            structure = actualDescriptor.DetermineObjectStructure( declaredType, actualType, state.Context.Config, out bool needsId, out bool needsType );

            // 4. Serialization & wrapping.
            if( actualDescriptor is IPrimitiveDescriptor primitiveDesc )
            {
                try
                {
                    primitiveDesc.SerializeDirect( value, ref data, state.Context );
                }
                catch( Exception ex )
                {
                    WrapException( ex, state, "Serializing Primitive", actualDescriptor, null );
                }

                SerializationHelpers.WriteMetadata( value, ref data, structure, state.Context, actualType, needsId, needsType );
                return MemberResolutionResult.Resolved;
            }

            if( actualDescriptor is ICompositeDescriptor )
            {
                // 5. Handle auto-references.
                if( !actualType.IsValueType )
                {
                    if( state.Context.Config.CycleHandling == CycleHandling.Throw )
                    {
                        if( state.Stack.ContainsTarget( value ) )
                        {
                            throw new UPSCircularReferenceException( state.Context, $"Circular reference detected for object of type {actualType.FullName}.", state.Stack.BuildPath(), null, null, "Processing Value", null );
                        }
                    }
                    else
                    {
                        if( state.VisitedObjects.Contains( value ) )
                        {
                            Guid id = state.Context.ReverseMap.GetID( value );
                            data = new SerializedObject()
                            {
                                { KeyNames.REF, id.SerializeGuid() }
                            };
                            return MemberResolutionResult.Resolved;
                        }
                    }
                }

                data = actualDescriptor is ICollectionDescriptor ? (SerializedData)new SerializedArray() : new SerializedObject();
                if( state.Context.Config.CycleHandling != CycleHandling.Throw )
                {
                    state.VisitedObjects.Add( value );
                }

                SerializationHelpers.WriteMetadata( value, ref data, structure, state.Context, actualType, needsId, needsType );
                return MemberResolutionResult.RequiresPush;
            }

            throw new UPSSerializationException( state.Context, $"Unsupported descriptor type: {actualDescriptor.GetType().FullName}", state.Stack.BuildPath(), actualDescriptor, null, "Processing Value", null );
        }

        private static void PushCursor( object target, object parent, IMemberInfo memberInfo, IDescriptor descriptor, ObjectStructure structure, SerializedData dataNode, SerializationState state )
        {
            SerializedData innerData = (structure == ObjectStructure.Wrapped && dataNode is SerializedObject obj && obj.TryGetValue( KeyNames.VALUE, out var inner )) ? inner : dataNode;

            var cursor = new SerializationCursor()
            {
                TargetObj = new TrackedObject( target, parent, memberInfo ),
                Descriptor = descriptor,
                DataNode = innerData,
                StepIndex = 0,
                Phase = SerializationCursorPhase.PreProcessing,
                WriteBackOnPop = false
            };
            state.Stack.Push( cursor );
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