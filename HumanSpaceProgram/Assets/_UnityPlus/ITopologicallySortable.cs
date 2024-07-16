using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus
{
    public interface ITopologicallySortable<T>
    {
        T ID { get; }

        /// <summary>
        /// The listener will run BEFORE the specified listeners (unless they're blocked).
        /// </summary>
        T[] Before { get; }

        /// <summary>
        /// The listener will run AFTER the specified listeners (unless they're blocked).
        /// </summary>
        T[] After { get; }
    }
}