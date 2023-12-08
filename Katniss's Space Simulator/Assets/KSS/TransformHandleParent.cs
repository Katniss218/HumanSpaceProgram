using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS
{
    /// <summary>
    /// Controls an entire set of transform handles.
    /// </summary>
    [DisallowMultipleComponent]
    public class TransformHandleParent : MonoBehaviour
    {
        [field: SerializeField]
        public Camera Camera { get; set; }

        private IEnumerable<TransformHandle> _handles = new List<TransformHandle>();

        // move parent by the sum of deltas of move handles.
        // don't rotate by rotation delta.

        // scale to keep arrows at constant size relative to the camera.

        // orientation of 'this.transform' dictates the orientation of the entire set of handles.
        
        void OnAfterTranslate( Vector3 worldSpaceDelta )
        {
            this.transform.position += worldSpaceDelta;
        }
    }
}