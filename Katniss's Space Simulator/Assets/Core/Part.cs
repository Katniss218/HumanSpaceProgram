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

        [field: SerializeField]
        public Part Parent { get; private set; }

        [field: SerializeField]
        public List<Part> Children { get; private set; } = new List<Part>();

        [field: SerializeField]
        public List<PartModule> Modules { get; private set; } = new List<PartModule>();

        public void SetParent( Part parent )
        {
            if( this.Vessel.RootPart == this )
            {
                throw new System.InvalidOperationException( "Can't reparent the root object." );
            }

            if( parent.Vessel != this.Vessel )
            {
                // cross-vessel parenting.
                // Move part to other vessel.

                this.SetVessel( parent.Vessel );
            }

            if( this.Parent != null )
            {
                this.Parent.Children.Remove( this );
            }

            this.Parent = parent;

            if( this.Parent != null )
            {
                this.Parent.Children.Add( this );
            }
        }

        public void SetPosition( Vector3 pos, bool moveChildren = true )
        {
            this.transform.position = pos;

            if( moveChildren )
            {
                foreach( var cp in this.Children )
                {
                    cp.SetPosition( pos, moveChildren );
                }
            }
        }

        public void SetRotation( Quaternion rot, bool moveChildren = true )
        {
            this.transform.rotation = rot;

            if( moveChildren )
            {
                foreach( var cp in this.Children )
                {
                    cp.SetRotation( rot, moveChildren );
                }
            }
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