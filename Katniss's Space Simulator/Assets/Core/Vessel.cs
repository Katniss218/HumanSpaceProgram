using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    public class Vessel : MonoBehaviour
    {
        public string DisplayName { get; set; }

        private Part _rootPart;
        public Part RootPart
        {
            get
            {
                if(_rootPart == null )
                {
                    _rootPart = this.GetRootPart();
                }
                return _rootPart;
            }
        }

        [field: SerializeField]
        public Part[] Parts { get; private set; }

        void Start()
        {
            Parts = this.RootPart.GetDescendants().ToArray();
        }

        void Update()
        {

        }
    }
}