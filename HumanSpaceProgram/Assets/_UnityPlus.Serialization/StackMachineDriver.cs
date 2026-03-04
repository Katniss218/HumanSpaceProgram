using System;
using System.Diagnostics;
using UnityEditor.VersionControl;

namespace UnityPlus.Serialization
{
    public class StackMachineDriver
    {
        private SerializationState _state;
        private IOperationStrategy _strategy;
        private readonly Stopwatch _timer = new Stopwatch();

        public bool IsFinished => _state != null && _state.Stack.Count == 0 && _state.Context.DeferredOperations.Count == 0;

        /// <summary>
        /// The final result of the operation.
        /// </summary>
        public object Result => _state?.RootResult;

        public StackMachineDriver( SerializationContext context )
        {
            _state = new SerializationState( context );
        }

        /// <summary>
        /// Initializes the driver with a specific strategy.
        /// </summary>
        public void Initialize( object root, IDescriptor rootDescriptor, IOperationStrategy strategy, SerializedData rootData = null )
        {
            if( strategy == null ) throw new ArgumentNullException( nameof( strategy ) );

            _state.Stack.Clear();
            _state.Context.DeferredOperations.Clear();
            _state.VisitedObjects.Clear();
            _state.RootResult = null;

            _strategy = strategy;

            try
            {
                _strategy.InitializeRoot( root, rootDescriptor, rootData, _state );
            }
            catch( Exception ex )
            {
                WrapFatalException( ex, "Initialization" );
            }
        }

        public void Tick( float timeBudgetMs )
        {
            _timer.Restart();

            // PASS 1: Main Stack Processing
            while( _state.Stack.Count > 0 )
            {
                ProcessMainStack( timeBudgetMs );
            }

            // PASS 2: Deferred Resolution
            if( _state.Stack.Count == 0 && _state.Context.DeferredOperations.Count > 0 )
            {
                ProcessDeferredQueue( timeBudgetMs );
            }
        }

