using HSP.ReferenceFrames;
using HSP.SceneManagement;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Vessels
{
    /// <summary>
    /// Helper class responsible for changing the state of a part or vessel.
    /// </summary>
    public static class VesselHierarchyUtils
    {
        private static Vector3 GetLocalPosRelative( Transform descendant, Transform ancestor )
        {
            if( descendant == ancestor ) return Vector3.zero;

            Vector3 localPos = Vector3.zero;
            Transform current = descendant;

            while( current != null && current != ancestor )
            {
                localPos = current.localPosition + current.localRotation * Vector3.Scale( current.localScale, localPos );
                current = current.parent;
            }

            return localPos;
        }

        private static Quaternion GetLocalRotRelative( Transform descendant, Transform ancestor )
        {
            if( descendant == ancestor ) return Quaternion.identity;

            Quaternion localRot = Quaternion.identity;
            Transform current = descendant;

            while( current != null && current != ancestor )
            {
                localRot = current.localRotation * localRot;
                current = current.parent;
            }

            return localRot;
        }

        private static (Vector3 localCoM, float totalMass) GetSubtreeLocalMassProperties( Vessel oldVessel, IEnumerable<VesselPart> parts )
        {
            float totalMass = 0;
            Vector3 localCoMAccumulator = Vector3.zero;

            foreach( var part in parts )
            {
                foreach( var massivePart in part.GetComponentsInChildren<IHasMass>() )
                {
                    if( massivePart is Component comp )
                    {
                        float m = massivePart.Mass;
                        totalMass += m;
                        Vector3 partLocalPos = GetLocalPosRelative( comp.transform, oldVessel.transform );
                        localCoMAccumulator += partLocalPos * m;
                    }
                }
            }

            if( totalMass <= 0 )
            {
                Vector3 avgLocalPosAccumulator = Vector3.zero;
                int count = 0;
                foreach( var part in parts )
                {
                    avgLocalPosAccumulator += GetLocalPosRelative( part.transform, oldVessel.transform );
                    count++;
                }
                Vector3 avgLocalPos = count > 0 ? avgLocalPosAccumulator / count : Vector3.zero;
                return (avgLocalPos, 0);
            }

            return (localCoMAccumulator / totalMass, totalMass);
        }

        public static void Attach( FAttachNode nodeA, FAttachNode nodeB )
        {
            if( nodeA == null || nodeB == null )
            {
                throw new ArgumentNullException( "Nodes to attach cannot be null." );
            }

            Vessel vesselA = nodeA.Part.GetVessel();
            Vessel vesselB = nodeB.Part.GetVessel();

            if( vesselA == vesselB )
            {
                vesselA.Graph.AddLink( nodeA, nodeB );
                vesselA.RebuildIslands();
            }
            else
            {
                // Merge vessels (B into A)
                Vessel mergedVessel = vesselB;
                Vessel remainingVessel = vesselA;

                Vector3Dbl posA = remainingVessel.ReferenceFrameTransform.GetAbsolutePosition();
                QuaternionDbl rotA = remainingVessel.ReferenceFrameTransform.GetAbsoluteRotation();

                Vector3Dbl posB = mergedVessel.ReferenceFrameTransform.GetAbsolutePosition();
                QuaternionDbl rotB = mergedVessel.ReferenceFrameTransform.GetAbsoluteRotation();

                foreach( var part in mergedVessel.Graph.GetAllParts() )
                {
                    Vector3 localPosB = GetLocalPosRelative( part.transform, mergedVessel.transform );
                    Quaternion localRotB = GetLocalRotRelative( part.transform, mergedVessel.transform );

                    Vector3Dbl absPartPos = posB + rotB * (Vector3Dbl)localPosB;
                    QuaternionDbl absPartRot = rotB * (QuaternionDbl)localRotB;

                    Vector3Dbl localPartPosA = QuaternionDbl.Inverse( rotA ) * (absPartPos - posA);
                    QuaternionDbl localPartRotA = QuaternionDbl.Inverse( rotA ) * absPartRot;

                    part.transform.SetParent( remainingVessel.transform, false );
                    part.transform.localPosition = (Vector3)localPartPosA;
                    part.transform.localRotation = (Quaternion)localPartRotA;

                    remainingVessel.Graph.AddNode( part );
                }

                remainingVessel.Graph.MergeGraph( mergedVessel.Graph );
                remainingVessel.Graph.AddLink( nodeA, nodeB );

                HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_VESSEL_MERGE.ID, new HSPEvent_AFTER_VESSEL_MERGE.Data
                {
                    RemainingVessel = remainingVessel,
                    MergedVessel = mergedVessel
                } );

                // Delete the old vessel since it has no parts left
                VesselFactory.Destroy( mergedVessel );

                remainingVessel.RebuildIslands();
                remainingVessel.RecalculatePartCache();
            }
        }

        public static void Detach( FAttachNode nodeA, FAttachNode nodeB )
        {
            if( nodeA == null || nodeB == null ) return;

            Vessel vessel = nodeA.Part.GetVessel();
            if( vessel != nodeB.Part.GetVessel() ) return; // Not in the same vessel

            vessel.Graph.RemoveLink( nodeA, nodeB );

            // Check if graph split
            var connectedComponents = vessel.Graph.GetConnectedComponents();

            if( connectedComponents.Count > 1 )
            {
                // Component 0 stays in the original vessel.
                var primaryComponent = connectedComponents[0];

                for( int i = 1; i < connectedComponents.Count; i++ )
                {
                    var splitParts = connectedComponents[i];
                    CreateVesselFromSplit( vessel, splitParts );
                }

                vessel.Graph.RetainOnly( primaryComponent );

                if( vessel.RootPart != null && !primaryComponent.Contains( vessel.RootPart.GetComponent<VesselPart>() ) )
                {
                    foreach( var p in primaryComponent )
                    {
                        vessel.RootPart = p.transform;
                        break;
                    }
                }
            }

            vessel.RebuildIslands();
            vessel.RecalculatePartCache();
        }

        private static void CreateVesselFromSplit( Vessel oldVessel, HashSet<VesselPart> splitParts )
        {
            (Vector3 localCoM, float totalMass) = GetSubtreeLocalMassProperties( oldVessel, splitParts );

            Vector3Dbl oldAbsPos = oldVessel.ReferenceFrameTransform.GetAbsolutePosition();
            QuaternionDbl oldAbsRot = oldVessel.ReferenceFrameTransform.GetAbsoluteRotation();
            Vector3Dbl oldAbsVel = oldVessel.ReferenceFrameTransform.GetAbsoluteVelocity();
            Vector3Dbl oldAbsAngVel = oldVessel.ReferenceFrameTransform.GetAbsoluteAngularVelocity();

            Vector3Dbl absCoMPosition = oldAbsPos + oldAbsRot * (Vector3Dbl)localCoM;

            Vector3Dbl oldCoMLocal = (Vector3Dbl)oldVessel.PhysicsTransform.LocalCenterOfMass;
            Vector3Dbl relativePos = (Vector3Dbl)localCoM - oldCoMLocal;
            Vector3Dbl relativePosWorld = oldAbsRot * relativePos;

            Vector3Dbl linearVelAtNewCoM = oldAbsVel + Vector3Dbl.Cross( oldAbsAngVel, relativePosWorld );

            Vessel newVessel = VesselFactory.CreatePartless( HSPSceneManager.GetScene( oldVessel.gameObject ),
                absCoMPosition,
                oldAbsRot,
                linearVelAtNewCoM,
                oldAbsAngVel );

            foreach( var part in splitParts )
            {
                Vector3 partLocalPos = GetLocalPosRelative( part.transform, oldVessel.transform );
                Quaternion partLocalRot = GetLocalRotRelative( part.transform, oldVessel.transform );

                Vector3 preciseLocalPos = partLocalPos - localCoM;
                Quaternion preciseLocalRot = partLocalRot;

                part.transform.SetParent( newVessel.transform, false );

                part.transform.localPosition = preciseLocalPos;
                part.transform.localRotation = preciseLocalRot;

                newVessel.Graph.AddNode( part );
            }

            foreach( var part in splitParts )
            {
                foreach( var link in oldVessel.Graph.GetLinksForPart( part ) )
                {
                    if( splitParts.Contains( link.NodeA.Part ) && splitParts.Contains( link.NodeB.Part ) )
                    {
                        if( !newVessel.Graph.ContainsLink( link ) )
                        {
                            newVessel.Graph.AddLink( link.NodeA, link.NodeB );
                        }
                    }
                }
            }

            VesselPart arbitraryRoot = null;
            foreach( var p in splitParts )
            {
                arbitraryRoot = p;
                break;
            }
            if( arbitraryRoot != null )
            {
                newVessel.RootPart = arbitraryRoot.transform;
            }

            newVessel.RebuildIslands();
            newVessel.RecalculatePartCache();

            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_VESSEL_SPLIT.ID, new HSPEvent_AFTER_VESSEL_SPLIT.Data
            {
                OldVessel = oldVessel,
                NewVessel = newVessel,
                SplitRoot = arbitraryRoot
            } );
        }
    }
}