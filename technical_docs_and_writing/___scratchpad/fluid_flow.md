# Fluid Flow

    This document is not strictly documentation. It hopefully should be close, but I'm not perfect and sometimes can't be bothered to update every doc.
#

Tanks are modelled as a network of thin cylinders.
These cylinders are placed at the edges of a tetrahedral mesh, connected at the vertices.

The cylinders have volume and are where the fluid is stored.
The volume of a cylinder (edge) is related to the volume of the N tetrahedrons neighboring it - proportional to the sum from 1 to N, of 1/6 * volume_of_Nth_tetrahedron.
- the volume of each tetrahedron is split among its 6 edges.
- The volumes are scaled such that the total volume of all cylinders is equal to the desired tank volume (set by the modder).

Voids can be represented by something that prevents the formation of tetrahedrons.

#### Inlets / Outlets:
Several nodes are placed around the tank, and 1 more for every inlet/outlet.

The fluid can enter/exit the tank at a node (vertex).
It is settled into hydrostatic equilibrium immediately as the inflow/outflow is processed.

#### Adding Nodes:
To add a new node, find 3 closest edges/nodes (should be equivalent), and these form the base of your new tetrahedron.
To figure out the pressure at an outlet, simply check where the highest level in the connected cylinders is (and add the tank's gas pressure to this head pressure, if applicable).

