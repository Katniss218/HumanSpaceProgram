using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.DesignScene.Tools
{
    /// <summary>
    /// Allows to move a selected part after placing.
    /// </summary>
    public class TranslateTool : MonoBehaviour
    {
        [SerializeField]
        Transform _selectedPart;

        [SerializeField]
        Camera _camera;

        public bool SnappingEnabled { get; set; }
        public float SnapInterval { get; set; }

        void Awake()
        {
            _camera = GameObject.Find( "Near camera" ).GetComponent<Camera>();
        }

        void Update()
        {
            // click on part to select and toggle handles.
            // hold and mouse over handles to translate.
            // Takes into account redirects ofc.
        }
    }
}