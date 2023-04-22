using KatnisssSpaceSimulator.Core.Managers;
using KatnisssSpaceSimulator.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    public class CelestialBodyFactory
    {
        //const float radius = 1000; //6371000f; // m
        public const float radius = 6371000f;
        //const float mass = 20e16f; //5.97e24f; // kg  // 20e16f for 1km radius is good
        public const float mass = 5.97e24f;
        public const int subdivs = 7; // 7 is the maximum value for a single plane that won't cause issues here.

        public CelestialBody Create( Vector3Large AIRFPosition )
        {
            GameObject cbGO = new GameObject( "celestialbody" );
            cbGO.transform.position = ReferenceFrameManager.WorldSpaceReferenceFrame.InverseTransformPosition( AIRFPosition );
            cbGO.transform.localScale = Vector3.one;

            Vector3[] offsets = new Vector3[6]
            {
                new Vector3( radius, 0, 0 ),
                new Vector3( -radius, 0, 0 ),
                new Vector3( 0, radius, 0 ),
                new Vector3( 0, -radius, 0 ),
                new Vector3( 0, 0, radius ),
                new Vector3( 0, 0, -radius ),
            };

            for( int i = 0; i < 6; i++ )
            {
                Transform t = CreateMeshPart( cbGO.transform );
                t.localPosition = offsets[i];

                Mesh mesh = CubeSphereTerrain.GeneratePartialCubeSphere( subdivs, radius, (CubeSphereTerrain.Face)i );
                t.GetComponent<MeshCollider>().sharedMesh = mesh;
                t.GetComponent<MeshFilter>().sharedMesh = mesh;
            }

            //SphereCollider c = cbGO.AddComponent<SphereCollider>();

            CelestialBody cb = cbGO.AddComponent<CelestialBody>();
            cb.AIRFPosition = AIRFPosition;
            cb.Mass = mass;
            cb.Radius = radius;

            CelestialBodyManager.Bodies.Add( cb );
            return cb;
        }

        Transform CreateMeshPart( Transform cbTransform )
        {
            GameObject go = new GameObject( "mesh" );
            go.transform.SetParent( cbTransform );
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            MeshFilter cbMesh = go.AddComponent<MeshFilter>();

            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = UnityEngine.Object.FindObjectOfType<zzzTestGameManager>().CBMaterial;

            MeshCollider c = go.AddComponent<MeshCollider>();

            return go.transform;
        }
    }
}
