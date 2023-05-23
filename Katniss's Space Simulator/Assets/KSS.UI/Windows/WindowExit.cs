using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace KSS.UI.Windows
{
    public class WindowExit : MonoBehaviour
    {
        [field: SerializeField]
        public RectTransform UITransform { get; set; }

        [SerializeField]
        public Button ExitButton { get; set; }

        void Start()
        {
            ExitButton.onClick.AddListener( Exit );
        }

        public void Exit()
        {
            Destroy( UITransform.gameObject );
        }
    }
}