# Propellant Flow

    This document is not strictly documentation. It hopefully should be close, but I'm not perfect and sometimes can't be bothered to update every doc.
#


if we do cubes (voxels) they have to be distributed in such a way that no volume inside the tank is left without a voxel, and no voxel is outside the tank.
- the voxels should be mostly inside the tank.
- could be arbitrary shapes tbh. only matters that the tank should be divided up into segments.

each inlet assigned a point that is closest to it?
maybe assign multiple points, and kind of interpolate for how long it has access to fluid inside. i.e. if inlet has access to 70% of point A and 30% of point B, it will
- DEPENDS ON POSITION OF THE POINT IN ACCELERATION SPACE (oriented to point along acceleration) (i.e. height)
then we take that height and point can feed until points above it are fully depleted and until points below it are depleted to 1 - the specified percent. 

throw a ray in the direction of felt acceleration and find the highest point that has fluid and is connected to the point with the inlet


## pressure:

we know the distance to the point, and the pressure at the point is computed every frame (multithreaded likely)




## flow

inflow and outflow have to be able to 'overflow' into the neighboring points, if the current point doesn't have enough volume 
   (small points - from cubes mostly outside the volume)


tanks start out uniformly mixed, each point has the same contents, each point is filled to the percent of the full tank

forcefully feeding stuff in should result in bursting of the tank


## inflow/outflow

inflow/outflow removes/adds fluids to/from cells (overflowing if not enough fluid is available)

maximum inflow/outflow needs to be capped due to contents in the tank not being able to move fast enough to reach the cells
this changes the pressure (or suction), making the contents move faster.



multithread:
{
    calculate tanks for this frame from existing inflow/outflow and acceleration relative to tank
    calculate updated inflow/outflow
    update inflow/outflow for the next frame
}

**damping or something to prevent 1-frame oscillations**









