using HSP.Time;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Time
{
    /// <summary>
    /// A readout for the mission elapsed time.
    /// </summary>
    public partial class UITextReadout_ElapsedUT : UIText
    {
        public double ReferenceUT { get; set; }

        void LateUpdate()
        {
            this.Text = $"{(TimeManager.UT - ReferenceUT):#0} s";
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout ) where T : UITextReadout_ElapsedUT
        {
            return UIText.Create<T>( parent, layout, "<placeholder_text>" );
        }
    }

    public static class UITextReadout_ElapsedUT_Ex
    {
        public static UITextReadout_ElapsedUT AddTextReadout_ElapsedUT( this IUIElementContainer parent, UILayoutInfo layout )
        {
            return UITextReadout_ElapsedUT.Create<UITextReadout_ElapsedUT>( parent, layout );
        }
    }

    public partial class UITextReadout_ElapsedUT
    {
        public UITextReadout_ElapsedUT WithReferenceUT( double referenceUT )
        {
            this.ReferenceUT = referenceUT;
            return this;
        }
    }
}