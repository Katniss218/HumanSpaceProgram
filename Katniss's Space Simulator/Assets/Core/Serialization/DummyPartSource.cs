using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core.Serialization
{
    public class DummyPartSource : PartFactory.PartSource
    {
        const string name = "tempname_part";

        public override Part Instantiate( Transform parent, Vector3 localPosition, Quaternion localRotation )
        {
            GameObject partGO = new GameObject( $"Part, '{name}'" );
            partGO.transform.SetParent( parent );

            Part part = partGO.AddComponent<Part>();
            part.DisplayName = name;
            part.SetLocalPosition( localPosition, false );
            part.SetLocalRotation( localRotation, false );

            GameObject gfx = new GameObject( "model" );
            gfx.transform.SetParent( partGO.transform );
            gfx.transform.localPosition = Vector3.zero;
            gfx.transform.localRotation = Quaternion.identity;
            gfx.transform.localScale = new Vector3( 1f, 1f, 1f );

            MeshFilter mf = gfx.AddComponent<MeshFilter>();
            mf.sharedMesh = UnityEngine.Object.FindObjectOfType<zzzTestGameManager>().Mesh;

            MeshRenderer mr = gfx.AddComponent<MeshRenderer>();
            mr.sharedMaterial = UnityEngine.Object.FindObjectOfType<zzzTestGameManager>().Material;

            BoxCollider c = gfx.AddComponent<BoxCollider>();
            c.size = Vector3.one;

            GameObject downTest = new GameObject( "thrust" );
            downTest.transform.SetParent( partGO.transform );
            downTest.transform.localPosition = Vector3.zero;
            downTest.transform.localRotation = Quaternion.Euler( -90, 0, 0 );
            downTest.transform.localScale = Vector3.one;
            part.Mass = 1;

            // Add modules, etc.

            return part;
        }
    }
}
