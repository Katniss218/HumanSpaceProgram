using KSS.Core;
using KSS.Core.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.UILib.UIElements;

namespace KSS.UI
{
    public class VelocityReadoutUI : MonoBehaviour
    {
        public UIText Text { get; set; }

        void LateUpdate()
        {
            var physObj = ActiveObjectManager.ActiveObject?.GetComponent<FreePhysicsObject>();
            Text.Text = physObj == null ? "" : $"{physObj.Velocity.magnitude:#0} m/s";
        }
    }
}