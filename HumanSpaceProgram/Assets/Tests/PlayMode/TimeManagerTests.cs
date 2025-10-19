using HSP.Time;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace HSP_Tests_PlayMode
{
    public class TimeManagerTests
    {
        [UnityTest]
        public IEnumerator UT_and_OldUT_AdvanceCorrectly()
        {
            GameObject go = new GameObject();
            TimeManager timeManager = go.AddComponent<TimeManager>();
            TimeManager.SetUT( 0 );

            var assertMonoBeh = go.AddComponent<AssertMonoBehaviour>();

            yield return new WaitForFixedUpdate();

            const double tol = 1e-9;
            double startUT = TimeManager.UT;

            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.FixedUpdate, ( frameInfo ) =>
            {

                double oldUT = TimeManager.OldUT;
                double ut = TimeManager.UT;
                double delta = TimeManager.FixedDeltaTime;

                double expectedUT = startUT + (delta * (1 + frameInfo.TotalFixedUpdates));

                Assert.That( ut, Is.EqualTo( expectedUT ).Within( tol ) );
                Assert.That( oldUT, Is.EqualTo( expectedUT - delta ).Within( tol ) );
            } );
            assertMonoBeh.AddAssert( AssertMonoBehaviour.Step.Update, ( frameInfo ) =>
            {
                double oldUT = TimeManager.OldUT;
                double ut = TimeManager.UT;
                double delta = TimeManager.FixedDeltaTime;

                double expectedUT = startUT + (delta * frameInfo.TotalFixedUpdates); // Since it updates at the start of fixedupdate, the correct value here is the same as in the last fixedupdate.

                Assert.That( ut, Is.EqualTo( expectedUT ).Within( tol ) );
                Assert.That( oldUT, Is.EqualTo( expectedUT - delta ).Within( tol ) );
            } );
            assertMonoBeh.Enable();

            yield return new WaitForSeconds( 2 );

            UnityEngine.Object.DestroyImmediate( go );
        }
    }
}