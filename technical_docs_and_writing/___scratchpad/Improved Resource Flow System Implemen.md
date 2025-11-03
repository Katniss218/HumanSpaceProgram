# Improved Resource Flow System Implementation

## Overview

Replace the current black-box tank system with a tetrahedralized lumped flow model where fluid is stored on edges, enabling proper handling of stratified fluids, compressibility, and robust flow solving.

## Core Architecture

### 1. Data Structures (HSP.ResourceFlow namespace)

- **`FlowNode`**: Represents a vertex in the tetrahedralization (local position Vector3)
- **`FlowEdge`**: Stores fluid contents (SubstanceStateCollection), has proper volume, connects two nodes
- **`FlowTetrahedron`**: Four nodes forming a tetrahedron, tracks desired/actual volumes
- **`FlowTank`**: Main container component implementing IResourceContainer, IResourceProducer, IResourceConsumer
- Contains tetrahedralization (nodes, edges, tetrahedra)
- Manages inlet nodes (created dynamically or snapped to existing nodes)
- Handles fluid stratification/mixing based on acceleration
- **`FlowNetwork`**: Groups tanks and pipes into independent networks for parallel solving
- **`FlowPipe`**: Replaces FBulkConnection, connects two FlowTanks via inlet nodes
- Can be simple pipe, pump, valve, etc.
- Calculates flow based on pressure difference and inlet areas

### 2. Volume Calculation System

- Compute desired volume for each tetrahedron based on user-defined tank volume
- Scale to actual total volume: `actual = (desired / desired_total) * actual_total`
- Distribute tetrahedron volume to edges proportional to edge length
- Each edge's proper volume = sum of contributions from all tetrahedra containing it

### 3. Tetrahedralization Management

- Initial tetrahedralization from user-defined nodes (convex hull or Delaunay)
- Dynamic inlet insertion:
- If outside existing tetrahedra: add new tetrahedron between closest triangle and inlet
- If inside existing tetrahedron: split into 4 new tetrahedra with new node
- If close to existing node: snap node to inlet position (track for restoration)
- Dynamic inlet removal: reverse the insertion operation
- Maintain consistency: removing all added inlets should restore original structure

### 4. Flow Physics

#### Inside Tank Flow

- Infinite flow capacity along edges within tank
- Fluid stratification:
- Under acceleration: stratified (no mixing, ullage gas on top)
- Zero-g: perfectly mixed
- Near zero-g: lerp between stratified and mixed (with time-varying smoothing)
- Calculate fluid height along each edge by projecting onto acceleration vector

#### Between Tank Flow

- Flow rate based on pressure difference and inlet cross-sectional areas
- Torricelli's law for velocity: `sqrt((2 * (P1 - P2)) / density)`
- Drain edges connected to outlet inlet:
- Liquids: drain until fluid level below inlet
- Gasses: can drain from below as well
- Back-fill/settle drained edges after outflow

### 5. Compressible Fluids

- Extend `Substance` class with compressibility properties (bulk modulus, ideal gas law parameters)
- Update `SubstanceStateCollection.GetVolume()` to account for pressure
- Update volume calculations throughout system

### 6. Flow Solver

- Implement robust solver for stiff systems (handles pressure spikes, loops, dead-heading)
- Network-based approach: solve each independent FlowNetwork separately
- Matrix-based solver using extended MatrixMxN:
- Add sparse matrix support or dense solver
- Newton-Raphson iteration for non-linear flow equations
- Handle edge cases:
- Dead-heading (pump into closed/overfilled tank)
- Loops (A → B → C → A)
- Overfilling/underfilling
- Cavitation (pressure drop, liquid boils to gas)

### 7. SubstanceStateCollection Pooling

- Implement object pooling for SubstanceStateCollection to reduce allocations
- Pool backing List<SubstanceState> arrays
- Reuse collections in hot paths (flow calculations, edge updates)

### 8. Integration Points

#### Update Interfaces

- Keep IResourceContainer, IResourceProducer, IResourceConsumer interfaces
- FlowTank implements all three
- Update Sample() and SampleFlow() methods for new model

#### Component Updates

- **Engines (FRocketEngine)**: Accept any fluid, work "correctly" only near design parameters
- **Pumps**: Set design pressure/flowrate
- **Vents/Intakes**: Connect as separate producer/consumer tanks

#### Serialization

- Serialize tetrahedralization (nodes, edges, tetrahedra)
- Serialize inlet positions and associations
- Handle migration from old FBulkContainer_Sphere format

### 9. Performance Optimizations

- Precompute and cache:
- Edge proper volumes
- Edge-to-tetrahedron mappings
- Node-to-edge adjacency lists
- Fluid height calculations (projected onto acceleration)
- Multithreading:
- Solve independent FlowNetworks in parallel
- Use Unity's Job System or thread-safe APIs
- Worker-thread-safe types throughout

### 10. Testing & Validation

- Unit tests for:
- Volume calculations
- Tetrahedralization operations (insert/remove inlets)
- Flow solver accuracy
- Edge cases (dead-heading, loops, overfilling)
- Performance tests for 100-tank networks
- Validation against expected physical behavior

## Implementation Files

### New Files

- `HumanSpaceProgram/Assets/HSP.ResourceFlow/FlowNode.cs`
- `HumanSpaceProgram/Assets/HSP.ResourceFlow/FlowEdge.cs`
- `HumanSpaceProgram/Assets/HSP.ResourceFlow/FlowTetrahedron.cs`
- `HumanSpaceProgram/Assets/HSP.ResourceFlow/FlowTank.cs`
- `HumanSpaceProgram/Assets/HSP.ResourceFlow/FlowNetwork.cs`
- `HumanSpaceProgram/Assets/HSP.ResourceFlow/FlowPipe.cs`
- `HumanSpaceProgram/Assets/HSP.ResourceFlow/FlowSolver.cs`
- `HumanSpaceProgram/Assets/HSP.ResourceFlow/Tetrahedralization.cs`
- `HumanSpaceProgram/Assets/HSP.ResourceFlow/SubstanceStateCollectionPool.cs`
- `HumanSpaceProgram/Assets/HSP.ResourceFlow/CompressibleSubstance.cs` (extend Substance)

### Modified Files

- `HumanSpaceProgram/Assets/HSP.ResourceFlow/Substance.cs` - Add compressibility properties
- `HumanSpaceProgram/Assets/HSP.ResourceFlow/SubstanceStateCollection.cs` - Add pooling support, pressure-aware volume
- `HumanSpaceProgram/Assets/_UnityEngine/MatrixMxN.cs` - Add sparse matrix or dense solver methods
- `HumanSpaceProgram/Assets/HSP.Vanilla/Components/FRocketEngine.cs` - Update to accept any fluid type
- `HumanSpaceProgram/Assets/HSP.Vanilla/Components/FBulkConnection.cs` - Mark deprecated, replaced by FlowPipe

## Implementation Phases

### Phase 1: Core System (Initial Implementation)

- Build basic data structures and tetrahedralization
- Simple iterative flow solver (functional but not optimized)
- Basic flow physics (no compression initially, add later)
- Get to testable/runnable state

### Phase 2: Optimization

- Implement sparse matrix solver
- Add multithreading for independent networks
- Optimize caching and precomputation
- Performance tuning for 100+ tank networks

## Notes

- Edge proper volume calculation is critical for accurate fluid height determination
- Inlet creation/deletion must preserve fluid contents (no duplication/loss)
- Solver must handle stiff systems (rapid pressure changes)
- All operations should be order-independent where possible