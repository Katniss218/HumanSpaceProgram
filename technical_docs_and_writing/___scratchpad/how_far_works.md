# Ferram Aerospace Research

dkavolis et al 2023

Wings use lifting line theory, body lift is handled by first voxelizing the vessel and then computing some shape properties. There's some hard coded handling for supersonic flows, most fluid properties use ideal gas relationships accounting for Mach number 
Water handling is buggy, it uses lerp between air and water aero properties based on submerged portion. Water is treated as if it was heavy and viscous air
For body lift, parts are also grouped into sections for better performance but I don't fully know how it works

The voxelization code is tripled in multiple places for each dominant cardinal direction but it could be simplified with generics in a way that also works with Burst
Voxelization algorithm is pretty efficient otherwise

Body forces are computed from differential shape properties but I don't know the method behind it
Voxelization is computed by iterating over every triangle so the savings add up. Some shapes are approximated with simpler ones that have fewer triangles. This produces a shell which is then filled

Filling is done from the front and back in parallel by tracking enter/exit from the shell, assuming front and back start outside
It also computes cross section properties in the same pass

Back and front are outside the shell, i.e. voxels are empty. Then you track whether you are inside the shell or not by transitions through the shell. Sort of a raycast algorithm through axis aligned grid

It [the VoxelCrossSection struct] is a cache to make physics computations faster, part side areas are each part area projections on an axis aligned cube

This one [PartSizePair struct] is for part and voxel size pairs, voxels are not just full or empty but they allow setting fractional fullness in each direction
So you can recover most of the precision lost due to discretization

FARAeroComponents deals with voxelization physics

[A "Sweep Plane" in the context of FAR is the] Current plane in voxel shell filling algorithm

