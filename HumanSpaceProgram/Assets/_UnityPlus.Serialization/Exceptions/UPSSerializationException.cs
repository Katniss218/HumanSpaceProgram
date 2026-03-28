using System;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Signals that an exception occurred during serialization or deserialization.
    /// </summary>
    public class UPSSerializationException : Exception
    {
        /// <summary>
        /// The debug info for the serialization operation when the exception occurred.
        /// </summary>
        public IReadonlySerializationContext Context { get; }

        /// <summary>
        /// The path to the object or member where the error occurred.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The descriptor of the type being processed.
        /// </summary>
        public IDescriptor Descriptor { get; }

        /// <summary>
        /// The member being processed, if applicable.
        /// </summary>
        public IMemberInfo Member { get; }

        /// <summary>
        /// The operation being performed (e.g., "Get Value", "Set Value", "Construct").
        /// </summary>
        public string Operation { get; }

        /// <summary>
        /// Shorthand to get the log from the context, which may contain additional errors and issues.
        /// </summary>
        public SerializationLog Log => Context?.Log;

        public override string Message
        {
            get
            {
                string baseMsg = base.Message;
                string contextInfo = $"\n- Operation: {Operation} " +
                    $"\n- Descriptor: {Descriptor} " +
                    $"\n- Path: {Path} " +
                    ((Member == null) ? "" : $"\n- Member: {Member.Name} ");

                return baseMsg + contextInfo;
            }
        }

        public UPSSerializationException( IReadonlySerializationContext ctx, string message )
            : base( message )
        {
            Context = ctx;
        }

        public UPSSerializationException( IReadonlySerializationContext ctx, string message, string path, IDescriptor descriptor, IMemberInfo member, string operation, Exception innerException )
            : base( message, innerException )
        {
            Context = ctx;
            Path = path;
            Descriptor = descriptor;
            Member = member;
            Operation = operation;
        }



        protected UPSSerializationException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}