using KatnisssSpaceSimulator.Core.ResourceFlowSystem;
using KatnisssSpaceSimulator.UILib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.AssetManagement;

namespace KatnisssSpaceSimulator.UI
{
    public class IResourceContainerUI : MonoBehaviour
    {
        /// <summary>
        /// The object that is referenced by this UI element.
        /// </summary>
        public IResourceContainer ReferenceObject { get; set; }

        [SerializeField]
        Image _image;

        void Start()
        {
            _image.type = Image.Type.Filled;
            _image.fillMethod = Image.FillMethod.Horizontal;
            _image.fillOrigin = 0;
        }

        void Update()
        {
            float perc = ReferenceObject.Contents.GetVolume() / ReferenceObject.MaxVolume;

            _image.fillAmount = perc;
        }

        public static IResourceContainerUI Create( RectTransform parent, IResourceContainer referenceObj )
        {
            GameObject root = UIHelper.UI( parent, "resource container UI", Vector2.zero, Vector2.zero, new Vector2( 250, 25 ) );

            GameObject bg = UIHelper.UI( root.transform, "background", Vector2.zero, Vector2.one, new Vector2( 0.5f, 0.5f ), Vector2.zero, Vector2.zero );

            Image image = bg.AddComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;
            image.type = Image.Type.Sliced;
            image.sprite = AssetRegistry<Sprite>.GetAsset( "Sprites/ui_pw_fillbar" );

            GameObject fg = UIHelper.UI( bg.transform, "foreground", Vector2.zero, Vector2.one, new Vector2( 0.5f, 0.5f ), Vector2.zero, Vector2.zero );

            Image imagefg = fg.AddComponent<Image>();
            imagefg.color = Color.green;
            imagefg.raycastTarget = false;
            imagefg.type = Image.Type.Filled;
            imagefg.sprite = AssetRegistry<Sprite>.GetAsset( "Sprites/ui_pw_fillbar" );

            IResourceContainerUI ui = root.AddComponent<IResourceContainerUI>();
            ui._image = imagefg;
            ui.ReferenceObject = referenceObj;

            return ui;
        }
    }
}