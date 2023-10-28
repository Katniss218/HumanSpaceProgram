using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.DesignScene.Tools
{
    public class RotateTool : MonoBehaviour
    {
        [SerializeField]
        Transform _selectedPart;

        [SerializeField]
        Camera _camera;

        public bool SnappingEnabled { get; set; }
        public float SnapAngle { get; set; }

        void Update()
        {
            // click on part to select and toggle handles.
            // hold and mouse over handles to rotate.
            // Takes into account redirects ofc.
        }
    }
}