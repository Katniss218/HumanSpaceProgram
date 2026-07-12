using System;
using Unity.Collections;
using UnityEngine;

namespace HSP.Vessels
{
    public class PhysicsDataUtils
    {
        /// <summary>
        /// Calculates the volume, surface area, center of mass, and principal inertia tensor of a closed, triangulated mesh.
        /// It uses the divergence theorem (often referred to as Mirtich's algorithm or the tetrahedron method).
        /// Accumulation is done about the origin for simplicity and efficiency in a single pass,
        /// and then shifted to the Center of Mass using the Parallel Axis Theorem.
        /// </summary>
        public static void CalculateMeshInertia( Mesh mesh, out float volume, out float surfaceArea, out Vector3 centerOfMass, out Vector3 inertiaTensor, out Quaternion inertiaTensorRotation )
        {
            using var vertices = new NativeArray<Vector3>( mesh.vertices, Allocator.TempJob );
            using var indices = new NativeArray<int>( mesh.triangles, Allocator.TempJob );

            CalculateMeshInertia( vertices, indices, out volume, out surfaceArea, out centerOfMass, out inertiaTensor, out inertiaTensorRotation );
        }

        /// <summary>
        /// Raw data overload, suitable for multithreaded jobs.
        /// </summary>
        public static void CalculateMeshInertia( NativeArray<Vector3> vertices, NativeArray<int> indices, out float volume, out float surfaceArea, out Vector3 centerOfMass, out Vector3 inertiaTensor, out Quaternion inertiaTensorRotation )
        {
            double sumVol = 0;
            double sumArea = 0;
            double sumCx = 0, sumCy = 0, sumCz = 0;
            double sumExx = 0, sumEyy = 0, sumEzz = 0;
            double sumExy = 0, sumEyz = 0, sumEzx = 0;

            for( int i = 0; i < indices.Length; i += 3 )
            {
                var v1 = vertices[indices[i]];
                var v2 = vertices[indices[i + 1]];
                var v3 = vertices[indices[i + 2]];

                double x1 = v1.x, y1 = v1.y, z1 = v1.z;
                double x2 = v2.x, y2 = v2.y, z2 = v2.z;
                double x3 = v3.x, y3 = v3.y, z3 = v3.z;

                // Signed volume of the tetrahedron (origin, v1, v2, v3) * 6
                double dV = x1 * (y2 * z3 - y3 * z2) + x2 * (y3 * z1 - y1 * z3) + x3 * (y1 * z2 - y2 * z1);

                sumVol += dV;

                sumCx += dV * (x1 + x2 + x3);
                sumCy += dV * (y1 + y2 + y3);
                sumCz += dV * (z1 + z2 + z3);

                sumExx += dV * (x1 * x1 + x2 * x2 + x3 * x3 + x1 * x2 + x2 * x3 + x3 * x1);
                sumEyy += dV * (y1 * y1 + y2 * y2 + y3 * y3 + y1 * y2 + y2 * y3 + y3 * y1);
                sumEzz += dV * (z1 * z1 + z2 * z2 + z3 * z3 + z1 * z2 + z2 * z3 + z3 * z1);

                sumExy += dV * (2 * x1 * y1 + 2 * x2 * y2 + 2 * x3 * y3 + x1 * y2 + x2 * y1 + x1 * y3 + x3 * y1 + x2 * y3 + x3 * y2);
                sumEyz += dV * (2 * y1 * z1 + 2 * y2 * z2 + 2 * y3 * z3 + y1 * z2 + y2 * z1 + y1 * z3 + y3 * z1 + y2 * z3 + y3 * z2);
                sumEzx += dV * (2 * z1 * x1 + 2 * z2 * x2 + 2 * z3 * x3 + z1 * x2 + z2 * x1 + z1 * x3 + z3 * x1 + z2 * x3 + z3 * x2);

                // Area calculation of the triangle
                double ux = x2 - x1;
                double uy = y2 - y1;
                double uz = z2 - z1;
                double vx = x3 - x1;
                double vy = y3 - y1;
                double vz = z3 - z1;

                double cx2 = uy * vz - uz * vy;
                double cy2 = uz * vx - ux * vz;
                double cz2 = ux * vy - uy * vx;

                double triArea2 = cx2 * cx2 + cy2 * cy2 + cz2 * cz2;
                if( triArea2 > 0 )
                {
                    sumArea += Math.Sqrt( triArea2 );
                }
            }

            if( Math.Abs( sumVol ) < 1e-12 )
            {
                volume = 0;
                surfaceArea = 0;
                centerOfMass = Vector3.zero;
                inertiaTensor = Vector3.one;
                inertiaTensorRotation = Quaternion.identity;
                return;
            }

            volume = (float)(sumVol / 6.0);
            surfaceArea = (float)(sumArea * 0.5);

            // Center of mass
            double cx = sumCx / (4.0 * sumVol);
            double cy = sumCy / (4.0 * sumVol);
            double cz = sumCz / (4.0 * sumVol);
            centerOfMass = new Vector3( (float)cx, (float)cy, (float)cz );

            // Inertia about the origin
            double Exx = sumExx / 60.0;
            double Eyy = sumEyy / 60.0;
            double Ezz = sumEzz / 60.0;
            double Exy = sumExy / 120.0;
            double Eyz = sumEyz / 120.0;
            double Ezx = sumEzx / 120.0;

            double Ixx_O = Eyy + Ezz;
            double Iyy_O = Exx + Ezz;
            double Izz_O = Exx + Eyy;
            double Ixy_O = -Exy;
            double Iyz_O = -Eyz;
            double Izx_O = -Ezx;

            // Shift to Center of Mass using Parallel Axis Theorem
            double vol = volume;
            double Ixx = Ixx_O - vol * (cy * cy + cz * cz);
            double Iyy = Iyy_O - vol * (cx * cx + cz * cz);
            double Izz = Izz_O - vol * (cx * cx + cy * cy);
            double Ixy = Ixy_O + vol * cx * cy;
            double Iyz = Iyz_O + vol * cy * cz;
            double Izx = Izx_O + vol * cz * cx;

            Matrix3x3 inertiaMatrix = new Matrix3x3(
                (float)Ixx, (float)Ixy, (float)Izx,
                (float)Ixy, (float)Iyy, (float)Iyz,
                (float)Izx, (float)Iyz, (float)Izz
            );

            inertiaTensor = inertiaMatrix.PhysxDiagonalize( out inertiaTensorRotation );
        }
    }
}
