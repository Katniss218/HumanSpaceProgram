using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.PlayerLoop;

namespace HSP_Tests_PlayMode
{
    public sealed class HistoryStartPhase { }

    /// <summary>
    /// An optional interface your state structs/classes can implement to feed 
    /// simulation-specific times into diagnostic error messages without relying on reflection.
    /// </summary>
    public interface IWithSimulationTime
    {
        float GetSimulationTime();
    }

    public struct SystemSnapshot<T>
    {
        public float UnityTime;
        public float DeltaTime;
        public Type Phase;
        /// <summary>
        /// 
        /// </summary>
        public int SequenceIndex;
        /// <summary>
        /// 
        /// </summary>
        public int FrameIndex;
        /// <summary>
        /// 
        /// </summary>
        public int FixedUpdateInFrameIndex;
        /// <summary>
        /// The number of times this specific phase type was executed before.
        /// </summary>
        public int PhaseExecutionIndex;
        public T Data;

        public override string ToString()
        {
            return $"Snapshot #{SequenceIndex} [{Phase.Name}] Time: {UnityTime:F4}s (dt: {DeltaTime:F4}s) Frame: {FrameIndex} SubFUI: {FixedUpdateInFrameIndex} PhaseExecutions: {PhaseExecutionIndex} Data: {Data}";
        }
    }

    // Static hooks into the UnityPlus player loop systems remain simple wrappers.
    [PlayerLoopSystem( typeof( UnityPlus.PlayerLoop.Phases.FixedUpdate ) )]
    public class HistoryRecorderFixedUpdateSystem : IPlayerLoopSystem
    {
        public void Run() => HistoryRecorder.OnPhase( typeof( UnityPlus.PlayerLoop.Phases.FixedUpdate ) );
    }

    [PlayerLoopSystem( typeof( UnityPlus.PlayerLoop.Phases.Update ) )]
    public class HistoryRecorderUpdateSystem : IPlayerLoopSystem
    {
        public void Run() => HistoryRecorder.OnPhase( typeof( UnityPlus.PlayerLoop.Phases.Update ) );
    }

    [PlayerLoopSystem( typeof( UnityPlus.PlayerLoop.Phases.LateUpdate ) )]
    public class HistoryRecorderLateUpdateSystem : IPlayerLoopSystem
    {
        public void Run() => HistoryRecorder.OnPhase( typeof( UnityPlus.PlayerLoop.Phases.LateUpdate ) );
    }

    [PlayerLoopSystem( typeof( UnityPlus.PlayerLoop.Phases.FrameEnd ) )]
    public class HistoryRecorderFrameEndSystem : IPlayerLoopSystem
    {
        public void Run() => HistoryRecorder.OnPhase( typeof( UnityPlus.PlayerLoop.Phases.FrameEnd ) );
    }

    public class HistoryRecorder : IDisposable
    {
        // Using WeakReference to prevent active recorders from leaking if a test crashes 
        // without reaching its Dispose() block.
        private static readonly List<WeakReference<HistoryRecorder>> _activeRecorders = new();

        public static void OnPhase( Type phaseType )
        {
            for( int i = _activeRecorders.Count - 1; i >= 0; i-- )
            {
                if( _activeRecorders[i].TryGetTarget( out var recorder ) )
                {
                    recorder.ExecutePhase( phaseType );
                }
                else
                {
                    _activeRecorders.RemoveAt( i );
                }
            }
        }

        private readonly float _maxRecordableTime;
        private readonly float _initialTime;
        private bool _isDisposed;

        // Automatically tracks execution counts for any phase dynamically
        private readonly Dictionary<Type, int> _phaseExecutionCounters = new();
        private int _lastFrameIndex;
        private int _fixedUpdateInFrameCount;

        private struct RecordRegistry
        {
            public Type PhaseType;
            public Action RecordAction;
        }

        private readonly List<RecordRegistry> _recordRegistries = new();
        private readonly Dictionary<Type, object> _tracks = new();

        public HistoryRecorder( float maxRecordableTime = 30f )
        {
            _maxRecordableTime = maxRecordableTime;
            _initialTime = Time.time;
            _lastFrameIndex = Time.frameCount;
            _fixedUpdateInFrameCount = 0;
            _activeRecorders.Add( new WeakReference<HistoryRecorder>( this ) );
        }

