using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib.Layout
{
    public static class UILayout
    {
        static HashSet<object> _set = new HashSet<object>();

        private static void BroadcastLayoutUpdateRecursive( object elem )
        {
            // TODO - this can be optimized:
            // - Mark as stale when something changes and redraw only once in lateupdate (problem - might lag a frame behind if the thing marking it as stale also runs in lateupdate).

            // ----

            // Update the children first (because the other dimensions of the changed UI element might depend on them).
            // - E.g. if the width of a vertical layout elem is changed, the text in children might get taller or shorter to fit the new width.
            if( elem is IUIElementContainer co )
            {
                foreach( var e in co.Children )
                {
                    if( !_set.Contains( e ) )
                    {
                        _set.Add( e );
                        BroadcastLayoutUpdateRecursive( e );
                    }
                }
            }

            // Update our element (self).
            if( elem is IUILayoutDriven ld )
            {
                ld.LayoutDriver?.DoLayout( ld );
            }
            else if( elem is IUILayoutSelf ls )
            {
                ls.DoLayout();
            }

            // Notify the parent, so it can re-layout itself if the dimensions of our element have changed.
            if( elem is IUIElementChild ci )
            {
                if( !_set.Contains( ci.Parent ) )
                {
                    _set.Add( ci.Parent );
                    BroadcastLayoutUpdateRecursive( ci.Parent );
                }
            }
        }

        public static void BroadcastLayoutUpdate( IUIElement elem )
        {
#warning TODO - This needs to be called when a UI element is added, removed, or has changed its sibling order.

            // also when an object is destroyed, it desyncs.

            _set.Clear();
            BroadcastLayoutUpdateRecursive( elem );
        }
    }
}