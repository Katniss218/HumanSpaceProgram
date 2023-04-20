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
            RootPart = part;
        }

        void Start()
        {
            Parts = this.GetPartsByHierarchy().ToArray();
        }

        void Update()
        {

        }
    }
}