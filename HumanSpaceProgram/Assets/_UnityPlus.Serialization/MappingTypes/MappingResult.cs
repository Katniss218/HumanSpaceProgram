using System;

namespace UnityPlus.Serialization
{
    [Flags]
    public enum SerializationResult : byte
    {
        NoChange = 0,
        /// <summary>
        /// This flag specifies that the de/serialization of this object has finished and shouldn't be retried. <br/>
        /// </summary>
        Finished = 1,
        /// <summary>
        /// This flag specifies that the de/serialization of this object ultimately failed.
        /// </summary>
        /// <remarks>
        /// Whether to retry depends on the <see cref="Finished"/> flag instead. <br/>
        /// This should also be set if any of the members/elements contained within the object have failed.
        /// </remarks>
        //Failed = Finished | 2 | HasFailures,
        Failed = 2,
        /// <summary>
        /// This flag specifies that the invocation has changed something about the resulting object/data.
        /// </summary>
        //Progressed = 4,
        // 8
        HasFailures = 16, // needs to be preserved and propagated to the parent.
        //HasSuccesses = 32,
        // 64
        Paused = 128,

        // =  -  =  -  =  -  =  -  =  -  =  -  =

        /// <summary>
        /// Use this to return a new finished status without a failure.
        /// </summary>
        PrimitiveFinished = Finished,
        /// <summary>
        /// Use this to return a new finished status with a failure.
        /// </summary>
        PrimitiveFinishedFailed = Finished | Failed,
        /// <summary>
        /// Use this to return a new status with a failure.
        /// </summary>
        PrimitiveRetryFailed = Failed,
    }
}