        private void ProcessMainStack( float timeBudgetMs )
        {
            if( _timer.ElapsedMilliseconds > timeBudgetMs )
                return;

            try
            {
                // Main step using strategy. The strategy processes the current cursor and returns a result indicating how to update the stack.
                SerializationCursor currentCursor = _state.Stack.Peek();
                SerializationCursorResult result = _strategy.Process( ref currentCursor, _state );

                // Apply updates to the stack state
                if( result == SerializationCursorResult.Advance )
                {
                    _state.Stack.Pop();
                    currentCursor.StepIndex++; // Centralized Increment
                    _state.Stack.Push( currentCursor );
                }
                else if( result == SerializationCursorResult.Jump )
                {
                    _state.Stack.Pop();
                    _state.Stack.Push( currentCursor ); // Push new state (Phase change or manual index set)
                }
                else if( result == SerializationCursorResult.Finished )
                {
                    SerializationCursor finishedCursor = _state.Stack.PopAndWriteback();
                    _strategy.OnCursorFinished( finishedCursor, _state );
                }
                else if( result == SerializationCursorResult.Push )
                {
                    SerializationCursor newChild = _state.Stack.Pop(); // The dependency pushed by the strategy
                    _state.Stack.Pop(); // The old parent state

                    currentCursor.StepIndex++; // Advance parent state so it resumes at Next when child returns

                    _state.Stack.Push( currentCursor ); // Push updated parent back
                    _state.Stack.Push( newChild ); // Push dependency back on top
                }
                else if( result == SerializationCursorResult.Deferred )
                {
                    _state.Stack.Pop(); // Discard from stack

                    // Cascade Deferral Logic
                    while( _state.Stack.Count > 0 )
                    {
                        SerializationCursor parent = _state.Stack.Peek();
                        if( parent.Phase == SerializationCursorPhase.Construction )
                        {
                            // Parent is in construction -> Cannot proceed without child -> Defer Parent
                            _state.Context.DeferredOperations.Enqueue( new DeferredOperation()
                            {
                                Target = parent.TargetObj.Parent, // The grandparent
                                Member = parent.TargetObj.Member,
                                Data = parent.DataNode,
                                Descriptor = parent.Descriptor,
                                ConstructionBuffer = parent.ConstructionBuffer,
                                ConstructionIndex = parent.StepIndex
                            } );
                            _state.Stack.Pop();
                        }
                        else
                        {
                            // Parent is Populating -> Can handle missing child by skipping member -> Advance Step
                            SerializationCursor p = _state.Stack.Pop();
                            p.StepIndex++;
                            _state.Stack.Push( p );
                            break;
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                WrapFatalException( ex, "Stack Processing" );
            }
        }

        private void ProcessDeferredQueue( float timeBudgetMs )
        {
            int count = _state.Context.DeferredOperations.Count;
            if( count == 0 )
                return;

            bool progressMade = false;
            int processedCount = 0;

            for( int i = 0; i < count; i++ )
            {
                if( _timer.ElapsedMilliseconds > timeBudgetMs )
                    return;

                DeferredOperation op = _state.Context.DeferredOperations.Dequeue();
                processedCount++;

                bool opSuccess = false;

                try
                {
                    // 1. Resume Interrupted Construction
                    if( op.ConstructionBuffer != null )
                    {
                        var cursor = new SerializationCursor()
                        {
                            TargetObj = new TrackedObject( null, op.Target, op.Member ),
                            Descriptor = op.Descriptor,
                            DataNode = op.Data,
                            ConstructionBuffer = op.ConstructionBuffer,
                            StepIndex = op.ConstructionIndex,
                            Phase = SerializationCursorPhase.Construction,
                        };

                        ICompositeDescriptor comp = (ICompositeDescriptor)op.Descriptor;
                        cursor.ConstructionStepCount = comp.GetConstructionStepCount( op.ConstructionBuffer );
                        cursor.PopulationStepCount = comp.GetStepCount( op.ConstructionBuffer ) - cursor.ConstructionStepCount;

                        _state.Stack.Push( cursor );
                        opSuccess = true;
                    }
                    // 2. Root Deferral
                    else if( op.Member == null )
                    {
                        var cursor = new SerializationCursor()
                        {
                            TargetObj = new TrackedObject( null ),
                            Descriptor = op.Descriptor,
                            DataNode = op.Data,
                            Phase = SerializationCursorPhase.PreProcessing,
                            StepIndex = 0
                        };
                        _state.Stack.Push( cursor );
                        opSuccess = true;
                    }
                    // 3. Member Deferral
                    else
                    {
                        IDescriptor desc = op.Member.TypeDescriptor;

                        if( desc is IPrimitiveDescriptor prim )
                        {
                            DeserializationResult res = prim.DeserializeDirect( op.Data, _state.Context, out object value );
                            if( res == DeserializationResult.Success )
                            {
                                object t = op.Target;
                                op.Member.SetValue( ref t, value );
                                opSuccess = true;
                            }
                            else
                            {
                                _state.Context.DeferredOperations.Enqueue( op );
                            }
                        }
                        else
                        {
                            var cursor = new SerializationCursor()
                            {
                                TargetObj = new TrackedObject( null, op.Target, op.Member ),
                                Descriptor = desc,
                                DataNode = op.Data,
                                Phase = SerializationCursorPhase.PreProcessing,
                                StepIndex = 0
                            };
                            _state.Stack.Push( cursor );
                            opSuccess = true;
                        }
                    }
                }
                catch( Exception ex )
                {
                    // Exceptions during deferred processing are tricky.
                    // We can't easily "path" them because the stack is empty (we just pushed).
                    // But we know the Operation context.
                    string message = op.Member != null ? $"Deferred Member {op.Member.Name}" : "Deferred Root";
                    WrapFatalException( ex, message );
                }

                if( opSuccess )
                    progressMade = true;
            }

            if( !progressMade && processedCount == count && _state.Stack.Count == 0 && _state.Context.DeferredOperations.Count > 0 )
            {
                string message = $"Circular Dependency Deadlock: {_state.Context.DeferredOperations.Count} items could not be resolved.";
                _state.Context.Log.Log( LogLevel.Fatal, message, _state );
                throw new UPSUnresolvableObjectException( _state.Context, message );
            }
        }

        private void WrapFatalException( Exception ex, string stage )
        {
            // If it's already one of our wrapped exceptions, just rethrow to avoid double-wrapping
            if( ex is UPSSerializationException )
                throw ex;

            string message = $"Serialization Fatal Error during {stage} at '{_state.Stack.BuildPath()}': {ex.Message}";
            _state.Context.Log.Log( LogLevel.Fatal, message );

            throw new UPSSerializationException( _state.Context, message, ex );
        }
    }
}