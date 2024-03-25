﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Inherit from this interface to specify that your component handles persisting its data by itself.
    /// </summary>
    /// <remarks>
    /// Invocation is not strictly enforced, and up to the serialization strategy.
    /// </remarks>
    public interface IPersistsData
    {
        /// <summary>
        /// Gets the persistent data from an object.
        /// </summary>
        /// <remarks>
        /// This should return the data required to reconstruct the full internal state of the object.
        /// </remarks>
        /// <param name="s">The reference map to use to resolve object references.</param>
        /// <returns>The serialized structure that contains the data. Identical to what is read by <see cref="SetData"/>.</returns>
        [return: NotNull]
        SerializedData GetData( [AllowNull] IReverseReferenceMap s );

        /// <summary>
        /// Sets the persistent data after creating the object with default parameters.
        /// </summary>
        /// <remarks>
        /// This should reconstruct the full internal state of the object from the given data. <br />
        /// *The data may be partial.*
        /// </remarks>
        /// <param name="l">The reference map to use to resolve object references.</param>
        /// <param name="data">The serialized structure that contains the data. Identical to what is created by <see cref="GetData"/>.</param>
        void SetData( [AllowNull] IForwardReferenceMap l, [DisallowNull] SerializedData data );
    }

    /// <summary>
    /// Inherit from this interface to specify that this component handles creation of objects during (de)serialization.
    /// </summary>
    /// <remarks>
    /// Invocation is not strictly enforced, and up to the serialization strategy.
    /// </remarks>
    public interface IPersistsObjects
    {
        SerializedData GetObjects( [AllowNull] IReverseReferenceMap s );
        void SetObjects( [AllowNull] IForwardReferenceMap l, [DisallowNull] SerializedData data );
    }
}