using UnityEngine;

namespace HSP.Trajectories
{
    public interface IAccelerationProvider
    {
        public Vector3Dbl GetAcceleration(); // returns acceleration at *current* ut.
    }
}