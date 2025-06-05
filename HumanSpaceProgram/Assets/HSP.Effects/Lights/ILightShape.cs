namespace HSP.Effects.Lights
{
    public interface ILightShape
    {
        public void OnInit( LightEffectHandle handle );

        public void OnUpdate( LightEffectHandle handle );
    }
}