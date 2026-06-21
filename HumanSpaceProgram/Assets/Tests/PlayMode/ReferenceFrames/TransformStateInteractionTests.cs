using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vanilla.ReferenceFrames;
using HSP.Vanilla.Scenes.GameplayScene;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityPlus.PlayerLoop;

namespace HSP_Tests_PlayMode.ReferenceFrames
{
    [Flags]
    public enum TransformProperties
    {
        None = 0,
        Position = 1 << 0,
        Rotation = 1 << 1,
        Velocity = 1 << 2,
        AngularVelocity = 1 << 3,
        SkipRigidbodyVelocity = 1 << 4,
        SkipRigidbodyAngularVelocity = 1 << 5,
        All = Position | Rotation | Velocity | AngularVelocity
    }

    public class TransformStateInteractionTests : ReferenceFrameTransformTestBase
    {
        private const TransformProperties PosRot = TransformProperties.Position | TransformProperties.Rotation;

        public static TestCaseData[] TransformStateInteractionTestCases =
        {
            new TestCaseData( typeof( FreeReferenceFrameTransform ), TransformProperties.All, (Action<IReferenceFrameTransform>)null ).Returns(null).SetName( "{m}(FreeReferenceFrameTransform, All)" ),
            new TestCaseData( typeof( KinematicReferenceFrameTransform ), TransformProperties.All | TransformProperties.SkipRigidbodyVelocity | TransformProperties.SkipRigidbodyAngularVelocity, (Action<IReferenceFrameTransform>)null ).Returns(null).SetName( "{m}(KinematicReferenceFrameTransform, All)" ),
            new TestCaseData( typeof( HybridReferenceFrameTransform ), TransformProperties.All, (Action<IReferenceFrameTransform>)null ).Returns(null).SetName( "{m}(HybridReferenceFrameTransform_SceneSpace, All)" ),
            new TestCaseData( typeof( HybridReferenceFrameTransform ), TransformProperties.All | TransformProperties.SkipRigidbodyVelocity | TransformProperties.SkipRigidbodyAngularVelocity, (Action<IReferenceFrameTransform>)(sut => sut.SetAbsolutePosition(new Vector3Dbl(1e8, 0, 0))) ).Returns(null).SetName( "{m}(HybridReferenceFrameTransform_AbsoluteSpace, All)" ),
            new TestCaseData( typeof( FixedReferenceFrameTransform ), PosRot, (Action<IReferenceFrameTransform>)null ).Returns(null).SetName( "{m}(FixedReferenceFrameTransform, PosRot)" ),
            new TestCaseData( typeof( DummyReferenceFrameTransform ), PosRot, (Action<IReferenceFrameTransform>)null ).Returns(null).SetName( "{m}(DummyReferenceFrameTransform, PosRot)" ),
            new TestCaseData( typeof( PinnedReferenceFrameTransform ), PosRot, (Action<IReferenceFrameTransform>)null ).Returns(null).SetName( "{m}(PinnedReferenceFrameTransform, PosRot)" ),
        };

        [UnityTest]
        [TestCaseSource( nameof( TransformStateInteractionTestCases ) )]
        public IEnumerator ManualValueSetting( Type transformType, TransformProperties properties, Action<IReferenceFrameTransform> setupAction )
        {
            IReferenceFrameTransform sut = CreateSut( transformType );

            setupAction?.Invoke( sut );

            yield return new WaitForFixedUpdate();

            var testFrames = new Func<IReferenceFrame>[]
            {
                () => new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero ),
                () => new OrientedReferenceFrame( TimeManager.UT, Vector3Dbl.zero, QuaternionDbl.Euler( 0, 45, 0 ) ),
                () => new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, new Vector3Dbl( 100, 0, 0 ) ),
            };

