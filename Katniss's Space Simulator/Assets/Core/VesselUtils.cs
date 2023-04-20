using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatnisssSpaceSimulator.Core
{
    public static class VesselUtils
    {
        // Staging is technically only the act of splitting the vessel in two.

        // Docking is then two vessels combine into one.

        public static (Vessel origV, Vessel newV) DetachVesselPart( Part splitPart )
        {
            // Detaches the specified part from the vessel.
            // Does nothing if the part is the root part.
            // The part becomes the root part of the new vessel.
            if( splitPart.IsRootPart )
            {
                throw new InvalidOperationException( "Can't detach the root part off of a vessel." );
            }
            // part and its children are one vessel, the rest is the other.
            Vessel origV = splitPart.Vessel;

            Vessel newV = new VesselFactory().Create( splitPart );

            return (origV, newV);
        }

        public static Vessel AttachVesselPart( Vessel vesselAddon, Part vessel1ReferencePart )
        {
            // Attaches the vessel's root part to the reference part. Deletes the old vessel but keeps the parts as part of the reference part's vessel.
            // vesselAddon becomes a child of vessel1ReferencePart.

            vesselAddon.RootPart.SetParent( vessel1ReferencePart );
            vessel1ReferencePart.Vessel.SetRootPart( vesselAddon.RootPart );
            vesselAddon.RootPart.SetVesselHierarchy( vessel1ReferencePart.Vessel );
            VesselFactory.Destroy( vesselAddon );
            return vessel1ReferencePart.Vessel;
        }
    }
}
