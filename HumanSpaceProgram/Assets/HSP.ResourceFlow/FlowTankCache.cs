using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.ResourceFlow
{
    internal class FlowTankCache
    {
        private readonly FlowTank _owner;

        // --- Cache Data ---

        /// <summary>
        /// Represents a slice of the tank at a specific potential interval.
        /// Acts as the "Container Geometry" cache.
        /// </summary>
        private struct PotentialSlice
        {
            public double PotentialBottom;
            public double PotentialTop;

            /// <summary>
            /// The total geometric volume (m^3) this slice can hold.
            /// </summary>
            public double VolumeCapacity;

            /// <summary>
            /// The Geometric Moment (Volume * Centroid) of the FULL slice.
            /// Used to calculate Center of Mass.
            /// </summary>
            public Vector3 GeometricMoment;

            /// <summary>
            /// The centroid of the full slice (Pre-calculated for interpolation).
            /// </summary>
            public Vector3 Centroid;

            public void AddVolume( double volume, Vector3 center )
            {
                VolumeCapacity += volume;
                GeometricMoment += center * (float)volume;
            }

            public void FinalizeCentroid()
            {
                if( VolumeCapacity > EPSILON_OVERLAP )
                    Centroid = GeometricMoment / (float)VolumeCapacity;
            }
        }

        /// <summary>
        /// Represents a specific fluid occupying a specific range of potential.
        /// Acts as the "Content State".
        /// </summary>
        private struct FluidInSlice
        {
            public ISubstance Substance;
            public double Density;
            public double Volume;

            // The calculated potential boundaries for this specific fluid
            public double PotentialStart;
            public double PotentialEnd;
        }

        internal const double EPSILON_OVERLAP = 1e-9;
        const double EPSILON_DEDUPE_POTENTIALS = 0.1;

        private double[] _nodePotentials; // Cached potentials per node.
        private PotentialSlice[] _slices; // The baked geometry (Null = Dirty)

        private FluidInSlice[] _fluidInSlices; // The calculated fluid stack (Null = Dirty)
        private readonly List<(ISubstance sub, double mass, double density)> _sortBuffer = new();

        private double _internalGasPressure;
        private double _ullageVolume;
        private readonly List<(ISubstance sub, double mass)> _gasBuffer = new(); // Temp buffer for gas calc

        public FlowTankCache( FlowTank owner )
        {
            _owner = owner;
        }

        public void InvalidateGeometryAndFluids()
        {
            _slices = null;
            _nodePotentials = null;
            InvalidateFluids(); // If geometry changes, fluid levels definitely change
        }

        public void InvalidateFluids()
        {
            _fluidInSlices = null;
        }

        public void RecalculateCache( bool force = false )
        {
            if( _slices == null || force )
            {
                BakePotentialSlices();
                _fluidInSlices = null;
            }

            if( _fluidInSlices == null || force )
            {
                DistributeFluidsToPotentials();
            }
        }

        private void BakePotentialSlices()
        {
            if( _owner._nodes == null || _owner._edges == null )
                return;

            int n = _owner._nodes.Length;
            _nodePotentials = new double[n];
            List<double> distinct = new( n );

            for( int i = 0; i < n; i++ )
            {
                _nodePotentials[i] = GetPotentialAt( _owner._nodes[i].pos );
                distinct.Add( _nodePotentials[i] );
            }

            distinct.Sort();
            int write = 0;
            for( int i = 0; i < distinct.Count; i++ )
            {
                if( write == 0 || (distinct[i] - distinct[write - 1] > EPSILON_DEDUPE_POTENTIALS) )
                    distinct[write++] = distinct[i];
            }
            distinct.RemoveRange( write, distinct.Count - write );

            int sliceCount = Math.Max( 1, distinct.Count - 1 );
            _slices = new PotentialSlice[sliceCount];
            if( sliceCount == 1 )
            {
                _slices[0].PotentialBottom = double.NegativeInfinity;
                _slices[0].PotentialTop = double.PositiveInfinity;
            }
            else
            {
                for( int i = 0; i < sliceCount; i++ )
                {
                    _slices[i].PotentialBottom = distinct[i];
                    _slices[i].PotentialTop = distinct[i + 1];
                }
            }

            if( sliceCount == 0 )
                return;

            for( int ei = 0; ei < _owner._edges.Length; ei++ )
            {
                ref FlowEdge edge = ref _owner._edges[ei];
                double p1 = _nodePotentials[edge.end1];
                double p2 = _nodePotentials[edge.end2];
                Vector3 pos1 = _owner._nodes[edge.end1].pos;
                Vector3 pos2 = _owner._nodes[edge.end2].pos;

                if( p1 > p2 )
                {
                    (p1, p2) = (p2, p1);
                    (pos1, pos2) = (pos2, pos1);
                }

                double potDiff = p2 - p1;

                if( potDiff <= EPSILON_OVERLAP )
                {
                    int k = FindClosestIndex( distinct, p1 );
                    if( k >= 0 )
                    {
                        Vector3 center = (pos1 + pos2) * 0.5f;

                        bool hasBottom = (k - 1 >= 0 && k - 1 < sliceCount);
                        bool hasTop = (k < sliceCount);

                        if( hasBottom && hasTop )
                        {
                            double halfVol = edge.Volume * 0.5;
                            _slices[k - 1].AddVolume( halfVol, center );
                            _slices[k].AddVolume( halfVol, center );
                        }
                        else if( hasBottom )
                        {
                            _slices[k - 1].AddVolume( edge.Volume, center );
                        }
                        else if( hasTop )
                        {
                            _slices[k].AddVolume( edge.Volume, center );
                        }
                    }
                    continue;
                }

                double invDiff = 1.0 / potDiff;
                int startIdx = FindIntervalIndex( distinct, p1 );
                if( startIdx < 0 ) startIdx = 0;

                for( int s = startIdx; s < sliceCount; s++ )
                {
                    ref PotentialSlice slice = ref _slices[s];
                    if( slice.PotentialBottom >= p2 )
                        break;
                    if( slice.PotentialTop <= p1 )
                        continue;

                    double oMin = Math.Max( p1, slice.PotentialBottom );
                    double oMax = Math.Min( p2, slice.PotentialTop );
                    double overlap = oMax - oMin;

                    if( overlap > EPSILON_OVERLAP )
                    {
                        double frac = overlap * invDiff;
                        double segVol = edge.Volume * frac;

                        float t1 = (float)((oMin - p1) * invDiff);
                        float t2 = (float)((oMax - p1) * invDiff);
                        Vector3 segCenter = Vector3.Lerp( pos1, pos2, (t1 + t2) * 0.5f );

                        slice.AddVolume( segVol, segCenter );
                    }
                }
            }

            for( int i = 0; i < sliceCount; i++ )
            {
                _slices[i].FinalizeCentroid();
            }
        }

        private void DistributeFluidsToPotentials()
        {
            if( _slices == null )
            {
                throw new InvalidOperationException( $"Tank was not sliced. Can't distribute fluids." );
            }
            int fluidCount = _owner.Contents.Count;

            _sortBuffer.Clear();
            _gasBuffer.Clear();

            foreach( (ISubstance substance, double mass) in _owner.Contents )
            {
                if( substance.Phase == SubstancePhase.Gas )
                {
                    _gasBuffer.Add( (substance, mass) );
                }
                else
                {
                    double density = substance.GetDensity( _owner.FluidState.Temperature, _owner.FluidState.Pressure );
                    _sortBuffer.Add( (substance, mass, density) );
                }
            }

            _sortBuffer.Sort( ( a, b ) => b.density.CompareTo( a.density ) );

            int liquidCount = _sortBuffer.Count;
            if( _fluidInSlices == null || _fluidInSlices.Length != liquidCount )
                _fluidInSlices = new FluidInSlice[liquidCount];

            double totalTankVolume = 0;
            for( int i = 0; i < _slices.Length; i++ )
                totalTankVolume += _slices[i].VolumeCapacity;

            double totalLiquidVolume = 0;
            for( int i = 0; i < liquidCount; i++ )
            {
                double vol = _sortBuffer[i].mass / _sortBuffer[i].density;
                totalLiquidVolume += vol;
                _fluidInSlices[i] = new FluidInSlice()
                {
                    Substance = _sortBuffer[i].sub,
                    Density = _sortBuffer[i].density,
                    Volume = vol
                };
            }

            _ullageVolume = Math.Max( 1e-6, totalTankVolume - totalLiquidVolume );

            _internalGasPressure = 0.0;
            if( _gasBuffer.Count > 0 )
            {
                foreach( var (sub, mass) in _gasBuffer )
                {
                    double gasDensity = mass / _ullageVolume;
                    _internalGasPressure += sub.GetPressure( _owner.FluidState.Temperature, gasDensity );
                }
            }

            double scale = 1.0;
            if( totalLiquidVolume > totalTankVolume && totalTankVolume > 0 )
            {
                scale = totalTankVolume / totalLiquidVolume;
                double overflowRatio = totalLiquidVolume / totalTankVolume;
                _internalGasPressure += (overflowRatio - 1.0) * 100_000_000;
            }

            double currentVolumeConsumed = 0;
            int sliceWalkerIdx = 0;
            double volumeInPreviousSlices = 0;

            for( int i = 0; i < liquidCount; i++ )
            {
                ref FluidInSlice layer = ref _fluidInSlices[i];
                layer.Volume *= scale;

                layer.PotentialStart = (i == 0)
                    ? _slices[0].PotentialBottom
                    : _fluidInSlices[i - 1].PotentialEnd;

                double volumeTarget = currentVolumeConsumed + layer.Volume;

                while( sliceWalkerIdx < _slices.Length )
                {
                    double sliceCap = _slices[sliceWalkerIdx].VolumeCapacity;
                    double totalAtSliceTop = volumeInPreviousSlices + sliceCap;

                    if( volumeTarget <= totalAtSliceTop )
                    {
                        double volInSlice = volumeTarget - volumeInPreviousSlices;
                        double fraction = (sliceCap > 1e-9) ? (volInSlice / sliceCap) : 1.0;
                        layer.PotentialEnd = Lerp( _slices[sliceWalkerIdx].PotentialBottom, _slices[sliceWalkerIdx].PotentialTop, fraction );
                        break;
                    }
                    else
                    {
                        volumeInPreviousSlices += sliceCap;
                        sliceWalkerIdx++;
                    }
                }

                if( sliceWalkerIdx >= _slices.Length )
                    layer.PotentialEnd = _slices[^1].PotentialTop;

                currentVolumeConsumed += layer.Volume;
            }
        }

        public double GetPotentialAt( Vector3 localPosition )
        {
            double linearContrib = 0.0;
            double rotationalContrib = 0.0;

            if( _owner.FluidAcceleration.sqrMagnitude > 0.001 )
            {
                linearContrib = -Vector3.Dot( _owner.FluidAcceleration, localPosition );
            }
            if( _owner.FluidAngularVelocity.sqrMagnitude > 0.001 )
            {
                Vector3 tang = Vector3.Cross( _owner.FluidAngularVelocity, localPosition );
                rotationalContrib = -0.5 * tang.sqrMagnitude;
            }

            return linearContrib + rotationalContrib;
        }

        public Vector3 GetCenterOfMass()
        {
            RecalculateCache();

            if( _fluidInSlices == null || _fluidInSlices.Length == 0 )
                return Vector3.zero;

            Vector3 totalMoment = Vector3.zero;
            double totalMass = 0.0;

            int sliceIdx = 0;
            for( int f = 0; f < _fluidInSlices.Length; f++ )
            {
                ref FluidInSlice layer = ref _fluidInSlices[f];
                double fStart = layer.PotentialStart;
                double fEnd = layer.PotentialEnd;
                double density = layer.Density;

                while( sliceIdx < _slices.Length && _slices[sliceIdx].PotentialTop <= fStart ) sliceIdx++;

                for( int s = sliceIdx; s < _slices.Length; s++ )
                {
                    ref PotentialSlice slice = ref _slices[s];
                    if( slice.PotentialBottom >= fEnd )
                        break;

                    double intersectStart = Math.Max( fStart, slice.PotentialBottom );
                    double intersectEnd = Math.Min( fEnd, slice.PotentialTop );
                    double span = intersectEnd - intersectStart;
                    if( span <= EPSILON_OVERLAP )
                        continue;

                    double sliceSpan = slice.PotentialTop - slice.PotentialBottom;
                    double fillRatio = (sliceSpan > EPSILON_OVERLAP) ? (span / sliceSpan) : 1.0;
                    double volInOverlap = slice.VolumeCapacity * fillRatio;
                    double mass = volInOverlap * density;

                    totalMoment += slice.Centroid * (float)mass;
                    totalMass += mass;
                }
            }

            return (totalMass > 1e-9) ? totalMoment / (float)totalMass : Vector3.zero;
        }

        public FluidState Sample( Vector3 localPosition, double holeArea )
        {
            RecalculateCache();

            FluidState result = _owner.FluidState;

            double geoPotential = GetPotentialAt( localPosition );
            double localPressure = _internalGasPressure;

            double liquidSurfacePot = (_fluidInSlices != null && _fluidInSlices.Length > 0)
                ? _fluidInSlices[^1].PotentialEnd
                : double.MinValue;

            bool isSampleInLiquid = geoPotential < liquidSurfacePot;

            if( isSampleInLiquid )
            {
                double hydrostaticPressure = 0;
                double liquidDensityAtPort = 0;

                for( int i = _fluidInSlices.Length - 1; i >= 0; i-- )
                {
                    ref FluidInSlice layer = ref _fluidInSlices[i];
                    if( geoPotential < layer.PotentialStart )
                    {
                        hydrostaticPressure += layer.Density * (layer.PotentialEnd - layer.PotentialStart);
                    }
                    else if( geoPotential < layer.PotentialEnd )
                    {
                        liquidDensityAtPort = layer.Density;
                        hydrostaticPressure += layer.Density * (layer.PotentialEnd - geoPotential);
                        break;
                    }
                }
                localPressure += hydrostaticPressure;

                double pressurePotential = 0;
                if( liquidDensityAtPort > 1e-9 )
                {
                    pressurePotential = _internalGasPressure / liquidDensityAtPort;
                }

                result.FluidSurfacePotential = liquidSurfacePot + pressurePotential;
            }
            else
            {
                double totalGasMass = 0;
                double weightedRsSum = 0;
                if( _gasBuffer.Count > 0 )
                {
                    foreach( var (sub, mass) in _gasBuffer )
                    {
                        totalGasMass += mass;
                        weightedRsSum += mass * sub.SpecificGasConstant;
                    }
                }

                double pressurePotential = 0;
                if( totalGasMass > 1e-9 )
                {
                    double averageRs = weightedRsSum / totalGasMass;
                    double temperature = _owner.FluidState.Temperature;
                    const double P_REF = 101325.0;
                    double pressureClamped = Math.Max( localPressure, 1e-5 );
                    pressurePotential = averageRs * temperature * Math.Log( pressureClamped / P_REF );
                }

                result.FluidSurfacePotential = geoPotential + pressurePotential;
            }

            result.Pressure = localPressure;

            return result;
        }

        public ISampledSubstanceStateCollection SampleSubstances( Vector3 localPosition, double mass )
        {
            RecalculateCache();

            var result = PooledReadonlySubstanceStateCollection.Get();
            if( _owner.Contents == null || _owner.Contents.IsEmpty() || mass <= 1e-9 )
                return result;

            double p = GetPotentialAt( localPosition );

#warning TODO - _fluidInSlices[^1].PotentialEnd was NaN
            double liquidSurfacePot = (_fluidInSlices != null && _fluidInSlices.Length > 0)
                ? _fluidInSlices[^1].PotentialEnd
                : double.MinValue;

            bool isSubmerged = p < liquidSurfacePot;

            if( isSubmerged )
            {
                double pClamped = Math.Clamp( p, _slices[0].PotentialBottom, _slices[^1].PotentialTop );
                int idx = FindSliceIndexForPotential( _fluidInSlices, pClamped );

                if( idx >= 0 )
                {
                    ref FluidInSlice layer = ref _fluidInSlices[idx];
                    double availableMass = layer.Volume * layer.Density;
                    result.Add( layer.Substance, Math.Min( mass, availableMass ) );
                }
            }
            else
            {
                if( _gasBuffer.Count == 0 )
                    return result;

                double totalGasMass = 0;
                foreach( var pair in _gasBuffer ) totalGasMass += pair.mass;

                if( totalGasMass > 1e-9 )
                {
                    double scale = Math.Min( 1.0, mass / totalGasMass );
                    foreach( var (sub, subMass) in _gasBuffer )
                    {
                        result.Add( sub, subMass * scale );
                    }
                }
            }

            return result;
        }

        private static int FindClosestIndex( IList<double> sortedList, double value )
        {
            const double TOLERANCE = EPSILON_DEDUPE_POTENTIALS * 1.5;

            int idx = (sortedList is List<double> list) ? list.BinarySearch( value ) : -1;

            if( idx >= 0 ) return idx;
            int nextIdx = ~idx;

            double diffNext = (nextIdx < sortedList.Count) ? Math.Abs( sortedList[nextIdx] - value ) : double.MaxValue;
            double diffPrev = (nextIdx > 0) ? Math.Abs( sortedList[nextIdx - 1] - value ) : double.MaxValue;

            if( diffNext < diffPrev && diffNext <= TOLERANCE ) return nextIdx;
            if( diffPrev <= diffNext && diffPrev <= TOLERANCE ) return nextIdx - 1;

            return -1;
        }

        private static int FindIntervalIndex( IList<double> sortedList, double val )
        {
            int n = sortedList.Count;
            if( n < 2 || val < sortedList[0] - EPSILON_OVERLAP || val > sortedList[n - 1] + EPSILON_OVERLAP )
                return -1;

            int lo = 0, hi = n - 2;
            while( lo <= hi )
            {
                int mid = lo + (hi - lo) / 2;
                if( val < sortedList[mid] - EPSILON_OVERLAP ) hi = mid - 1;
                else if( val > sortedList[mid + 1] + EPSILON_OVERLAP ) lo = mid + 1;
                else return mid;
            }
            return -1;
        }

        private static int FindSliceIndexForPotential( FluidInSlice[] slices, double potential )
        {
            int low = 0;
            int high = slices.Length - 1;

            while( low <= high )
            {
                int mid = low + (high - low) / 2;
                ref FluidInSlice slice = ref slices[mid];

                if( potential < slice.PotentialStart - EPSILON_OVERLAP ) high = mid - 1;
                else if( potential > slice.PotentialEnd + EPSILON_OVERLAP ) low = mid + 1;
                else return mid;
            }

            return -1;
        }

        private static double Lerp( double a, double b, double t ) => a + (b - a) * t;
    }
}
