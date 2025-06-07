using UnityPlus.Serialization;

namespace HSP.Effects
{
    public class ConstantEffectValue<T>
    {
        public T value { get; }

        public EffectValueDriver<T>[] drivers { get; set; }

        public ConstantEffectValue() { }

        public ConstantEffectValue( T value )
        {
            this.value = value;
        }

        public void InitDrivers<TDriven>( TDriven handle )
        {
            if( drivers == null || drivers.Length == 0 )
                return;

            foreach( var driver in drivers )
            {
                if( driver.Getter is IInitValueGetter<TDriven> initGetter )
                    initGetter.OnInit( handle );
            }
        }

        public T Get()
        {
            T accValue = value;

            if( drivers == null || drivers.Length == 0 )
                return accValue;

            foreach( var driver in drivers )
            {
                accValue = driver.Curve.Interpolator.Multiply( accValue, driver.Curve.Evaluate( driver.Getter.Get() ) );
            }

            return accValue;
        }


        [MapsInheritingFrom( typeof( ConstantEffectValue<> ) )]
        public static SerializationMapping ConstantEffectValueMapping()
        {
            return new MemberwiseSerializationMapping<ConstantEffectValue<T>>()
                .WithReadonlyMember( "value", o => o.value )
                .WithFactory<T>( ( value ) => new ConstantEffectValue<T>( value ) )
                .WithMember( "drivers", o => o.drivers );
        }
    }
}