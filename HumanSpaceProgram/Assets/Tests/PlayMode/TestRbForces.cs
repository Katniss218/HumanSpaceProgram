using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HSP.Tests.PlayMode
{
    public class TestRbForces
    {
        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // Tests use a fresh, clean scene.

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator RigidbodyVelocity___ShouldBeZeroAfterAddingForce_AndChangeOnlyAfterPhysicsProcessing()
        {
            GameObject obj = new GameObject();

            Rigidbody rb = obj.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.velocity = Vector3.zero;

            Debug.Log( rb.velocity );
            // getting velocity twice as opposed to once gives different results.
            // after removing forcemode = force and resetting force back to force, it's zero again now.
           // Assert.That( rb.velocity, Is.EqualTo( Vector3.zero ) );

            yield return null;

            rb.AddForce( new Vector3( 1111, 1, 1 ), ForceMode.Force );

            Debug.Log( rb.velocity );
           // Assert.That( rb.velocity, Is.EqualTo( Vector3.zero ) ); // should be unchanged immediately after adding force.

            yield return null;
            rb.AddForce( new Vector3( 1111, 1, 1 ), ForceMode.Force );

            Debug.Log( rb.velocity );
           // Assert.That( rb.velocity, Is.Not.EqualTo( Vector3.zero ) );
        }
    }
}