        private List<SystemSnapshot<T>> GetOrCreateTrack<T>()
        {
            if( !_tracks.TryGetValue( typeof( T ), out var track ) )
            {
                track = new List<SystemSnapshot<T>>();
                _tracks[typeof( T )] = track;
            }
            return (List<SystemSnapshot<T>>)track;
        }

        public void RecordInstant<TState>( TState payload, Type manualPhaseType = null )
        {
            float time = Time.time;
            var track = GetOrCreateTrack<TState>();
            float dt = track.Count > 0 ? time - track[^1].UnityTime : 0f;
            var phase = manualPhaseType ?? typeof( object );

            track.Add( new SystemSnapshot<TState>()
            {
                UnityTime = time,
                DeltaTime = dt,
                Phase = phase,
                SequenceIndex = track.Count,
                FrameIndex = Time.frameCount,
                PhaseExecutionIndex = _phaseExecutionCounters.GetValueOrDefault( phase ),
                FixedUpdateInFrameIndex = _fixedUpdateInFrameCount,
                Data = payload
            } );
        }

        public void Record<TPhase, TState>( Func<TState> stateExtractor ) where TPhase : struct
        {
            List<SystemSnapshot<TState>> track = GetOrCreateTrack<TState>();
            Type phaseType = typeof( TPhase );

            if( track.Count == 0 )
            {
                track.Add( new SystemSnapshot<TState>()
                {
                    UnityTime = Time.time,
                    DeltaTime = 0f,
                    Phase = typeof( HistoryStartPhase ),
                    SequenceIndex = track.Count,
                    FrameIndex = Time.frameCount,
                    PhaseExecutionIndex = 0,
                    FixedUpdateInFrameIndex = _fixedUpdateInFrameCount,
                    Data = stateExtractor()
                } );
            }

            _recordRegistries.Add( new RecordRegistry()
            {
                PhaseType = phaseType,
                RecordAction = () =>
                {
                    float time = Time.time;
                    float dt = track.Count > 0 ? time - track[^1].UnityTime : 0f;

                    track.Add( new SystemSnapshot<TState>()
                    {
                        UnityTime = time,
                        DeltaTime = dt,
                        Phase = phaseType,
                        SequenceIndex = track.Count,
                        FrameIndex = Time.frameCount,
                        PhaseExecutionIndex = _phaseExecutionCounters.GetValueOrDefault( phaseType ),
                        FixedUpdateInFrameIndex = _fixedUpdateInFrameCount,
                        Data = stateExtractor()
                    } );
                }
            } );
        }

        private void ExecutePhase( Type phaseType )
        {
            if( phaseType == typeof( UnityPlus.PlayerLoop.Phases.FrameEnd ) )
            {
                if( Time.time - _initialTime > _maxRecordableTime )
                {
                    throw new AssertionException( $"HistoryRecorder test execution exceeded maximum allowed time of {_maxRecordableTime} seconds." );
                }
            }

            int currentFrame = Time.frameCount;
            if( currentFrame != _lastFrameIndex )
            {
                _lastFrameIndex = currentFrame;
                _fixedUpdateInFrameCount = 0;
            }

            // Increment execution count for this phase type dynamically
            _phaseExecutionCounters[phaseType] = _phaseExecutionCounters.GetValueOrDefault( phaseType ) + 1;

            if( phaseType == typeof( UnityPlus.PlayerLoop.Phases.FixedUpdate ) )
            {
                _fixedUpdateInFrameCount++;
            }

            // Loop index used for safety in case registries are altered
            for( int i = 0; i < _recordRegistries.Count; i++ )
            {
                var registry = _recordRegistries[i];
                if( registry.PhaseType == phaseType )
                {
                    try
                    {
                        registry.RecordAction();
                    }
                    catch( Exception ex )
                    {
                        Debug.LogException( ex );
                    }
                }
            }
        }

        public IReadOnlyList<SystemSnapshot<T>> GetHistory<T>()
        {
            return GetOrCreateTrack<T>();
        }

        public void Dispose()
        {
            if( !_isDisposed )
            {
                _activeRecorders.RemoveAll( r => !r.TryGetTarget( out var target ) || target == this );
                _isDisposed = true;
            }
        }
    }
}