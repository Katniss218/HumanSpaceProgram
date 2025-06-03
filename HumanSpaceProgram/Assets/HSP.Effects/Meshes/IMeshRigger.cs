using System.Collections.Generic;
using UnityEngine;

namespace HSP.Effects.Meshes
{
    public interface IMeshRigger
    {
        void RigInPlace( Mesh mesh, IReadOnlyList<BindPose> bones );

        Mesh RigCopy( Mesh mesh, IReadOnlyList<BindPose> bones );
    }
}