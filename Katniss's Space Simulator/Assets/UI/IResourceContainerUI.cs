using KatnisssSpaceSimulator.Core.ResourceFlowSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KatnisssSpaceSimulator.UI
{
    public class IResourceContainerUI : MonoBehaviour
    {
        /// <summary>
        /// The object that is referenced by this UI element.
        /// </summary>
        public IResourceContainer ReferenceObject { get; set; }

        [SerializeField]
        private Image _fillImg;

        void Update()
        {
            float perc = ReferenceObject.Contents.GetVolume() / ReferenceObject.MaxVolume;

            _fillImg.fillAmount = perc;
        }
    }
}