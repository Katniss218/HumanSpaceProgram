using HSP.ReferenceFrames;
using HSP.Vanilla.Components;
using HSP.Vessels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.HSP.Vanilla
{
    /// <summary>
    /// Manages the control frame selected by the player.
    /// </summary>
    public class SelectedControlFrameManager : SingletonMonoBehaviour<SelectedControlFrameManager>
    {
        private FControlFrame _selectedControlFrame;

        public static FControlFrame ControlFrame
        {
            get => instance._selectedControlFrame;
            set => instance._selectedControlFrame = value;
        }

        /// <summary>
        /// Tries to get the rotation of the control frame (in scene space). Falls back to the rotation of the vessel if unavailable.
        /// </summary>
        /// <returns>The rotation of the specified control frame, or the vessel.</returns>
        public static Quaternion GetRotation( Transform fallback )
        {
            return instance._selectedControlFrame != null
                ? instance._selectedControlFrame.GetRotation()
                : fallback.rotation;
        }

        /// <summary>
        /// Tries to get the rotation of the control frame (in absolute inertial space). Falls back to the rotation of the vessel if unavailable.
        /// </summary>
        /// <returns>The rotation of the specified control frame, or the vessel.</returns>
        public static QuaternionDbl GetAbsoluteRotation( Transform fallback )
        {
            return SceneReferenceFrameManager.ReferenceFrame.TransformRotation( instance._selectedControlFrame != null 
                ? instance._selectedControlFrame.GetRotation() 
                : fallback.rotation );
        }

        // TODO - other control signal generators might want to be hooked up to a different control frame, but that's a different system.
    }
}