Their [voxels'] size varies with the size of the vessel bounds, there's a limit to the number of voxels

CL and CD are computed first and then used to compute the actual forces with units


# FAR reverse engineering, Katniss:

simulation and flight contexts

FARVesselAero
- main runtime vessel simulation class

FARAeroSection
- can be merged together, based on flatness (within 5%)
- are used to calculate the aero forces on a voxel vessel

main axis is the axis along which the VoxelCrossSections and FARAeroSections are iterated.

Each FARAeroSection has a corresponding VoxelCrossSection, and their counts match
VehicleVoxel.Volume is the volume of the entire voxel domain, not each voxel.

voxel count is the total count, not per axis.



# AI analysis, take with a large grain of salt:

### High-level architecture

- **Core flow**: `FARVesselAero` owns the runtime simulation for a `Vessel`. Each update it:
  - Ensures geometry modules are ready (`GeometryPartModule` on each `Part`), then builds/updates `VehicleAerodynamics`.
  - `VehicleAerodynamics` queries a `VehicleVoxel` to obtain an ordered array of `VoxelCrossSection` along the chosen main axis, computes section properties, builds/merges `FARAeroSection` objects, and binds parts to sections with per-part factors.
  - Each `FARAeroSection` computes forces/torques per part using local kinematics and pre-baked Mach-dependent curves; results are accumulated by each `FARAeroPartModule` which then applies them to Unity `Rigidbody`s.
  - `VesselIntakeRamDrag` applies intake-specific drag; legacy wings (`LEGACYferram4`) compute forces separately and can provide a reference area for GUI.

### Atmosphere and gas properties

- **Gas properties**: `FARAtmosphere.GetGasProperties(body, latLonAlt, ut)` returns a `GasProperties` with `Pressure`, `Temperature`, `AdiabaticIndex`, `GasConstant`, and derived `SpeedOfSound = sqrt(γ R T)`.
- **Data sources**: Defaults call stock KSP APIs (`GetPressure`, `GetTemperature`, `atmosphereAdiabaticIndex`, and `PhysicsGlobals.IdealGasConstant / body.atmosphereMolarMass`). Custom dispatchers can override; `IsCustom` flags when any are overridden.
- **Wind**: `GetWind(...)` currently returns zero by default; can be overridden.
- **Reynolds number**: `FARAeroUtil.CalculateReynoldsNumber(ρ, L, V, M, T, γ)` with skin friction from `FARAeroUtil.SkinFrictionDrag(Re, M)` using piecewise laminar/turbulent correlations and a roughness multiplier.

### Voxelization and cross-sections

- **Voxel domain**: `VehicleVoxel` builds a 3D grid of `VoxelChunk` over vessel bounds. Voxel element size and resolution depend on settings (`FARSettingsScenarioModule.VoxelSettings`), with optional “higher res voxel points”.
- **Filling algorithm**: Triangles are rasterized to a watertight shell; the interior is filled by sweep from front/back using enter/exit parity. Along the sweep axis, the algorithm also calculates per-slice cross-section metrics in a single pass.
- **Cross-sections**: `VoxelCrossSection` per slice contains:
  - **area** and **centroid**
  - **secondAreaDeriv** (for slender-body wave drag usage)
  - **flatnessRatio** and **flatNormalVector** (shape anisotropy for body lift/drag)
  - **partSideAreaValues**: per-`Part` directional projected areas and exposure counts
  - **cpSonicForward/cpSonicBackward**: pressure coefficients near Mach 1 for forward/backward sweep
- **Main axis selection**: `VehicleAerodynamics` chooses `_vehicleMainAxis` and requests `CrossSectionData(...)`, which returns section range `[front, back]`, slice thickness, and `MaxCrossSectionArea`. Vehicle length = thickness × section count.

### Section generation, merging, and smoothing

- For each cross-section index `i` between `front..back`, compute potential-flow normal-force seed from area gradient:
  - `i = 0`: `ΔA = next - cur`
  - `i = N`: `ΔA = cur - prev`
  - else: `ΔA = 0.5 * (next - prev)`
- Derive a diameter proxy and flatness from `VoxelCrossSection` and store per-section parameters: `potentialFlowNormalForce`, `viscCrossflowDrag`, `diameter`, `flatnessRatio`, `hypersonicMomentForward/Backward` base coefficients, and pre-baked Mach curves for x-force (pressure and skin friction).
- Adjacent `FARAeroSection`s are merged if within 5% relative difference in `flatnessRatio` and `diameter`, and their normal vectors are sufficiently aligned. Merging averages geometry and sums coefficients while keeping per-part data coherent (scaled by `mergeFactor`).
- Smoothing of derivatives is applied based on fineness ratio and voxel grid “filledness” to reduce discretization noise in transonic shaping.

### Force model per section (per part)

- For each `PartData` in a section:
  - Compute part-space vectors: `xRefVector` (reference axial), `nRefVector` (section normal). Compute local velocity `velLocal = context.LocalVelocity + (centroid × angVel)` and its unit vector.
  - Compute AoA terms: `cosAoA = dot(xRef, vel̂)`, `sin²AoA = max(1 - cos², 0)`, `sin2AoA = 2 sin |cos|`, `cosHalfAoA = sqrt(0.5 + 0.5 |cos|)`.
  - Normal force direction: project against `xRefVector` to get `localNormalForceVec`.
  - Normal-force magnitude:
    - Potential-flow: `n_pot = potentialFlowNormalForce * sign(cosAoA) * cosHalfAoA * sin2AoA`, clipped to ≥ 0 on the rear face.
    - Viscous crossflow: compute crossflow Mach/Re `M_cf = M * sinAoA`, `Re_cf = (Re/L) * diameter * sinAoA / normalForceFactor`. Then
      `n_visc = viscCrossflowDrag * sin²AoA * CrossFlowDrag(M_cf, Re_cf)`.
    - Apply anisotropy: `normalForceFactor = mix(1/flatnessRatio, flatnessRatio, (dot(localNormalForceVec, nRef))²)`, then `nForce = (n_pot + n_visc) * normalForceFactor`.
  - Axial force (x-force):
    - Skin friction: `x_sf = -Cf(Re,M) * curve_skinFriction(M) * sign(cosAoA) * cos²AoA`.
    - Pressure drag at AoA: add `cos²AoA * xForcePressureAoA{0|180}(M)` depending on sign(cosAoA).
    - Rarefied correction: subtract a component along velocity proportional to `pseudoKnudsenNumber = M / (Re + M)`; keep magnitude for anti-drag term.
  - Moments and damping:
    - Base moment `m = cosAoA * sinAoA`, scaled between forward/backward hypersonic factors by Mach (piecewise blend below/above ~0.6 and >6). Damping moment = `4 m` with same factor. Extra roll damping from skin friction with arm ≈ `0.5 * diameter` and adjusted by flatness.
    - Decompose angular velocity into axial and non-axial components; subtract damping proportional to these and inversely to `|vel|²` with floor to avoid blow-up at low speed.
  - Compose force/torque vectors:
    - `force = xForce * xRefVector + nForce * localNormalForceVec - localVelForce * vel̂`
    - `torque = cross(xRefVector, localNormalForceVec) * moment` (then subtract damping terms)
  - Optional per-part modifier: `AeroForceModifier(part, float3(-F∥, |F⊥|, |τ|))` rescales force/torque components.
  - Scale by `dragFactor` and apply to the part via context.

### Crossflow drag curves

- `CrossFlowDrag(M_cf, Re_cf)` uses empirical curves:
  - Mach curve: piecewise `FloatCurve` with bump near transonic (peaks ~1.0–2.1) then declines to ~1.2 by M=10.
  - Reynolds curve: `FloatCurve` that sharply reduces drag past transition (~2.5e5–5e5), blended out for `M_cf > 0.4` and ignored for `M_cf > 0.5`.
  - Final: `(ReCurve(Re_cf) - 1) * reynoldsInfluence(M_cf) + 1`, multiplied by Mach curve.

### Building sections from voxels

- For each slice, compute potential-flow seed from area gradient and sonic base drag weighting. Bake Mach curves for pressure and skin friction in each section. Compute `finenessRatio = length / maxDiameter` to tune smoothing passes. Store `xForcePressureAoA0`, `xForcePressureAoA180`, `xForceSkinFriction` as `FARFloatCurve` with precomputed keys.

### Per-part association and projected areas

- Each section stores `PartData` built by transforming world-space centroid and reference vectors into part local space using cached `PartTransformInfo` per part.
- `dragFactor` per part is proportional to its contribution in the cross-section (from `VoxelCrossSection.partSideAreaValues` aggregate). All forces and torques for the section are split to parts by this factor.
- Exposure/shielding is computed by `VehicleExposure` using GPU/CPU rasterization of renderers along `Airstream`/`Sun`/`Body` directions, controlling effective area of legacy wing models and part modules.

### Runtime integration and update cadence

- `FARKSPAddonFlightScene` sets up at scene start: loads aero data, initializes crossflow curves, disables stock aero force gizmos, and pumps the voxel main-thread task queue.
- `FARVesselAero` lifecycle:
  - `OnStart`: initializes lists and GUI; caches references.
  - `VesselUpdate(recalcGeoModules)`: rate-limited by `minPhysTicksPerUpdate`; builds current geometry module list; waits until all geometry ready; triggers geometry updates; builds/updates `VehicleAerodynamics`.
  - `CalculateAndApplyVesselAeroProperties`: updates `MachNumber`, `ReynoldsNumber` (using current `Length`), computes `skinFrictionDragCoefficient`, `pseudoKnudsenNumber`, updates each `FARAeroPartModule` kinematics, then asks each `FARAeroSection` to compute and apply forces. Applies intake ram drag. Finally `FARAeroPartModule.ApplyForces()` pushes to Unity physics.
  - `SimulateAeroProperties(...)`: prediction path for given `velocity`/`altitude` uses `FARAeroSection.PredictionCalculateAeroForces` with a simulation context and caches results by `(velocity, position)`.

### Mach regime specifics

- Transonic handling via:
  - Section-local `cpSonicForward/backward` from voxel sweep near M≈1.
  - Crossflow drag Mach curve with a pronounced bump near M≈1.
  - Pressure x-force curves blended by Mach and AoA sign.
- Hypersonic moments: separate forward/backward coefficients blended by Mach (<0.6 uses 0.6× opposite, >6 uses target, linear blend in between).
- Rarefied flow: `pseudoKnudsenNumber = M / (Re + M)` reduces axial force and adds an anti-drag term along velocity.

### Water/ocean and density blending

- Legacy wings: if the body has an ocean, density is blended by submerged fraction: `ρ = oceanDensity*1000*submerged + atmDensity*(1 - submerged)`; otherwise use atmospheric density. This approximates water as heavy viscous air.

### GUI and reference areas

- The flight GUI computes total aerodynamic forces by summing per-part forces/torques (`FARFlightGUI/PhysicsCalcs`). Reference area for coefficients:
  - Use legacy wing area if any wing models exist; otherwise use `VehicleAerodynamics.MaxCrossSectionArea`; fallback to 1 m².
  - Coefficients: `C = Force / (q * A_ref)` with `q = dynamicPressure`.

### Performance and threading

- Voxelization and cross-section building use a custom `VoxelizationThreadpool` to parallelize and offload from the main thread where possible; results that touch Unity API are marshaled back on the main thread.
- Object pooling: `VehicleVoxel` recycles `VoxelChunk` and sweep-plane buffers; queue sizes depend on voxel settings and vessel controllability.
- Update limiter avoids doing heavy work every physics tick unless needed.

### Key types (re-implementation checklist)

- `VehicleVoxel`: grid setup, triangle rasterization to shell, front/back fill, per-slice accumulation of `VoxelCrossSection` properties, per-part side areas.
- `VoxelCrossSection`: area, centroid, second derivative, flatness, normal, per-part area tallies, cp at sonic.
- `VehicleAerodynamics`: orchestrates voxel queries, builds `FARAeroSection`s from slices, merges/smooths, computes `Length`, `_sectionThickness`, `MaxCrossSectionArea`, maps parts to sections with transforms and factors.
- `FARAeroSection`: per-section force model with curves, per-part kinematics, anti-drag, damping, torque, and application hooks.
- `FARAtmosphere`: pressure/temperature/γ/R providers; `GasProperties` and `SpeedOfSound`.
- `FARAeroUtil`: Reynolds, skin friction correlations, shock/Prandtl–Meyer helpers.
- `FARVesselAero`: KSP `VesselModule` that ties everything into FixedUpdate, rate limits, and calls into sections to apply forces.

### Notes and caveats

- Voxel resolution and filledness influence smoothing and accuracy; low filledness increases smoothing to reduce noise.
- AoA sign splits pressure curves (AoA≈0 vs 180) to capture fore/aft asymmetry.
- Normal forces are clamped to zero on the rear in potential-flow part to avoid unphysical suction.
- Damping includes empirical gains to stabilize low-speed/high-AoA behavior.

