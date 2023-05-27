using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UILib;
using UnityEngine;

namespace KSS.Core
{
    public class UIStyleManager : MonoBehaviour
    {
        static UIStyleManager _instance;
        public static UIStyleManager Instance
        {
            get
            {
                if( _instance == null )
                {
                    _instance = FindObjectOfType<UIStyleManager>();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets or sets the style used to create *new* UI elements.
        /// </summary>
        [field: SerializeField]
        public UIStyle Style { get; internal set; } // this won't reset the existing elements, no need to bother.
    }
}
