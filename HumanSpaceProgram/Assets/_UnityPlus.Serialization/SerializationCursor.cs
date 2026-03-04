using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Represents the state of a single object currently being processed on the stack.
    /// </summary>
    public struct SerializationCursor
    {
        /// <summary>
        /// The current phase of this object's deserialization.
        /// </summary>
        public SerializationCursorPhase Phase { get; set; }

        /// <summary>
        /// Stores the ID of the object if it was read from the wrapper before unwrapping collections.
        /// </summary>
        public Guid? PendingID;
        
        /// <summary>
        /// Stores the ID of the object if it was read from the wrapper before unwrapping collections.
        /// </summary>
        public Type PendingActualType;

        /// <summary>
        /// The object, parent, and access info encapsulated in a single struct.
        /// </summary>
        public TrackedObject TargetObj { get; set; }

        /// <summary>
        /// Storage for constructor arguments during the Construction Phase.
        /// </summary>
        public object[] ConstructionBuffer { get; set; }

        /// <summary>
        /// The descriptor that defines how to process this Target.
        /// </summary>
        public IDescriptor Descriptor;

        /// <summary>
        /// The current step (arg index or member index) we are processing within the current Phase.
        /// </summary>
        public int StepIndex { get; set; }

        /// <summary>
        /// Used for O(N) iteration of collections during Serialization phase.
        /// If null, StepIndex is used to access members via GetMemberInfo(index).
        /// </summary>
        public IEnumerator<IMemberInfo> MemberEnumerator { get; set; }

        /// <summary>
        /// The number of steps in the Construction Phase.
        /// </summary>
        public int ConstructionStepCount { get; set; }

        /// <summary>
        /// The number of steps in the Population Phase.
        /// </summary>
        public int PopulationStepCount { get; set; }

        /// <summary>
        /// The SerializedData node associated with this object.
        /// </summary>
        public SerializedData DataNode { get; set; }

        /// <summary>
        /// If true, the Target will be written back to the Parent via the Member accessor when this cursor is popped.
        /// Required for Deserialization (Assigning results) and Value Types (Propagating mutations).
        /// </summary>
        public bool WriteBackOnPop { get; set; }
    }
}