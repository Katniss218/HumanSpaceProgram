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
#warning TODO - split contents into gas and liquid contents? I think it would be better to just filter over the main contents array, the length is small (mostly 2-3 subs)
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
            // "cache" refers to the data that will change without changing the tank geometry.
            if( _slices == null || force )
            {
                BakePotentialSlices();
                _fluidInSlices = null; // Geometry changed, so fluid distribution doesn't match the volume distribution by potential.
            }

            if( _fluidInSlices == null || force )
            {
                DistributeFluidsToPotentials();
            }
        }

        /// <summary>
        /// Rebuilds the cache of volume per each interval between sorted potential values.
        /// </summary>
        private void BakePotentialSlices()
        {
            if( _owner._nodes == null || _owner._edges == null )
                return;

            // 1. Calculate and Sort Potentials
            int n = _owner._nodes.Length;
            _nodePotentials = new double[n];
            List<double> distinct = new( n );

            for( int i = 0; i < n; i++ )
            {
                _nodePotentials[i] = GetPotentialAt( _owner._nodes[i].pos );
                distinct.Add( _nodePotentials[i] );
            }

            distinct.Sort();
            // Dedupe
            int write = 0;
            for( int i = 0; i < distinct.Count; i++ )
            {
                if( write == 0 || (distinct[i] - distinct[write - 1] > EPSILON_DEDUPE_POTENTIALS) )
                    distinct[write++] = distinct[i];
            }
            distinct.RemoveRange( write, distinct.Count - write );

            // 2. Init Slices
            int sliceCount = Math.Max( 1, distinct.Count - 1 ); // max 1 => no distinct breakpoints (no potential gradient), single slice.
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

            // 3. Add Edge volumes to corresponding potential slices via overlap.
            for( int ei = 0; ei < _owner._edges.Length; ei++ )
            {
                ref FlowEdge edge = ref _owner._edges[ei];
                double p1 = _nodePotentials[edge.end1];
                double p2 = _nodePotentials[edge.end2];
                Vector3 pos1 = _owner._nodes[edge.end1].pos;
                Vector3 pos2 = _owner._nodes[edge.end2].pos;

                // Ensure p1 < p2
                if( p1 > p2 )
                {
                    (p1, p2) = (p2, p1);
                    (pos1, pos2) = (pos2, pos1);
                }

                double potDiff = p2 - p1;

                // --- PERPENDICULAR EDGE ---
                if( potDiff <= EPSILON_OVERLAP )
                {
                    // Find the exact potential boundary this edge sits on
                    int k = FindClosestIndex( distinct, p1 );

                    // If found, split volume between slice below (k-1) and slice above (k)
                    if( k >= 0 )
                    {
                        Vector3 center = (pos1 + pos2) * 0.5f;

                        bool hasBottom = (k - 1 >= 0 && k - 1 < sliceCount);
                        bool hasTop = (k < sliceCount);

                        if( hasBottom && hasTop )
                        {
                            // Split 50/50
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

                // --- STANDARD SLOPED EDGE ---
                double invDiff = 1.0 / potDiff;
                int startIdx = FindIntervalIndex( distinct, p1 ); // Use helper
                if( startIdx < 0 ) startIdx = 0;

                for( int s = startIdx; s < sliceCount; s++ )
                {
                    ref PotentialSlice slice = ref _slices[s];
                    if( slice.PotentialBottom >= p2 )
                        break;
                    if( slice.PotentialTop <= p1 )
                        continue;

                    // Calc Overlap
                    double oMin = Math.Max( p1, slice.PotentialBottom );
                    double oMax = Math.Min( p2, slice.PotentialTop );
                    double overlap = oMax - oMin;

                    if( overlap > EPSILON_OVERLAP )
                    {
                        double frac = overlap * invDiff;
                        double segVol = edge.Volume * frac;

                        // Centroid of the segment
                        float t1 = (float)((oMin - p1) * invDiff);
                        float t2 = (float)((oMax - p1) * invDiff);
                        Vector3 segCenter = Vector3.Lerp( pos1, pos2, (t1 + t2) * 0.5f );

                        slice.AddVolume( segVol, segCenter );
                    }
                }
            }

            // 4. Finalize
            for( int i = 0; i < sliceCount; i++ )
            {
                _slices[i].FinalizeCentroid();
            }
        }

        /// <summary>
        /// Separates liquids into potential slices and calculates total gas pressure for the ullage.
        /// </summary>
        private void DistributeFluidsToPotentials()
        {
            if( _slices == null )
            {
                throw new InvalidOperationException( $"Tank was not sliced. Can't distribute fluids." );
            }
            int fluidCount = _owner.Contents.Count;

            // 1. Separate Liquids and Gases
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

            // 2. Process Liquids (Standard logic)
            // Sort descending by density.
            _sortBuffer.Sort( ( a, b ) => b.density.CompareTo( a.density ) );

            int liquidCount = _sortBuffer.Count;
            if( _fluidInSlices == null || _fluidInSlices.Length != liquidCount )
                _fluidInSlices = new FluidInSlice[liquidCount];

            // Total Tank Volume
            double totalTankVolume = 0;
            for( int i = 0; i < _slices.Length; i++ )
                totalTankVolume += _slices[i].VolumeCapacity;

            // Calculate Liquid Volumes
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

            // 3. Process Gases (New Logic)
            // Ullage is the empty space. Clamp to small epsilon to avoid divide-by-zero if tank is full.
            _ullageVolume = Math.Max( 1e-6, totalTankVolume - totalLiquidVolume );

            _internalGasPressure = 0.0;
            if( _gasBuffer.Count > 0 )
            {
                // Dalton's Law: Sum of partial pressures
                foreach( var (sub, mass) in _gasBuffer )
                {
                    double gasDensity = mass / _ullageVolume;
                    _internalGasPressure += sub.GetPressure( _owner.FluidState.Temperature, gasDensity );
                }
            }

            // 4. Handle Hydraulic Lock (Liquid > Tank Volume)
            // If liquids overflow, pressure spikes massively. 
            // We apply a scaling factor to the liquid slices for geometry, but logically the pressure is immense.
            double scale = 1.0;
            if( totalLiquidVolume > totalTankVolume && totalTankVolume > 0 )
            {
                scale = totalTankVolume / totalLiquidVolume;
#warning TODO - handle with substance's actual bulk modulus.
                double overflowRatio = totalLiquidVolume / totalTankVolume;
                _internalGasPressure += (overflowRatio - 1.0) * 100_000_000; // 100MPa per 100% overfill
            }

            // 5. Map Liquid Layers to Potentials (Existing logic, but using liquidCount)
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

        /// <summary>
        /// Calculates the scalar potential energy at a point. <br/>
        /// Fluids effectively "fall" from high to low potential.
        /// </summary>
        /// <remarks>
        /// Note: Standard physics defines Force = -Gradient(Potential). <br/>
        /// If FluidAcceleration is "Gravity" (pointing down), Potential = -g.y (increases going up).
        /// </remarks>
        /// <param name="localPosition">The position, in tank-space.</param>
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

                // advance sliceIdx until we reach any overlap
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

            // 1. Geometric Potential (Gravity + Centrifugal) in J/kg
            double geoPotential = GetPotentialAt( localPosition );

            // 2. Identify Local Properties
            double localPressure = _internalGasPressure;

            // Top of liquid stack
            double liquidSurfacePot = (_fluidInSlices != null && _fluidInSlices.Length > 0)
                ? _fluidInSlices[^1].PotentialEnd
                : double.MinValue;

            bool isSampleInLiquid = geoPotential < liquidSurfacePot;

            if( isSampleInLiquid )
            {
                // --- LIQUID PHASE ---
                double hydrostaticPressure = 0;

                for( int i = _fluidInSlices.Length - 1; i >= 0; i-- )
                {
                    ref FluidInSlice layer = ref _fluidInSlices[i];
                    if( geoPotential < layer.PotentialStart )
                    {
                        hydrostaticPressure += layer.Density * (layer.PotentialEnd - layer.PotentialStart);
                    }
                    else if( geoPotential < layer.PotentialEnd )
                    {
                        hydrostaticPressure += layer.Density * (layer.PotentialEnd - geoPotential);
                        break;
                    }
                }
                localPressure += hydrostaticPressure;

                // For a liquid, the effective potential driving flow out of any submerged
                // orifice is the potential of the free surface.
                result.FluidSurfacePotential = liquidSurfacePot;
            }
            else
            {
                // --- GAS PHASE ---
                // In gas, pressure potential matters for equalization. The reference density
                // acts as a consistent scaling factor for pressure head.
                const double POTENTIAL_REFERENCE_DENSITY = 1000.0;
                result.FluidSurfacePotential = geoPotential + (localPressure / POTENTIAL_REFERENCE_DENSITY);
            }

            result.Pressure = localPressure;

            return result;
        }

        public ISampledSubstanceStateCollection SampleSubstances( Vector3 localPosition, double flowRate, double dt )
        {
            RecalculateCache();

            var result = PooledReadonlySubstanceStateCollection.Get();

            if( _owner.Contents == null || _owner.Contents.IsEmpty() )
                return result;

            double p = GetPotentialAt( localPosition );

            // Determine if we are hitting liquid or gas
            // Liquids are stored in _fluidInSlices.
            double liquidSurfacePot = (_fluidInSlices != null && _fluidInSlices.Length > 0)
                ? _fluidInSlices[^1].PotentialEnd
                : double.MinValue;

            bool isSubmerged = p < liquidSurfacePot;

            if( isSubmerged )
            {
                // --- LIQUID EXTRACTION ---
                double pClamped = Math.Clamp( p, _slices[0].PotentialBottom, _slices[^1].PotentialTop );
                int idx = FindSliceIndexForPotential( _fluidInSlices, pClamped );

                if( idx >= 0 )
                {
                    ref FluidInSlice layer = ref _fluidInSlices[idx];
                    double requestedMass = flowRate * dt * layer.Density;

                    // Simple availability check (without the transient fix for now)
                    double availableMass = layer.Volume * layer.Density;

                    result.Add( layer.Substance, Math.Min( requestedMass, availableMass ) );
                }
            }
            else
            {
                // --- GAS EXTRACTION ---
                // If we are in the gas, we extract a mixture of all gases proportional to their mass
                if( _gasBuffer.Count == 0 )
                    return result;

                double totalGasMass = 0;
                foreach( var pair in _gasBuffer ) totalGasMass += pair.mass;

                if( totalGasMass > 1e-9 )
                {
                    double avgGasDensity = totalGasMass / _ullageVolume;
                    double requestedTotalMass = flowRate * dt * avgGasDensity;

                    // Scale down if requesting more than available
                    double scale = 1.0;
                    if( requestedTotalMass > totalGasMass )
                        scale = totalGasMass / requestedTotalMass;

                    foreach( var (sub, mass) in _gasBuffer )
                    {
                        double extractMass = (mass / totalGasMass) * requestedTotalMass * scale;
                        result.Add( sub, extractMass );
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Finds the index i such that sortedList[i] is within tolerance of value. 
        /// Returns -1 if no match found.
        /// </summary>
        private static int FindClosestIndex( IList<double> sortedList, double value )
        {
            const double TOLERANCE = EPSILON_DEDUPE_POTENTIALS * 1.5;

            // If found, returns index. If not, returns bitwise complement of next largest element.
            int idx = (sortedList is List<double> list)
                ? list.BinarySearch( value )
                : -1; // Fallback if not List<T>, strictly not needed if we only use List

            if( idx >= 0 )
                return idx;

            int nextIdx = ~idx;

            // Check neighbors (nextIdx and nextIdx - 1) for tolerance match
            double diffNext = (nextIdx < sortedList.Count)
                ? Math.Abs( sortedList[nextIdx] - value )
                : double.MaxValue;
            double diffPrev = (nextIdx > 0)
                ? Math.Abs( sortedList[nextIdx - 1] - value )
                : double.MaxValue;

            if( diffNext < diffPrev && diffNext <= TOLERANCE )
                return nextIdx;
            if( diffPrev <= diffNext && diffPrev <= TOLERANCE )
                return nextIdx - 1;

            return -1;
        }

        /// <summary>
        /// Finds index i such that list[i] <= val <= list[i+1].
        /// </summary>
        private static int FindIntervalIndex( IList<double> sortedList, double val )
        {
            int n = sortedList.Count;
            if( n < 2 )
                return -1;

            // Bounds check
            if( val < sortedList[0] - EPSILON_OVERLAP || val > sortedList[n - 1] + EPSILON_OVERLAP )
                return -1;

            int lo = 0, hi = n - 2;
            while( lo <= hi )
            {
                int mid = lo + (hi - lo) / 2;
                if( val < sortedList[mid] - EPSILON_OVERLAP )
                    hi = mid - 1;
                else if( val > sortedList[mid + 1] + EPSILON_OVERLAP )
                    lo = mid + 1;
                else return mid;
            }
            return -1;
        }

        /// <summary>
        /// optimized binary search directly on the struct array to avoid allocations.
        /// </summary>
        private static int FindSliceIndexForPotential( FluidInSlice[] slices, double potential )
        {
            int low = 0;
            int high = slices.Length - 1;

            while( low <= high )
            {
                int mid = low + (high - low) / 2;
                ref FluidInSlice slice = ref slices[mid];

                if( potential < slice.PotentialStart - EPSILON_OVERLAP )
                    high = mid - 1;
                else if( potential > slice.PotentialEnd + EPSILON_OVERLAP )
                    low = mid + 1;
                else return mid;
            }

            return -1;
        }

        private static double Lerp( double a, double b, double t )
        {
            return a + (b - a) * t;
        }
    }
}
