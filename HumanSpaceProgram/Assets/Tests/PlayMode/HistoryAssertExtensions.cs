using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System;
using System.Linq;
using NUnit.Framework;

namespace HSP_Tests_PlayMode
{
    public static class HistoryAssertExtensions
    {
        public static HistoryTimelineAssert<T> AssertTimeline<T>( this HistoryRecorder recorder )
        {
            return new HistoryTimelineAssert<T>( recorder.GetHistory<T>() );
        }

        public static IEnumerable<SystemSnapshot<T>> InPhase<T, TPhase>( this IEnumerable<SystemSnapshot<T>> snapshots )
        {
            return snapshots.Where( s => s.Phase == typeof( TPhase ) );
        }

        public static SystemSnapshot<T> FirstInPhase<T, TPhase>( this IEnumerable<SystemSnapshot<T>> snapshots )
        {
            var match = snapshots.FirstOrDefault( s => s.Phase == typeof( TPhase ) );
            if( match.Phase == null )
            {
                throw new AssertionException( $"History query failed: No snapshot of phase type {typeof( TPhase ).Name} was found in the recorded track." );
            }
            return match;
        }

        public static SystemSnapshot<T> LastInPhase<T, TPhase>( this IEnumerable<SystemSnapshot<T>> snapshots )
        {
            var matches = snapshots.Where( s => s.Phase == typeof( TPhase ) ).ToList();
            if( matches.Count == 0 )
            {
                throw new AssertionException( $"History query failed: No snapshots of phase type {typeof( TPhase ).Name} were found." );
            }
            return matches.Last();
        }
    }

    public static class HistoryAssert
    {
        public static void AssertState<T, TValue>( this SystemSnapshot<T> snapshot, Func<T, TValue> selector, IResolveConstraint constraint, string valueName = "Value" )
        {
            TValue actualValue = selector( snapshot.Data );
            try
            {
                Assert.That( actualValue, constraint );
            }
            catch( AssertionException ex )
            {
                // Fallback simulation-time check via interface or reflection if requested.
                string simTimeInfo = string.Empty;
                if( snapshot.Data is IWithSimulationTime temporal )
                {
                    simTimeInfo = $", SimTime: {temporal.GetSimulationTime():F4}s";
                }
                else
                {
                    // Clean, non-throwing reflection fallback search for common timing fields.
                    var type = typeof( T );
                    var prop = type.GetProperty( "Time" ) ?? type.GetProperty( "UT" );
                    if( prop != null )
                    {
                        try { simTimeInfo = $", SimTime: {prop.GetValue( snapshot.Data )}"; } catch { }
                    }
                }

                throw new AssertionException(
                    $"History Assertion failed in {snapshot.Phase.Name} phase!\n" +
                    $"  [Snapshot #{snapshot.SequenceIndex}]\n" +
                    $"  Frame Stats:    FrameIndex = {snapshot.FrameIndex}, FixedUpdateInFrame = {snapshot.FixedUpdateInFrameIndex}\n" +
                    $"  Time Info:      UnityTime = {snapshot.UnityTime:F4}s (dt = {snapshot.DeltaTime:F4}s){simTimeInfo}\n" +
                    $"  Field/Metric:   {valueName}\n" +
                    $"  Details:        {ex.Message}\n"
                );
            }
        }
    }

    public class HistoryTimelineAssert<T>
    {
        private readonly IReadOnlyList<SystemSnapshot<T>> _sequence;
        private int _currentIndex = 0;

        public HistoryTimelineAssert( IReadOnlyList<SystemSnapshot<T>> sequence )
        {
            _sequence = sequence ?? throw new ArgumentNullException( nameof( sequence ) );
        }

        public HistoryTimelineAssert<T> StartingHere()
        {
            if( _sequence.Count == 0 )
            {
                throw new AssertionException( "Timeline assertion failed: No snapshots are recorded." );
            }
            _currentIndex = 0;
            return this;
        }

        public HistoryTimelineAssert<T> StartingHere<TValue>( Func<T, TValue> selector, IResolveConstraint constraint, string valueName = "Value" )
        {
            return StartingHere().Verify( selector, constraint, valueName );
        }

        public HistoryTimelineAssert<T> StartingHere( Action<T> assertionAction )
        {
            return StartingHere().Verify( assertionAction );
        }

        public HistoryTimelineAssert<T> Verify<TValue>( Func<T, TValue> selector, IResolveConstraint constraint, string valueName = "Value" )
        {
            if( _currentIndex < 0 || _currentIndex >= _sequence.Count )
            {
                throw new InvalidOperationException( $"Invalid timeline state: Cursor index {_currentIndex} is out of bounds." );
            }
            _sequence[_currentIndex].AssertState( selector, constraint, valueName );
            return this;
        }

        public HistoryTimelineAssert<T> Verify( Action<T> assertionAction )
        {
            if( _currentIndex < 0 || _currentIndex >= _sequence.Count )
            {
                throw new InvalidOperationException( $"Invalid timeline state: Cursor index {_currentIndex} is out of bounds." );
            }
            try
            {
                assertionAction?.Invoke( _sequence[_currentIndex].Data );
            }
            catch( Exception ex )
            {
                throw new AssertionException(
                    $"Timeline assertion failure in phase {_sequence[_currentIndex].Phase.Name} at Snapshot #{_sequence[_currentIndex].SequenceIndex}!\n" +
                    $"Details: {ex.Message}"
                );
            }
            return this;
        }

        // Direct, highly readable shortcuts using the UnityPlus phase mappings.
        public HistoryTimelineAssert<T> NextUpdate() => NextPhase<UnityPlus.PlayerLoop.Phases.Update>();
        public HistoryTimelineAssert<T> NextFixedUpdate() => NextPhase<UnityPlus.PlayerLoop.Phases.FixedUpdate>();
        public HistoryTimelineAssert<T> NextLateUpdate() => NextPhase<UnityPlus.PlayerLoop.Phases.LateUpdate>();
        public HistoryTimelineAssert<T> NextFrameEnd() => NextPhase<UnityPlus.PlayerLoop.Phases.FrameEnd>();

        /// <summary>
        /// Moves the cursor to the next sequential snapshot regardless of phase type.
        /// </summary>
        public HistoryTimelineAssert<T> Next()
        {
            if( _currentIndex + 1 >= _sequence.Count )
            {
                ThrowWithHistoryContext( "Already at the end of recorded history track." );
            }
            _currentIndex++;
            return this;
        }

        /// <summary>
        /// Moves the cursor forward to the next snapshot matching the specified phase type.
        /// </summary>
        public HistoryTimelineAssert<T> NextPhase<TPhase>()
        {
            Type phaseType = typeof( TPhase );
            for( int i = _currentIndex + 1; i < _sequence.Count; i++ )
            {
                if( _sequence[i].Phase == phaseType )
                {
                    _currentIndex = i;
                    return this;
                }
            }

            ThrowWithHistoryContext( $"Could not find any subsequent phase of type {phaseType.Name} after current cursor index {_currentIndex}." );
            return this;
        }

        private void ThrowWithHistoryContext( string message )
        {
            var historySummary = string.Join( "\n", _sequence.Select( ( s, idx ) =>
                $"{(idx == _currentIndex ? "-> " : "   ")} - {s}" ) );

            throw new AssertionException(
                $"Timeline Assertion failed: {message}\n" +
                $"Current Timeline Snapshot history:\n{historySummary}"
            );
        }
    }
}