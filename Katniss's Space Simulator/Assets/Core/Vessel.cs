using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    public class Vessel : MonoBehaviour
    {
        public string DisplayName { get; set; }

        [field: SerializeField]
        public Part RootPart { get; private set; }

        [field: SerializeField]
        public Part[] Parts { get; private set; }

        public void SetRootPart( Part part )
        {
            if( part.Parent != null ) // change this because it's ugly.
            {
                throw new System.InvalidOperationException();
            }
            RootPart = part;
        }

        void Start()
        {
            Parts = this.GetParts().ToArray();
        }

        void Update()
        {

        }
    }
}