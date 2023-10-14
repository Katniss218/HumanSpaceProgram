using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// A class responsible for instantiating part hierarchy from a source (save file, picked in VAB, etc).
    /// </summary>
    public sealed class PartFactory
    {
        /// <summary>
        /// Inherit from this class to define a way of instantiating parts to the scene.
        /// </summary>
        public abstract class PartSource
        {
            public string PartID { get; set; }

            public PartSource( string partID )
            {
                this.PartID = partID;
            }

            /// <summary>
            /// Implement this to create a part in the scene, with the specified object as its parent, with the specified local (parent's space) position and rotation.
            /// </summary>
            /// <returns>The newly created part that exists in the scene.</returns>
            public abstract Transform Instantiate( Transform parent, Vector3 localPosition, Quaternion localRotation );
        }

        // Parts are a flat Unity hierarchy of objects that are the direct child of a vessel object.

        // Add source for the part's persistent data and modules.

        public PartSource Source { get; set; }

        public PartFactory( PartSource source )
        {
            this.Source = source;
        }

#warning TODO - modify it so that you first create a vessel/building and then add parts to it.
        public Transform CreateRoot( Vessel vessel )
        {
            if( vessel.RootPart != null )
            {
                throw new InvalidOperationException( $"The vessel '{vessel}' already has a root part." );
            }

            Transform part = Source.Instantiate( vessel.transform, Vector3.zero, Quaternion.identity );

            vessel.SetRootPart( part );
            vessel.RecalculateParts();

            return part;
        }
        public Transform CreateRoot( Building building )
        {
            if( building.RootPart != null )
            {
                throw new InvalidOperationException( $"The building '{building}' already has a root part." );
            }

            Transform part = Source.Instantiate( building.transform, Vector3.zero, Quaternion.identity );

            building.SetRootPart( part );

            return part;
        }

        public Transform Create( Transform parent, Vector3 localPosition, Quaternion localRotation )
        {
            if( parent == null )
            {
                throw new ArgumentNullException( nameof( parent ), $"Parent can't be null." );
            }

            Transform part = Source.Instantiate( parent, localPosition, localRotation );
            //part.SetVesselRecursive( parent.Vessel );

            //part.Parent = parent;
            //part.Parent.Children.Add( part );
            part.GetVessel().RecalculateParts();
            return part;
        }

        public static void Destroy( Transform part )
        {
            UnityEngine.Object.Destroy( part.gameObject );

            if( part.IsRootOfVessel() )
            {
                VesselFactory.Destroy( part.GetVessel() );
            }
            else
            {
                part.GetVessel().RecalculateParts();
            }
        }
    }
}