using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS
{
    /// <summary>
    /// Controls the parent object.
    /// </summary>
    public class TransformHandleParent : MonoBehaviour
    {
        [field: SerializeField]
        public Camera Camera { get; set; }

        // move parent by the sum of deltas of move handles.
        // don't rotate by rotation delta.

        // scale to keep arrows at constant size relative to the camera.
    }
}