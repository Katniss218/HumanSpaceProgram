using KatnisssSpaceSimulator.Camera;
using KatnisssSpaceSimulator.Core;
using KatnisssSpaceSimulator.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.UI
{
    public class PartWindowManager : MonoBehaviour
    {
        List<PartWindow> _partWindows = new List<PartWindow>();

        private void Update()
        {
            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
            {
                return;
            }

            if( Input.GetKeyDown( KeyCode.Mouse0 ) )
            {
                if( !Physics.Raycast( CameraController.Instance.MainCamera.ScreenPointToRay( Input.mousePosition ), out RaycastHit hit ) )
                {
                    return;
                }

                Part part = hit.collider.GetComponentInParent<Part>();
                if( part == null )
                {
                    return;
                }

                _partWindows = _partWindows.Where( pw => pw != null ).ToList(); // remove destroyed windows from list.

                foreach( var pw in _partWindows )
                {
                    if( pw.Part == part )
                    {
                        return;
                    }
                }

                PartWindow window = PartWindow.Create( part );
                _partWindows.Add( window );
            }
        }
    }
}