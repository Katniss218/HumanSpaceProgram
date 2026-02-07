using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus
{
    /// <summary>
    /// A thread-safe dispatcher that allows background threads to schedule work on the Unity Main Thread.
    /// <para>
    /// Critical for the Asset System to avoid deadlocks when mixing Synchronous and Asynchronous code.
    /// </para>
    /// </summary>
    public static class MainThreadDispatcher
    {
        private static readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();
        private static int _mainThreadId;

        /// <summary>
        /// Initializes the dispatcher. Automatically called on subsystem registration or first use.
        /// </summary>
        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.SubsystemRegistration )]
        private static void Initialize()
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;

            // Ensure we have a ticker to pump the queue automatically during normal gameplay
            // We use the PlayerLoop or a hidden GameObject. For simplicity and robustness:
            if( Application.isPlaying )
            {
                var host = new GameObject( "HSP_MainThreadDispatcher" );
                UnityEngine.Object.DontDestroyOnLoad( host );
                host.AddComponent<DispatcherTicker>();
                host.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        /// <summary>
        /// Returns true if the current thread is the Unity Main Thread.
        /// </summary>
        public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == _mainThreadId;

        /// <summary>
        /// Processes all currently queued actions. 
        /// <para>
        /// Call this inside a blocking wait loop to prevent deadlocks.
        /// </para>
        /// </summary>
        public static void Pump()
        {
            // Limit processing per frame/call to avoid infinite loops if tasks queue more tasks endlessly?
            // For now, process everything currently available.
            int count = _executionQueue.Count;
            while( count > 0 && _executionQueue.TryDequeue( out Action action ) )
            {
                try
                {
                    action();
                }
                catch( Exception ex )
                {
                    Debug.LogException( ex );
                }
                count--;
            }
        }

        /// <summary>
        /// Enqueues an action to run on the main thread.
        /// </summary>
        public static void Enqueue( Action action )
        {
            _executionQueue.Enqueue( action );
        }

        /// <summary>
        /// Enqueues a function to run on the main thread and returns a Task representing the result.
        /// </summary>
        public static Task<T> RunAsync<T>( Func<T> work )
        {
            if( IsMainThread )
            {
                // Optimization: If we are already on the main thread, just run it.
                // NOTE: In the context of a blocking Pump loop, this is safe. 
                // In other contexts, be aware this runs synchronously.
                try
                {
                    return Task.FromResult( work() );
                }
                catch( Exception ex )
                {
                    return Task.FromException<T>( ex );
                }
            }

            var tcs = new TaskCompletionSource<T>();
            _executionQueue.Enqueue( () =>
            {
                try
                {
                    var result = work();
                    tcs.TrySetResult( result );
                }
                catch( Exception ex )
                {
                    tcs.TrySetException( ex );
                }
            } );
            return tcs.Task;
        }

        /// <summary>
        /// Enqueues an action to run on the main thread and returns a Task.
        /// </summary>
        public static Task RunAsync( Action work )
        {
            if( IsMainThread )
            {
                try
                {
                    work();
                    return Task.CompletedTask;
                }
                catch( Exception ex )
                {
                    return Task.FromException( ex );
                }
            }

            var tcs = new TaskCompletionSource<bool>();
            _executionQueue.Enqueue( () =>
            {
                try
                {
                    work();
                    tcs.TrySetResult( true );
                }
                catch( Exception ex )
                {
                    tcs.TrySetException( ex );
                }
            } );
            return tcs.Task;
        }

        // Internal Ticker to keep the queue moving during normal frames
        private class DispatcherTicker : MonoBehaviour
        {
            private void Update()
            {
                MainThreadDispatcher.Pump();
            }
        }
    }
}