using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    public class Part : MonoBehaviour
    {
        private Vessel _vessel;
        public Vessel Vessel
        {
            get
            {
                if( _vessel == null )
                {
                    _vessel = this.GetVessel();
                }
                return _vessel;
            }
            private set
            {
                _vessel = value;
            }
        }

        public string DisplayName { get; set; }

        public Part Parent { get; private set; }
        public List<Part> Children { get; private set; } = new List<Part>();
        public List<PartModule> Modules { get; private set; } = new List<PartModule>();

        public void RecalculateCachedHierarchy()
        {
            this.Parent = this.GetParent(); // Should throw when the part is contained in an invalid hierarchy.
            if( Parent != null ) // this is not root.
            {
                this.Parent.Children.Add( this );
            }
        }

        void Awake()
        {
            // When part is created, add it to the cached hierarchy.
            RecalculateCachedHierarchy();
        }

        void Start()
        {
            foreach( var module in Modules )
            {
                module.Start();
            }
        }

        void Update()
        {
            foreach( var module in Modules )
            {
                module.Update();
            }
        }

        void FixedUpdate()
        {
            foreach( var module in Modules )
            {
                module.FixedUpdate();
            }
        }
    }
}