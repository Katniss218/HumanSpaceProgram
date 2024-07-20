using HSP.Vanilla.Components;
using UnityEngine;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Components
{
    public class UISequenceAction : UIPanel
    {
        // dropdown that selects the sequencer at the top.

        // dropdown position clamped to the top of the screen.

        private SequenceAction _sequenceControlGroup;

        private void Refresh()
        {
            // update icon (if applicable)
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout ) where T : UISequenceAction
        {
            T uiSequenceAction = (T)UIPanel.Create<T>( parent, layout, null )
                .WithTint( Color.white );

            // add list of actions.

            return uiSequenceAction;
        }
    }

    public static class UISequenceAction_Ex
    {
        public static UISequenceAction AddSequenceAction( this IUIElementContainer parent, UILayoutInfo layout )
        {
            return UISequenceAction.Create<UISequenceAction>( parent, layout );
        }
    }
}