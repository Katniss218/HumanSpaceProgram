using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus
{
    public interface IOverridable<T>
    {
        T ID { get; }

        T[] Blacklist { get; }
    }
}