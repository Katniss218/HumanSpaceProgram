using HSP.ReferenceFrames;
using HSP.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.Vessels
{
    public struct VesselSubtreePhysicsState
    {
        public Vector3Dbl AbsoluteCoM;
        public Vector3Dbl AbsoluteLinearVelocity;
        public Vector3Dbl AbsoluteAngularVelocity;
        public float TotalMass;
    }

    /// <summary>
    /// Helper class responsible for changing the state of a part or vessel.
    /// </summary>
    public static class VesselHierarchyUtils
    {
        private static (Vector3 pos, Quaternion rot) GetLocalPoseRelative( Transform descendant, Transform ancestor )
        {
            if( descendant == ancestor )
                return (Vector3.zero, Quaternion.identity);

            Vector3 localPos = Vector3.zero;
            Quaternion localRot = Quaternion.identity;
            Transform current = descendant;

            while( current != null && current != ancestor )
            {
                localPos = current.localPosition + current.localRotation * Vector3.Scale( current.localScale, localPos );
                localRot = current.localRotation * localRot;
                current = current.parent;
            }

            return (localPos, localRot);
        }

        private static (VesselSubtreePhysicsState state, Vector3 localCoM) GetSubtreePhysicsState( IVessel vessel, IEnumerable<VesselPart> parts )
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
                        (Vector3 partLocalPos, _) = GetLocalPoseRelative( comp.transform, vessel.transform );
                        localCoMAccumulator += partLocalPos * m;
                    }
                }
            }

            Vector3 localCoM;
            if( totalMass <= 0 )
            {
                Vector3 avgLocalPosAccumulator = Vector3.zero;
                int count = 0;
                foreach( var part in parts )
                {
                    (Vector3 partLocalPos, _) = GetLocalPoseRelative( part.transform, vessel.transform );
                    avgLocalPosAccumulator += partLocalPos;
                    count++;
                }
                localCoM = count > 0 ? avgLocalPosAccumulator / count : Vector3.zero;
            }
            else
            {
                localCoM = localCoMAccumulator / totalMass;
            }

            Vector3Dbl oldAbsPos = vessel.ReferenceFrameTransform.GetAbsolutePosition();
            QuaternionDbl oldAbsRot = vessel.ReferenceFrameTransform.GetAbsoluteRotation();
            Vector3Dbl oldAbsVel = vessel.ReferenceFrameTransform.GetAbsoluteVelocity();
            Vector3Dbl oldAbsAngVel = vessel.ReferenceFrameTransform.GetAbsoluteAngularVelocity();

            Vector3Dbl absCoMPosition = oldAbsPos + oldAbsRot * (Vector3Dbl)localCoM;

            Vector3Dbl oldCoMLocal = (Vector3Dbl)vessel.PhysicsTransform.LocalCenterOfMass;
            Vector3Dbl relativePos = (Vector3Dbl)localCoM - oldCoMLocal;
            Vector3Dbl relativePosWorld = oldAbsRot * relativePos;

            Vector3Dbl linearVelAtNewCoM = oldAbsVel + Vector3Dbl.Cross( oldAbsAngVel, relativePosWorld );

            var state = new VesselSubtreePhysicsState
            {
                AbsoluteCoM = absCoMPosition,
                AbsoluteLinearVelocity = linearVelAtNewCoM,
                AbsoluteAngularVelocity = oldAbsAngVel,
                TotalMass = totalMass
            };

            return (state, localCoM);
        }

        public static void Attach( FAttachNode.SnappingCandidate candidate )
        {
            Attach( candidate.snappedNode, candidate.targetNode );
        }

        private class SurfaceAttachNode : FAttachNode { }

        public static void SurfaceAttach( VesselPart partA, VesselPart partB )
        {
            //var dummyA = new SurfaceAttachNode();
            //var dummyB = new SurfaceAttachNode();

            //Attach( dummyA, dummyB );
            throw new NotImplementedException();
        }

        public static void Attach( FAttachNode nodeA, FAttachNode nodeB )
        {
            if( nodeA == null || nodeB == null )
            {
                throw new ArgumentNullException( "Nodes to attach cannot be null." );
            }

            if( nodeA == nodeB )
            {
                throw new ArgumentException( "Nodes to attach cannot be the same node." );
            }

            VesselPart partA = nodeA.Part;
            VesselPart partB = nodeB.Part;

            if( partA == null || partB == null )
                return;

            Vessel vesselA = partA.Vessel;
            Vessel vesselB = partB.Vessel;

            VesselAttachmentGraph newGraph;
            var graphA = vesselA.Attachments;
            var graphB = vesselB.Attachments;
            if( vesselA == null && vesselB == null )
            {
                Vessel newVessel = VesselFactory.CreatePartless<Vessel>( HSPSceneManager.GetScene( partA.gameObject ),
                    partA.transform.position,
                    partA.transform.rotation,
                    Vector3Dbl.zero,
                    Vector3Dbl.zero );

                newGraph = VesselAttachmentGraph.Merge( graphA, graphB, partA, partB );

                newVessel.SetGraph( newGraph );
                return;
            }
            if( vesselA == null )
            {
                newGraph = VesselAttachmentGraph.Merge( graphA, vesselB.Attachments, partA, partB );
                vesselB.SetGraph( newGraph );
                return;
            }
            if( vesselB == null )
            {
                newGraph = VesselAttachmentGraph.Merge( vesselA.Attachments, graphB, partA, partB );
                vesselA.SetGraph( newGraph );
                return;
            }
            if( vesselA == vesselB )
            {
                newGraph = VesselAttachmentGraph.AddEdge( vesselA.Attachments, partA, partB );
                vesselA.SetGraph( newGraph );
                return;
            }

            var mergedParts = vesselA.Parts.ToList();
            newGraph = VesselAttachmentGraph.Merge( vesselB.Attachments, vesselA.Attachments, partB, partA );
            vesselB.SetGraph( newGraph );

            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_VESSEL_MERGE.ID, new HSPEvent_AFTER_VESSEL_MERGE.Data()
            {
                RemainingVessel = vesselB,
                MergedVessel = vesselA,
                MergedParts = mergedParts
            } );

            VesselFactory.Destroy( vesselA );
        }

        public static void Detach( FAttachNode nodeA )
        {
            FAttachNode nodeB = null; // @@todo - get from graph.

            if( nodeA == null || nodeB == null )
                return;

            Vessel vessel = nodeA.transform.GetVessel();
            if( vessel != nodeB.transform.GetVessel() )
                return;

            var newGraphs = VesselAttachmentGraph.RemoveEdge( vessel.Attachments, nodeA.transform.GetComponentInParent<VesselPart>(), nodeB.transform.GetComponentInParent<VesselPart>() );

            if( newGraphs.extraGraph == null )
            {
                // Edge removal did not cause a split
                vessel.SetGraph( newGraphs.graph );
            }
            else
            {
                var remainingGraph = newGraphs.graph;
                var splitGraph = newGraphs.extraGraph;

                // First extract the parts for the new vessel to avoid them being deleted/modified
                var splitParts = new HashSet<VesselPart>( splitGraph.Nodes );

                // Set the remaining graph on the original vessel.
                vessel.SetGraph( remainingGraph );

                CreateVesselFromSplit( vessel, splitParts, splitGraph );
            }
        }

        public static bool TryDetach( VesselPart part )
        {
            if( part == null || part.Vessel == null )
                return false;

            Vessel vessel = part.Vessel;

            // Assume the first part in the vessel's Parts list is the root.
            VesselPart rootPart = vessel.Parts.FirstOrDefault();
            if( rootPart == null )
                return false;

            if( part == rootPart )
            {
                // The whole vessel is being picked up, no need to split.
                return true;
            }

            // Create a directed graph from the root
            var directedGraph = DirectedVesselAttachmentGraph.Create( vessel.Attachments, rootPart );

            // Find the subgraph starting from the clicked part
            var detachedParts = directedGraph.GetSubgraph( part );

            // Remove edges connecting the detached subgraph from the rest of the graph
            var newAdjacency = new Dictionary<VesselPart, List<AttachmentEdge>>();
            foreach( var node in vessel.Attachments.Nodes )
            {
                newAdjacency[node] = new List<AttachmentEdge>( vessel.Attachments.GetEdges( node ) );
            }

            // Find edges that cross between the remaining parts and the detached parts, and remove them
            var remainingParts = vessel.Attachments.Nodes.Except( detachedParts ).ToHashSet();

            foreach( var detachedNode in detachedParts )
            {
                newAdjacency[detachedNode].RemoveAll( e => remainingParts.Contains( e.Target ) );
            }
            foreach( var remainingNode in remainingParts )
            {
                newAdjacency[remainingNode].RemoveAll( e => detachedParts.Contains( e.Target ) );
            }

            // The remaining graph is valid because we only cut edges between remaining and detached.
            var remainingAdjacency = new Dictionary<VesselPart, List<AttachmentEdge>>();
            foreach( var remainingNode in remainingParts )
            {
                remainingAdjacency[remainingNode] = newAdjacency[remainingNode];
            }
            var remainingGraph = VesselAttachmentGraph.CreateValidated( remainingAdjacency );

            var detachedAdjacency = new Dictionary<VesselPart, List<AttachmentEdge>>();
            foreach( var detachedNode in detachedParts )
            {
                detachedAdjacency[detachedNode] = newAdjacency[detachedNode];
            }
            var detachedGraph = VesselAttachmentGraph.CreateValidated( detachedAdjacency );

            // Apply remaining graph to the original vessel
            vessel.SetGraph( remainingGraph );

            // Create a new vessel for the detached parts
            CreateVesselFromSplit( vessel, detachedParts, detachedGraph );

            return true;
        }

        private static void CreateVesselFromSplit( IVessel oldVessel, HashSet<VesselPart> splitParts, VesselAttachmentGraph splitGraph )
        {
            (VesselSubtreePhysicsState state, Vector3 localCoM) = GetSubtreePhysicsState( oldVessel, splitParts );

            Vessel newVessel = VesselFactory.CreatePartless<Vessel>( HSPSceneManager.GetScene( oldVessel.gameObject ),
                state.AbsoluteCoM,
                oldVessel.ReferenceFrameTransform.GetAbsoluteRotation(),
                state.AbsoluteLinearVelocity,
                state.AbsoluteAngularVelocity );

            foreach( var part in splitParts )
            {
                (Vector3 partLocalPos, Quaternion partLocalRot) = GetLocalPoseRelative( part.transform, oldVessel.transform );

                Vector3 preciseLocalPos = partLocalPos - localCoM;
                Quaternion preciseLocalRot = partLocalRot;

                part.transform.SetParent( newVessel.transform, false );

                part.transform.localPosition = preciseLocalPos;
                part.transform.localRotation = preciseLocalRot;
            }

            newVessel.SetGraph( splitGraph );

            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_VESSEL_SPLIT.ID, new HSPEvent_AFTER_VESSEL_SPLIT.Data
            {
                OldVessel = oldVessel,
                NewVessel = newVessel,
                SplitParts = splitParts.ToList()
            } );
        }
    }
}