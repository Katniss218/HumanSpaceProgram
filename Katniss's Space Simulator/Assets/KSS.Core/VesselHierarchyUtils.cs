using KSS.Core.Components;
using KSS.Core.Physics;
using KSS.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// Helper class responsible for changing the state of a part or vessel.
    /// </summary>
    /// <remarks>
    /// The public methods in this class are guaranteed to never produce an invalid state (i.e. a vessel with an unparented part that's not its root).
    /// </remarks>
    public static class VesselHierarchyUtils
    {

        /// <summary>
        /// Change the state of the part hierarchy.
        /// </summary>
        /// <remarks>
        /// This method is exhaustive. <br/>
        /// Support for: <br/>
        /// - splitting vessels (<paramref name="part"/> = any non-root, <paramref name="parentPart"/> = null). <br/>
        /// - joining vessels (<paramref name="part"/> = any, <paramref name="parentPart"/> = any, different vessel), 
        ///     if <paramref name="part"/> is a root, it will delete the <paramref name="part"/>'s vessel. <br/>
        /// - re-parenting parts (<paramref name="part"/> = any, <paramref name="parentPart"/> = any, same vessel). <br/>
        /// - re-rooting the vessel (<paramref name="part"/> = <see cref="Vessel.RootPart"/>, <paramref name="parentPart"/> = any non-root, same vessel). <br/>
        /// </remarks>
        /// <param name="part">The part to set as a child of <paramref name="parentPart"/>.</param>
        /// <param name="parentPart">The part to set as the parent of <paramref name="part"/></param>
        public static void SetParent( Transform part, Transform parentPart )
        {
            if( part == null )
            {
                throw new ArgumentNullException( nameof( part ), $"Part to reparent can't be null." );
            }

            if( parentPart == null )
            {
                // We could either set the part to root, but that's handled by picking the root part.
                // We could detach the parts from the vessel entirely, but that's not a legal state.
                // We could create a new vessel with the specified part as its root.

                if( part.IsRootOfPartObject() )
                {
                    // create a new vessel, but the part is already the root of a new vessel. This is equivalent to recreating the original vessel and deleting the old one (i.e. a "do nothing").
                    return;
                }

                MakeNewVesselOrBuilding( part );
                return;
            }

            if( part.GetVessel() == parentPart.GetVessel() )
            {
                if( part.IsRootOfPartObject() )
                {
                    SwapRoots( part, parentPart ); // Re-rooting as in KSP would be `Reparent( newRoot.Vessel.RootPart, newRoot )`
                }
                else
                {
                    Reattach( part, parentPart );
                }
            }
            else
            {
                if( part.IsRootOfPartObject() )
                {
                    JoinVesselsRoot( part, parentPart );
                }
                else
                {
                    JoinVesselsNotRoot( part, parentPart );
                }
            }
        }

        public static void AttachLoose( Transform looseRoot, Transform parent )
        {
            Vessel v = parent.GetVessel();
            looseRoot.SetParent( parent, true );
            v.RecalculateParts();
        }

        // The following helper methods must always produce a legal state or throw an exception.

        /// <summary>
        /// Parents oldRoot to newRoot, and makes newRoot the root on the vessel.
        /// </summary>
        private static void SwapRoots( Transform oldRoot, Transform newRoot )
        {
            Contract.Assert( oldRoot.GetVessel() == newRoot.GetVessel() );
            Contract.Assert( oldRoot.IsRootOfPartObject() );
            Contract.Assert( !newRoot.IsRootOfPartObject() );

            newRoot.GetVessel().RootPart = newRoot;
            Reattach( oldRoot, newRoot );
        }

        /// <summary>
        /// Attaches the part to the parent (assuming both are on the same vessel, and part is not its root).
        /// </summary>
        private static void Reattach( Transform part, Transform parent )
        {
            Contract.Assert( part.GetVessel() == parent.GetVessel() );
            Contract.Assert( !part.IsRootOfPartObject() );

            part.SetParent( parent );
        }

        private static void JoinVesselsRoot( Transform partToJoin, Transform parent )
        {
            Contract.Assert( partToJoin.GetVessel() != parent.GetVessel() );
            Contract.Assert( partToJoin.IsRootOfPartObject() );

            Vessel oldVessel = partToJoin.GetVessel();
            oldVessel.RootPart = null; // needed for the assert in the next method.

            JoinVesselsNotRoot( partToJoin, parent );

            // If part being joined is the root, we need to delete the vessel being joined, since it would become partless after joining.
            VesselFactory.Destroy( oldVessel );
        }

        /// <summary>
        /// Joins the partToJoin to parent's vessel, using the parent as the parent for partToJoin.
        /// </summary>
        private static void JoinVesselsNotRoot( Transform partToJoin, Transform parent )
        {
            Contract.Assert( partToJoin.GetVessel() != parent.GetVessel() );
            Contract.Assert( !partToJoin.IsRootOfPartObject() );
            // Move partToJoin to parent's vessel.
            // Attach partToJoin to parent.

            Vessel oldv = partToJoin.GetVessel();
            Reattach( partToJoin, parent );
            partToJoin.SetParent( parent.GetVessel().transform );

            oldv.RecalculateParts();
            parent.GetVessel().RecalculateParts();
        }

        /// <summary>
        /// Splits off the part from its original vessel, and makes a new vessel with it as its root.
        /// </summary>
        private static void MakeNewVesselOrBuilding( Transform partToSplit )
        {
            Contract.Assert( !partToSplit.IsRootOfPartObject() );

            // Detach the parts from the old vessel.
            Vessel oldv = partToSplit.GetVessel();

#warning TODO - Use linear and angular velocities of part that works correctly for spinning vessels.
            Vessel newVessel = VesselFactory.CreatePartless(
                SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( partToSplit.transform.position ),
                SceneReferenceFrameManager.SceneReferenceFrame.TransformRotation( partToSplit.transform.rotation ),
                oldv.PhysicsObject.Velocity,
                oldv.PhysicsObject.AngularVelocity );

            partToSplit.SetParent( newVessel.transform );
            newVessel.RootPart = partToSplit;
            oldv.RecalculateParts();
            newVessel.RecalculateParts();

            if( IsAnchored( partToSplit ) )
            {
                PinnedPhysicsObject ppo = oldv.GetComponent<PinnedPhysicsObject>();
                newVessel.Pin( ppo.ReferenceBody, ppo.ReferencePosition, ppo.ReferenceRotation );
            }
            else
            {
#warning TODO - Fixing oldv's AIRF pos/rot shouldn't be required here. RE: RootObjectTransform airfpos is incorrect.
               // oldv.AIRFPosition = SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( oldv.RootPart.position );
               // oldv.AIRFRotation = SceneReferenceFrameManager.SceneReferenceFrame.TransformRotation( oldv.RootPart.rotation );
            }
        }

        /// <summary>
        /// Sets the root object in the hierarchy to the specified object.
        /// </summary>
        public static void ReRoot( Transform newRoot )
        {
            // To set the root, means to set the parent chain to be a child chain.
            // This can be seen graphically on the following tree:
            /*
                        1
                       / \
                      2   3
                     /   / \ 
                    4   9  [8]   <---- new root
                           / \
                          6   7
            */
            Transform parent = newRoot.parent;
            parent.SetParent( newRoot, true ); // worldPositionStays *might* introduce precision issues if performed far away from origin.
            ReRoot( parent );
        }

        /// <summary>
        /// Checks if the object should be anchored.
        /// </summary>
        public static bool IsAnchored( Transform transform )
        {
            return transform.gameObject.HasComponentInChildren<FAnchor>();
        }

        /// <summary>
        /// Checks if the root of the object should be anchored.
        /// </summary>
        public static bool IsRootAnchored( Transform transform )
        {
            return transform.root.gameObject.HasComponentInChildren<FAnchor>();
        }
    }
}