            foreach( var frameGetter in testFrames )
            {
                refFrameManager.RequestReferenceFrameSwitch( frameGetter.Invoke() );
                yield return new WaitForFixedUpdate();

                if( properties.HasFlag( TransformProperties.Position ) ) TestPosition( sut, new Vector3Dbl( 10, 20, 30 ), new Vector3( 5, 10, 15 ), "FixedUpdate" );
                if( properties.HasFlag( TransformProperties.Rotation ) ) TestRotation( sut, QuaternionDbl.Euler( 15, 30, 45 ), Quaternion.Euler( 10, 20, 30 ), "FixedUpdate" );
                if( properties.HasFlag( TransformProperties.Velocity ) ) TestVelocity( sut, new Vector3Dbl( 1, 2, 3 ), new Vector3( 0.5f, 1.0f, 1.5f ), "FixedUpdate", properties.HasFlag( TransformProperties.SkipRigidbodyVelocity ) );
                if( properties.HasFlag( TransformProperties.AngularVelocity ) ) TestAngularVelocity( sut, new Vector3Dbl( 0.1, 0.2, 0.3 ), new Vector3( 0.05f, 0.1f, 0.15f ), "FixedUpdate", properties.HasFlag( TransformProperties.SkipRigidbodyAngularVelocity ) );

                yield return null; // Update phase

                if( properties.HasFlag( TransformProperties.Position ) ) TestPosition( sut, new Vector3Dbl( 25, 35, 45 ), new Vector3( 8, 12, 16 ), "Update" );
                if( properties.HasFlag( TransformProperties.Rotation ) ) TestRotation( sut, QuaternionDbl.Euler( 30, 45, 60 ), Quaternion.Euler( 20, 30, 40 ), "Update" );
            }

