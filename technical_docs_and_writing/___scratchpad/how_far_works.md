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


