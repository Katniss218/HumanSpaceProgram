using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib
{
    /// <summary>
    /// Destroys a <see cref="GameObject"/> after the specified button has been pressed.
    /// </summary>
    public class RectTransformCloser : MonoBehaviour
    {
        [field: SerializeField]
        public RectTransform UITransform { get; set; }

        [field: SerializeField]
        Button _exitButton;

        public Func<bool> CanClose { get; set; } = True;

        static bool True()
        {
            return true;
        }

        public Button ExitButton
        {
            get => _exitButton;
            set
            {
                if( _exitButton != null )
                {
                    _exitButton.onClick.RemoveListener( this.TryClose );
                }

                _exitButton = value;

                if( _exitButton != null )
                {
                    _exitButton.onClick.AddListener( this.TryClose );
                }
            }
        }

        void Start()
        {
            if( ExitButton == null )
            {
                return;
            }

            ExitButton.onClick.AddListener( this.TryClose ); // this will add twice if we set the button using code, but we can't check if it's already added.
        }

        void TryClose()
        {
            if( CanClose != null && !(CanClose()) )
            {
                return;
            }
            if( UITransform == null )
            {
                return;
            }

            Destroy( UITransform.gameObject );
            if( ExitButton != null )
            {
                ExitButton.onClick.RemoveListener( this.TryClose );
            }
        }
    }
}