            DestroySut( sut );
        }

        [UnityTest]
        [TestCaseSource( nameof( TransformStateInteractionTestCases ) )]
        public IEnumerator ManualValueSetting_WhenDisabled( Type transformType, TransformProperties properties, Action<IReferenceFrameTransform> setupAction )
        {
            IReferenceFrameTransform sut = CreateSut( transformType );

            setupAction?.Invoke( sut );

            sut.gameObject.SetActive( false );

            if( properties.HasFlag( TransformProperties.Position ) ) TestPosition( sut, new Vector3Dbl( 10, 20, 30 ), new Vector3( 5, 10, 15 ), "Disabled" );
            if( properties.HasFlag( TransformProperties.Rotation ) ) TestRotation( sut, QuaternionDbl.Euler( 15, 30, 45 ), Quaternion.Euler( 10, 20, 30 ), "Disabled" );
            if( properties.HasFlag( TransformProperties.Velocity ) ) TestVelocity( sut, new Vector3Dbl( 1, 2, 3 ), new Vector3( 0.5f, 1.0f, 1.5f ), "Disabled", properties.HasFlag( TransformProperties.SkipRigidbodyVelocity ) );
            if( properties.HasFlag( TransformProperties.AngularVelocity ) ) TestAngularVelocity( sut, new Vector3Dbl( 0.1, 0.2, 0.3 ), new Vector3( 0.05f, 0.1f, 0.15f ), "Disabled", properties.HasFlag( TransformProperties.SkipRigidbodyAngularVelocity ) );

            DestroySut( sut );
            yield break;
        }

        private void TestPosition( IReferenceFrameTransform sut, Vector3Dbl absVal, Vector3 localVal, string context )
        {
            sut.SetAbsolutePosition( absVal );
            Assert.That( sut.GetAbsolutePosition(), Is.EqualTo( absVal ).Using( vector3DblApproxComparer ), $"{context}: Absolute position should match SET value" );
            Assert.That( sut.transform.position, Is.EqualTo( (Vector3)GameplaySceneReferenceFrameManager.ReferenceFrame.InverseTransformPosition( absVal ) ).Using( vector3ApproxComparer ), $"{context}: Unity transform position should match expected local position after absolute position SET" );
            if( sut is Component comp && comp.TryGetComponent<Rigidbody>( out var rb ) )
            {
                Assert.That( rb.position, Is.EqualTo( sut.transform.position ).Using( vector3ApproxComparer ), $"{context}: Rigidbody position should immediately match unity transform position after absolute position SET" );
            }

            sut.SetPosition( localVal );
            Assert.That( sut.GetPosition(), Is.EqualTo( localVal ).Using( vector3ApproxComparer ), $"{context}: Local scene position should match SET value" );
            Assert.That( sut.transform.position, Is.EqualTo( localVal ).Using( vector3ApproxComparer ), $"{context}: Unity transform position should match local position SET" );
            if( sut is Component comp2 && comp2.TryGetComponent<Rigidbody>( out var rb2 ) )
            {
                Assert.That( rb2.position, Is.EqualTo( localVal ).Using( vector3ApproxComparer ), $"{context}: Rigidbody position should immediately match local position SET" );
            }
        }

        private void TestRotation( IReferenceFrameTransform sut, QuaternionDbl absVal, Quaternion localVal, string context )
        {
            sut.SetAbsoluteRotation( absVal );
            Assert.That( sut.GetAbsoluteRotation(), Is.EqualTo( absVal ).Using( quaternionDblApproxComparer ), $"{context}: Absolute rotation should match SET value" );
            Assert.That( sut.transform.rotation, Is.EqualTo( (Quaternion)GameplaySceneReferenceFrameManager.ReferenceFrame.InverseTransformRotation( absVal ) ).Using( quaternionApproxComparer ), $"{context}: Unity transform rotation should match expected local rotation after absolute rotation SET" );
            if( sut is Component comp && comp.TryGetComponent<Rigidbody>( out var rb ) )
            {
                Assert.That( rb.rotation, Is.EqualTo( sut.transform.rotation ).Using( quaternionApproxComparer ), $"{context}: Rigidbody rotation should immediately match unity transform rotation after absolute rotation SET" );
            }

            sut.SetRotation( localVal );
            Assert.That( sut.GetRotation(), Is.EqualTo( localVal ).Using( quaternionApproxComparer ), $"{context}: Local scene rotation should match SET value" );
            Assert.That( sut.transform.rotation, Is.EqualTo( localVal ).Using( quaternionApproxComparer ), $"{context}: Unity transform rotation should match local rotation SET" );
            if( sut is Component comp2 && comp2.TryGetComponent<Rigidbody>( out var rb2 ) )
            {
                Assert.That( rb2.rotation, Is.EqualTo( localVal ).Using( quaternionApproxComparer ), $"{context}: Rigidbody rotation should immediately match local rotation SET" );
            }
        }

        private void TestVelocity( IReferenceFrameTransform sut, Vector3Dbl absVal, Vector3 localVal, string context, bool skipRigidbodyCheck )
        {
            sut.SetAbsoluteVelocity( absVal );
            Assert.That( sut.GetAbsoluteVelocity(), Is.EqualTo( absVal ).Using( vector3DblApproxComparer ), $"{context}: Absolute velocity should match SET value" );
            if( !skipRigidbodyCheck && sut is Component comp && comp.TryGetComponent<Rigidbody>( out var rb ) )
            {
                Assert.That( rb.velocity, Is.EqualTo( (Vector3)GameplaySceneReferenceFrameManager.ReferenceFrame.InverseTransformVelocity( absVal ) ).Using( vector3ApproxComparer ), $"{context}: Rigidbody velocity should immediately match expected local velocity after absolute velocity SET" );
            }

            sut.SetVelocity( localVal );
            Assert.That( sut.GetVelocity(), Is.EqualTo( localVal ).Using( vector3ApproxComparer ), $"{context}: Local scene velocity should match SET value" );
            if( !skipRigidbodyCheck && sut is Component comp2 && comp2.TryGetComponent<Rigidbody>( out var rb2 ) )
            {
                Assert.That( rb2.velocity, Is.EqualTo( localVal ).Using( vector3ApproxComparer ), $"{context}: Rigidbody velocity should immediately match local velocity SET" );
            }
        }

        private void TestAngularVelocity( IReferenceFrameTransform sut, Vector3Dbl absVal, Vector3 localVal, string context, bool skipRigidbodyCheck )
        {
            sut.SetAbsoluteAngularVelocity( absVal );
            Assert.That( sut.GetAbsoluteAngularVelocity(), Is.EqualTo( absVal ).Using( vector3DblApproxComparer ), $"{context}: Absolute angular velocity should match SET value" );
            if( !skipRigidbodyCheck && sut is Component comp && comp.TryGetComponent<Rigidbody>( out var rb ) )
            {
                Assert.That( rb.angularVelocity, Is.EqualTo( (Vector3)GameplaySceneReferenceFrameManager.ReferenceFrame.InverseTransformAngularVelocity( absVal ) ).Using( vector3ApproxComparer ), $"{context}: Rigidbody angular velocity should immediately match expected local angular velocity after absolute angular velocity SET" );
            }

            sut.SetAngularVelocity( localVal );
            Assert.That( sut.GetAngularVelocity(), Is.EqualTo( localVal ).Using( vector3ApproxComparer ), $"{context}: Local scene angular velocity should match SET value" );
            if( !skipRigidbodyCheck && sut is Component comp2 && comp2.TryGetComponent<Rigidbody>( out var rb2 ) )
            {
                Assert.That( rb2.angularVelocity, Is.EqualTo( localVal ).Using( vector3ApproxComparer ), $"{context}: Rigidbody angular velocity should immediately match local angular velocity SET" );
            }
        }
    }
}
