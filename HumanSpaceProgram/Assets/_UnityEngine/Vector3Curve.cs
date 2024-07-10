using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public class Vector3Curve
    {
        private struct Entry
        {
            public float x, y, z;
            public float t;
        }

        private Entry[] _sortedEntries;
    }
}