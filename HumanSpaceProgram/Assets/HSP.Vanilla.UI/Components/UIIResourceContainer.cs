using HSP.ResourceFlow;
using HSP.Vanilla.Components;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Components
{
    public class UIIResourceContainer : UIPartWindowComponent<IResourceContainer>
    {
        UIValueBar _bar;

        void Update()
        {
#warning TODO - Inefficient as fuck.
            _bar.ClearSegments();

            for( int i = 0; i < this.ReferenceComponent.Contents.Count; i++ )
            {
                var sbs = this.ReferenceComponent.Contents[i];
                float perc = (float)(sbs.mass / sbs.s.GetDensityAtSTP()) / ReferenceComponent.MaxVolume;

                var seg = _bar.AddSegment( perc );
                seg.Sprite = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar" );
                seg.Color = sbs.s.DisplayColor;
            }
        }

        protected internal static T Create<T>( IUIElementContainer parent, IResourceContainer referenceComponent ) where T : UIIResourceContainer
        {
            T uiIResourceContainer = UIPartWindowComponent<IResourceContainer>.Create<T>( parent, referenceComponent );

            uiIResourceContainer.Title = "Tank";
            uiIResourceContainer.OpenHeight = 15f;

            UIValueBar uiValueBar = uiIResourceContainer.contentPanel.AddHorizontalValueBar( new UILayoutInfo( UIFill.Fill() ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/bar_background" ) )
                .WithPadding( 5, 5, 1 );

            uiIResourceContainer._bar = uiValueBar;

            UILayoutManager.ForceLayoutUpdate( uiIResourceContainer );

            return uiIResourceContainer;
        }
    }

    public static class UIIResourceContainer_Ex
    {
        public static UIIResourceContainer AddIResourceContainer( this IUIElementContainer parent, IResourceContainer referenceObj )
        {
            return UIIResourceContainer.Create<UIIResourceContainer>( parent, referenceObj );
        }
    }

    public static class ON_PART_WINDOW_REDRAW_Listener
    {
        public const string ADD_UIIResourceContainer = HSPEvent.NAMESPACE_HSP + ".add_uiiresourcecontainer";

        [HSPEventListener( HSPEvent_ON_PART_WINDOW_REDRAW.ID, ADD_UIIResourceContainer )]
        public static void OnPartWindowRedraw( (IUIElementContainer parent, Component component) e )
        {
            if( e.component is not IResourceContainer res )
                return;

            e.parent.AddIResourceContainer( res );
        }
    }
}