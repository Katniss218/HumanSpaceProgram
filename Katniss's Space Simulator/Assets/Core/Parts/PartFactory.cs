using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core.Parts
{
    /// <summary>
    /// A class responsible for instantiating a part from a source (save file, picked in VAB, etc).
    /// </summary>
    public sealed class PartFactory
    {
        /// <summary>
        /// Inherit from this class to define a way of instantiating parts to the scene.
        /// </summary>
        public abstract class PartSource
        {
            /// <summary>
            /// Implement this to create a part in the scene, with the specified object as its parent, with the specified local (parent's space) position and rotation.
            /// </summary>
            /// <returns>The newly created part that exists in the scene.</returns>
            public abstract Part Instantiate( Transform parent, Vector3 localPosition, Quaternion localRotation );
        }

        // Parts are a flat Unity hierarchy of objects that are the direct child of a vessel object.

        // Add source for the part's persistent data and modules.

        public PartSource Source { get; set; }

        public PartFactory( PartSource source )
        {
            this.Source = source;
        }

        public Part CreateRoot( Vessel vessel )
        {
            if( vessel.RootPart != null )
            {
                throw new InvalidOperationException( $"The vessel '{vessel}' already has a root part." );
            }

            Part part = Source.Instantiate( vessel.transform, Vector3.zero, Quaternion.identity );
            part.SetVesselRecursive( vessel );

            vessel.SetRootPart( part );
            vessel.RecalculateParts();

            return part;
        }

        public Part Create( Part parent, Vector3 vesselPosition, Quaternion vesselRotation )
        {
            if( parent == null )
            {
                throw new ArgumentNullException( nameof( parent ), $"Parent can't be null." );
            }

            Part part = Source.Instantiate( parent.Vessel.transform, vesselPosition, vesselRotation );
            part.SetVesselRecursive( parent.Vessel );

            part.Parent = parent;
            part.Parent.Children.Add( part );
            part.Vessel.RecalculateParts();
            return part;
        }

        public static void Destroy( Part part, bool keepChildren = false )
        {
            UnityEngine.Object.Destroy( part.gameObject );
            foreach( var cp in part.Children )
            {
                Destroy( cp );
            }
            if( part.IsRootOfVessel )
            {
                VesselFactory.Destroy( part.Vessel );
            }
            else
            {
                part.Vessel.RecalculateParts();
            }
        }
    }
}