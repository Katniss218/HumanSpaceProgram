using HSP.Trajectories;
using NUnit.Framework;
using UnityEngine;

namespace HSP_Tests_EditMode
{
    public class Ephemeris2Tests
    {
        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // Tests use a fresh, clean scene.

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        [Test]
        public void Insertion_1___IsCorrect()
        {
            // Arrange
            Ephemeris2 sut = new Ephemeris2( 10, maxError: 0, double.PositiveInfinity );

            // Act
            sut.InsertAdaptive( 0, new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 ) );

            // Assert
            Assert.That( sut.Count, Is.EqualTo( 1 ) );
            Assert.That( sut.HighUT, Is.EqualTo( 0 ) );
            Assert.That( sut.LowUT, Is.EqualTo( 0 ) );
        }

        [Test]
        public void Insertion_2___IsCorrect()
        {
            // Arrange
            Ephemeris2 sut = new Ephemeris2( 10, maxError: 0, double.PositiveInfinity );

            // Act
            sut.InsertAdaptive( 0, new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 ) );
            sut.InsertAdaptive( 1, new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 ) );

            // Assert
            Assert.That( sut.Count, Is.EqualTo( 2 ) );
            Assert.That( sut.HighUT, Is.EqualTo( 1 ) );
            Assert.That( sut.LowUT, Is.EqualTo( 0 ) );
        }

        [Test]
        public void Insertion_Three___IsCorrect()
        {
            // Arrange
            Ephemeris2 sut = new Ephemeris2( 10, maxError: 0, double.PositiveInfinity );

            // Act
            sut.InsertAdaptive( 0, new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 ) );
            sut.InsertAdaptive( 1, new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 ) );
            sut.InsertAdaptive( 2, new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 ) );

            // Assert
            Assert.That( sut.Count, Is.EqualTo( 3 ) );
            Assert.That( sut.HighUT, Is.EqualTo( 2 ) );
            Assert.That( sut.LowUT, Is.EqualTo( 0 ) );
        }

        [Test]
        public void Insertion_TooManySamples___Resizes()
        {
            const int initial = 10;
            const int count = 45;
            // Arrange
            Ephemeris2 sut = new Ephemeris2( initial, maxError: 0, double.PositiveInfinity );

            // Act
            double step = 1.0;
            for( int i = 0; i < count; i++ )
            {
                sut.InsertAdaptive( i * step, new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 ) );
            }

            // Assert
            Assert.That( sut.Duration, Is.EqualTo( count - 1 ) ); // 45 samples = 44 intervals between them.
            Assert.That( sut.Capacity, Is.EqualTo( 80 ) ); // 10 * 2^3, resizes 3 times
        }

        [Test]
        public void Insertion_Overflow___SlidesForward()
        {
            const int max = 10;
            const int count = 20;
            // Arrange
            Ephemeris2 sut = new Ephemeris2( max, maxError: 0, max );

            // Act
            double step = 1.0;
            for( int i = 0; i < count; i++ )
            {
                sut.InsertAdaptive( i * step, new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 ) );
            }

            // Assert
            Assert.That( sut.Duration, Is.EqualTo( max ) );
            Assert.That( sut.HighUT, Is.EqualTo( (count - 1) * step ) );
            Assert.That( sut.LowUT, Is.EqualTo( (count - max - 1) * step ) );
        }
        
        [Test]
        public void Insertion_Overflow___SlidesBackward()
        {
            const int max = 10;
            const int count = 20;
            // Arrange
            Ephemeris2 sut = new Ephemeris2( max, maxError: 0, max );

            // Act
            double step = -1.0; // Decreasing ut.
            for( int i = 0; i < count; i++ )
            {
                sut.InsertAdaptive( i * step, new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 ) );
            }

            // Assert
            Assert.That( sut.Duration, Is.EqualTo( max ) );
            Assert.That( sut.HighUT, Is.EqualTo( (count - max - 1) * step ) );
            Assert.That( sut.LowUT, Is.EqualTo( (count - 1) * step ) );
        }
        [Test]
        public void Insertion_Overflow_MultipleTimes___GrowsOnce()
        {
            const int max = 10;
            const int count = 200; // Overflow multiple times.
            // Arrange
            Ephemeris2 sut = new Ephemeris2( max, maxError: 0, max );

            // Act
            double step = 1.0;
            for( int i = 0; i < count; i++ )
            {
                sut.InsertAdaptive( i * step, new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 ) );
            }

            // Assert
            Assert.That( sut.Duration, Is.EqualTo( max ) );
            Assert.That( sut.HighUT, Is.EqualTo( (count - 1) * step ) );
            Assert.That( sut.LowUT, Is.EqualTo( (count - max - 1) * step ) );
            Assert.That( sut.Capacity, Is.EqualTo( 20 ) ); // Capacity should grow once, because samples are spread uniformly, error tolerance is 0.
        }

        [Test]
        public void Evaluate_2___IsCorrect()
        {
            // Arrange
            Ephemeris2 sut = new Ephemeris2( 10, maxError: 0, double.PositiveInfinity );
            var expecteds = new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 );
            var expected = new TrajectoryStateVector( new Vector3Dbl( 0.5, 0.5, 0.5 ), new Vector3Dbl( 0.5, 0.5, 0.5 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 );
            var expectede = new TrajectoryStateVector( new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 );

            // Act
            sut.InsertAdaptive( 0, new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 ) );
            sut.InsertAdaptive( 1, new TrajectoryStateVector( new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 ) );

            // Assert
            Assert.That( sut.Evaluate( 0 ), Is.EqualTo( expecteds ) );
            Assert.That( sut.Evaluate( 0.5 ), Is.EqualTo( expected ) );
            Assert.That( sut.Evaluate( 1 ), Is.EqualTo( expectede ) );
        }

        [Test]
        public void Evaluate_AfterOverflow___IsCorrect()
        {
            const int max = 10;
            const int count = 20;
            // Arrange
            Ephemeris2 sut = new Ephemeris2( max, maxError: 0, double.PositiveInfinity );
            var expecteds = new TrajectoryStateVector( new Vector3Dbl( 10, 10, 10 ), new Vector3Dbl( 10, 10, 10 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 );
            var expected = new TrajectoryStateVector( new Vector3Dbl( 15.7, 15.7, 15.7 ), new Vector3Dbl( 15.7, 15.7, 15.7 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 );
            var expectede = new TrajectoryStateVector( new Vector3Dbl( 19, 19, 19 ), new Vector3Dbl( 19, 19, 19 ), new Vector3Dbl( 0, 0, 0 ), 1000.0 );

            // Act
            double step = 1.0;
            for( int i = 0; i < count; i++ )
            {
                sut.InsertAdaptive( i * step, new TrajectoryStateVector( new Vector3Dbl( i, i, i ), new Vector3Dbl( i, i, i ), new Vector3Dbl( 0, 0, 0 ), 1000.0 ) );
            }
            // Assert
            Assert.That( sut.Evaluate( 10 ), Is.EqualTo( expecteds ) );
            Assert.That( sut.Evaluate( 15.7 ), Is.EqualTo( expected ) );
            Assert.That( sut.Evaluate( 19 ), Is.EqualTo( expectede ) );
        }

        [Test]
        public void Error___WhenEqual___IsZero()
        {
            // Arrange

            // Act
            var error = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 1, 2, 3 ), new Vector3Dbl( 4, 5, 6 ), new Vector3Dbl( 7, 8, 9 ), 10.0 ),
                new TrajectoryStateVector( new Vector3Dbl( 1, 2, 3 ), new Vector3Dbl( 4, 5, 6 ), new Vector3Dbl( 7, 8, 9 ), 10.0 )
            );
            var error2 = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 0 ),
                new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 0 )
            );

            // Assert
            Assert.That( error, Is.EqualTo( 0 ) );
            Assert.That( error2, Is.EqualTo( 0 ) );
        }
        [Test]
        public void Error___WhenSimilar___IsSmall()
        {
            // Arrange

            // Act
            var error = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 1, 1, 1 ), 1.0 ),
                new TrajectoryStateVector( new Vector3Dbl( 1.01, 1.01, 1.01 ), new Vector3Dbl( 1.01, 1.01, 1.01 ), new Vector3Dbl( 1.01, 1.01, 1.01 ), 1.01 )
            );

            // Assert
            Assert.That( error, Is.EqualTo( 0 ).Within( 0.01 ) );
        }
        [Test]
        public void Error___WhenOpposite___IsLarge()
        {
            // Arrange

            // Act
            var error = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 1, 1, 1 ), 1.0 ),
                new TrajectoryStateVector( new Vector3Dbl( -1, -1, -1 ), new Vector3Dbl( -1, -1, -1 ), new Vector3Dbl( -1, -1, -1 ), -1.0 )
            );

            // Assert
            Assert.That( error, Is.EqualTo( 1 ).Within( 1e-5 ) );
        }
        [Test]
        public void Error___WhenPerpendicular___IsMedium()
        {
            // Arrange

            // Act
            var error = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 1, 0, 0 ), new Vector3Dbl( 0, 1, 0 ), new Vector3Dbl( 0, 0, 1 ), 0 ),
                new TrajectoryStateVector( new Vector3Dbl( 0, 1, 0 ), new Vector3Dbl( 0, 0, 1 ), new Vector3Dbl( 1, 0, 0 ), 0 )
            );

            // Assert
            Assert.That( error, Is.EqualTo( 0.7071 ).Within( 1e-5 ) );
        }
        [Test]
        public void Error___WhenTwiceAsLong___IsMedium()
        {
            // Arrange

            // Act
            var error = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 1, 0, 0 ), new Vector3Dbl( 0, 1, 0 ), new Vector3Dbl( 0, 0, 1 ), 0 ),
                new TrajectoryStateVector( new Vector3Dbl( 2, 0, 0 ), new Vector3Dbl( 0, 2, 0 ), new Vector3Dbl( 0, 0, 2 ), 0 )
            );
            var error2 = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 10, 0, 0 ), new Vector3Dbl( 0, 10, 0 ), new Vector3Dbl( 0, 0, 10 ), 0 ),
                new TrajectoryStateVector( new Vector3Dbl( 20, 0, 0 ), new Vector3Dbl( 0, 20, 0 ), new Vector3Dbl( 0, 0, 20 ), 0 )
            );

            // Assert
            Assert.That( error, Is.EqualTo( 0.33333333333 ).Within( 1e-5 ) );
            Assert.That( error, Is.EqualTo( error2 ).Within( 1e-5 ) );
        }
        [Test]
        public void Error___IndependentOfMagnitude()
        {
            // Arrange

            // Act
            var error4 = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 0.0 ),
                new TrajectoryStateVector( new Vector3Dbl( 1e12, 1e12, 1e12 ), new Vector3Dbl( 1e12, 1e12, 1e12 ), new Vector3Dbl( 1e12, 1e12, 1e12 ), 1e12 )
            );
            var error3 = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 0.0 ),
                new TrajectoryStateVector( new Vector3Dbl( 10000, 10000, 10000 ), new Vector3Dbl( 10000, 10000, 10000 ), new Vector3Dbl( 10000, 10000, 10000 ), 10000.0 )
            );
            var error2 = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 0.0 ),
                new TrajectoryStateVector( new Vector3Dbl( 10, 10, 10 ), new Vector3Dbl( 10, 10, 10 ), new Vector3Dbl( 10, 10, 10 ), 10.0 )
            );
            var error = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 0.0 ),
                new TrajectoryStateVector( new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 1, 1, 1 ), 1.0 )
            );

            // Assert
            Assert.That( error, Is.EqualTo( error2 ).Within( 0.001 ) );
            Assert.That( error, Is.EqualTo( error3 ).Within( 0.001 ) );
            Assert.That( error, Is.EqualTo( error4 ).Within( 0.001 ) );
        }
        [Test]
        public void Error___IndependentOfMagnitude_Translated()
        {
            // Arrange

            // Act
            var error2 = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 5, 5, 5 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 1, 1, 1 ), 1.0 ),
                new TrajectoryStateVector( new Vector3Dbl( 5, 5, 5 ), new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 1, 1, 1 ), 1.0 )
            );
            var error = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 555555555, 555555555, 555555555 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 1, 1, 1 ), 1.0 ),
                new TrajectoryStateVector( new Vector3Dbl( 555555555, 555555555, 555555555 ), new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 1, 1, 1 ), 1.0 )
            );

            // Assert
            Assert.That( error, Is.EqualTo( error2 ) );
        }

        [Test]
        public void Error___Tests()
        {
            // Arrange

            // Act
            var error1 = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 0.0 ),
                new TrajectoryStateVector( new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 1, 1, 1 ), 1000.0 )
            );
            Debug.Log( "Error1: " + error1 );
            var error2 = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 0.0 ),
                new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 0.0 )
            );
            Debug.Log( "Error2: " + error2 );
            var error3 = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 1, 2, 3 ), new Vector3Dbl( 4, 5, 6 ), new Vector3Dbl( 7, 8, 9 ), 10.0 ),
                new TrajectoryStateVector( new Vector3Dbl( 1, 2, 3 ), new Vector3Dbl( 4, 5, 6 ), new Vector3Dbl( 7, 8, 9 ), 10.0 )
            );
            Debug.Log( "Error3: " + error3 );
            var error4 = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 1, 1, 1 ), 1.0 ),
                new TrajectoryStateVector( new Vector3Dbl( 1.01, 1.01, 1.01 ), new Vector3Dbl( 1.01, 1.01, 1.01 ), new Vector3Dbl( 1.01, 1.01, 1.01 ), 1.01 )
            );
            Debug.Log( "Error4: " + error4 );
            var error5 = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 1, 1, 1 ), new Vector3Dbl( 1, 1, 1 ), 1.0 ),
                new TrajectoryStateVector( new Vector3Dbl( -1, -1, -1 ), new Vector3Dbl( -1, -1, -1 ), new Vector3Dbl( -1, -1, -1 ), -1.0 )
            );
            Debug.Log( "Error5: " + error5 );
            var error6 = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), new Vector3Dbl( 0, 0, 0 ), 0.0 ),
                new TrajectoryStateVector( new Vector3Dbl( 10, 10, 10 ), new Vector3Dbl( 10, 10, 10 ), new Vector3Dbl( 10, 10, 10 ), 10.0 )
            );
            Debug.Log( "Error6: " + error6 );
            var error7 = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 949999999985.37, 3074493.32, 0.00 ), new Vector3Dbl( -0.05, 9749.15, 0.00 ), new Vector3Dbl( 0, 0, 0 ), 5.96999991261919E+24 ),
                new TrajectoryStateVector( new Vector3Dbl( 876902719663.86, 299443237356.88, 51040.40 ), new Vector3Dbl( -4632.00, 8980.11, 0.00 ), new Vector3Dbl( 0, 0, 0 ), 5.96999991261919E+24 )
            );
            error7 = Ephemeris2.CalculateError(
                new TrajectoryStateVector( new Vector3Dbl( 949999999985.37, 3074493.32, 0.00 ), new Vector3Dbl( -0.05, 9749.15, 0.00 ), new Vector3Dbl( 0, 0, 0 ), 5.96999991261919E+24 ),
                new TrajectoryStateVector( new Vector3Dbl( 896902719663.86, 199443237356.88, 51040.40 ), new Vector3Dbl( -1632.00, 9180.11, 0.00 ), new Vector3Dbl( 0, 0, 0 ), 5.96999991261919E+24 )
            );
            Debug.Log( "Error7: " + error7 );

            // Assert
            Assert.That( error1, Is.EqualTo( error2 ) );
        }
    }
}