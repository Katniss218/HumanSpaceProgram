using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Inherit from this interface to specify that this component handles creation of objects during (de)serialization.
    /// </summary>
    /// <remarks>
    /// Invocation is not strictly enforced, and up to the serialization strategy.
    /// </remarks>
    public interface IPersistsObjects
    {
        SerializedObject GetObjects( IReverseReferenceMap s );
        void SetObjects( SerializedObject data, IForwardReferenceMap l );
    }
}