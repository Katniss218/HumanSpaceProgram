using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    /// <summary>
    /// A class responsible for instantiating a part from a source (save file, picked in VAB, etc).
    /// </summary>
    public class PartFactory
    {
        // Parts are a flat Unity hierarchy of objects that are the direct child of a vessel object.

        // Add source for the part's persistent data and modules.

        const string name = "tempname_part";

        public Part CreateRoot( Vessel vessel )
        {
            if( vessel.RootPart != null )
            {
                throw new InvalidOperationException( $"The vessel '{vessel}' already has a root part." );
            }

            Part part = Create( vessel.transform, Vector3.zero, Quaternion.identity );
            part.SetVesselRecursive( vessel );

            vessel.SetRootPart( part );
            vessel.RecalculateParts();

            return part;
        }

        public Part Create( Part parent, Vector3 localPosition, Quaternion localRotation )
        {
            if( parent == null )
            {
                throw new ArgumentNullException( nameof( parent ), $"Parent can't be null." );
            }

            Part part = Create( parent.Vessel.transform, localPosition, localRotation );
            part.SetVesselRecursive( parent.Vessel );

            part.Parent = parent;
            part.Parent.Children.Add( part );
            part.Vessel.RecalculateParts();
            return part;
        }

        private Part Create( Transform parent, Vector3 localPosition, Quaternion localRotation )
        {
            GameObject partGO = new GameObject( $"Part, '{name}'" );
            partGO.transform.SetParent( parent );
            partGO.transform.localPosition = localPosition;
            partGO.transform.localRotation = localRotation;

            Part part = partGO.AddComponent<Part>();
            part.DisplayName = name;

            GameObject gfx = new GameObject( "model" );
            gfx.transform.SetParent( partGO.transform );
            gfx.transform.localPosition = Vector3.zero;
            gfx.transform.localRotation = Quaternion.identity;
            gfx.transform.localScale = new Vector3( 0.125f, 0.125f, 0.125f );

            MeshFilter mf = gfx.AddComponent<MeshFilter>();
            mf.sharedMesh = UnityEngine.Object.FindObjectOfType<zzzTestGameManager>().Mesh;

            MeshRenderer mr = gfx.AddComponent<MeshRenderer>();
            mr.sharedMaterial = UnityEngine.Object.FindObjectOfType<zzzTestGameManager>().Material;

            BoxCollider c = gfx.AddComponent<BoxCollider>();
            c.size = Vector3.one * 5;

            GameObject downTest = new GameObject( "thrust" );
            downTest.transform.SetParent( partGO.transform );
            downTest.transform.localPosition = Vector3.zero;
            downTest.transform.localRotation = Quaternion.identity;
            downTest.transform.localScale = Vector3.one;
            downTest.transform.forward = Vector3.up;
            part.Mass = 1;

            // Add modules, etc.

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
