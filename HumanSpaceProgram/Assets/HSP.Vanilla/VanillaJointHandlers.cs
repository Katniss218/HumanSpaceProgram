using System;
using UnityEngine;
using HSP.Vessels;
using HSP.Vanilla.Components;

namespace HSP.Vanilla
{
    public static class VanillaJointHandlers
    {
        public const string REGISTER_RIGID_JOINT = HSPEvent.NAMESPACE_HSP + ".7f8c1a23-4b5c-4d6e-8f1a-9b0c1d2e3f4a";
        public const string REGISTER_LINEAR_JOINT = HSPEvent.NAMESPACE_HSP + ".1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d";
        public const string REGISTER_ANGULAR_JOINT = HSPEvent.NAMESPACE_HSP + ".2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e";
        public const string REGISTER_BALL_JOINT = HSPEvent.NAMESPACE_HSP + ".3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, REGISTER_RIGID_JOINT )]
        private static void RegisterRigidJoint()
        {
            VesselJointRegistry.RegisterHandler<FAttachNode_Rigid, FAttachNode_Rigid>( CreateRigidJoint );
        }

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, REGISTER_LINEAR_JOINT )]
        private static void RegisterLinearJoint()
        {
            VesselJointRegistry.RegisterHandler<FAttachNode_LinearBase, FAttachNode_LinearHead>( CreateLinearJoint );
        }

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, REGISTER_ANGULAR_JOINT )]
        private static void RegisterAngularJoint()
        {
            VesselJointRegistry.RegisterHandler<FAttachNode_AngularBase, FAttachNode_AngularHead>( CreateAngularJoint );
        }

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, REGISTER_BALL_JOINT )]
        private static void RegisterBallJoint()
        {
            VesselJointRegistry.RegisterHandler<FAttachNode_BallBase, FAttachNode_BallHead>( CreateBallJoint );
        }

        private static ConfigurableJoint CreateBaseJoint( FAttachNode nodeA, FAttachNode nodeB )
        {
#warning TODO - we need a helper method here for rigidbody / island management. do not do it inline.
            Rigidbody rbA = nodeA.Part != null
                ? nodeA.Part.GetComponentInParent<Rigidbody>()
                : null;

            Rigidbody rbB = nodeB.Part != null
                ? nodeB.Part.GetComponentInParent<Rigidbody>()
                : null;

            if( rbA == null || rbB == null || rbA == rbB )
                return null;

            ConfigurableJoint joint = rbA.gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = rbB;

            // Configure anchors in local space
            joint.anchor = rbA.transform.InverseTransformPoint( nodeA.transform.position );
            joint.connectedAnchor = rbB.transform.InverseTransformPoint( nodeB.transform.position );

            // Set primary and secondary axes aligned with nodeA's orientation
            joint.axis = rbA.transform.InverseTransformDirection( nodeA.transform.forward );
            joint.secondaryAxis = rbA.transform.InverseTransformDirection( nodeA.transform.up );

            return joint;
        }

        private static ConfigurableJoint CreateRigidJoint( FAttachNode nodeA, FAttachNode nodeB )
        {
            ConfigurableJoint joint = CreateBaseJoint( nodeA, nodeB );
            if( joint == null )
                return null;

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            return joint;
        }

        private static ConfigurableJoint CreateLinearJoint( FAttachNode nodeA, FAttachNode nodeB )
        {
            ConfigurableJoint joint = CreateBaseJoint( nodeA, nodeB );
            if( joint == null )
                return null;

            FAttachNode_LinearBase baseNode = (FAttachNode_LinearBase)nodeA;

            // Allow translation along the X axis (or primary axis)
            joint.xMotion = ConfigurableJointMotion.Limited;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            // Configure limits
            float limitRange = Mathf.Max( 0.001f, baseNode.MaxLimit - baseNode.MinLimit );
            joint.linearLimit = new SoftJointLimit { limit = limitRange };

            if( baseNode.Spring > 0f || baseNode.Damping > 0f )
            {
                joint.linearLimitSpring = new SoftJointLimitSpring { spring = baseNode.Spring, damper = baseNode.Damping };
            }

            return joint;
        }

        private static ConfigurableJoint CreateAngularJoint( FAttachNode nodeA, FAttachNode nodeB )
        {
            ConfigurableJoint joint = CreateBaseJoint( nodeA, nodeB );
            if( joint == null )
                return null;

            FAttachNode_AngularBase baseNode = (FAttachNode_AngularBase)nodeA;

            // Allow angular rotation around X-axis (primary/twist axis)
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            // Configure limits
            joint.lowAngularXLimit = new SoftJointLimit { limit = baseNode.MinAngle };
            joint.highAngularXLimit = new SoftJointLimit { limit = baseNode.MaxAngle };

            if( baseNode.Spring > 0f || baseNode.Damping > 0f )
            {
                joint.angularXLimitSpring = new SoftJointLimitSpring { spring = baseNode.Spring, damper = baseNode.Damping };
            }

            return joint;
        }

        private static ConfigurableJoint CreateBallJoint( FAttachNode nodeA, FAttachNode nodeB )
        {
            ConfigurableJoint joint = CreateBaseJoint( nodeA, nodeB );
            if( joint == null )
                return null;

            FAttachNode_BallBase baseNode = (FAttachNode_BallBase)nodeA;

            // Allow all angular rotations (X/Y/Z)
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

            // Configure limits
            joint.lowAngularXLimit = new SoftJointLimit { limit = -baseNode.XLimit };
            joint.highAngularXLimit = new SoftJointLimit { limit = baseNode.XLimit };
            joint.angularYLimit = new SoftJointLimit { limit = baseNode.YLimit };
            joint.angularZLimit = new SoftJointLimit { limit = baseNode.ZLimit };

            if( baseNode.Spring > 0f || baseNode.Damping > 0f )
            {
                var spring = new SoftJointLimitSpring { spring = baseNode.Spring, damper = baseNode.Damping };
                joint.angularXLimitSpring = spring;
                joint.angularYZLimitSpring = spring;
            }

            return joint;
        }
    }
}