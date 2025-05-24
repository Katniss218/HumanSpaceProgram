using UnityPlus.Serialization;

namespace HSP.Effects
{
    public class MinMaxEffectValue<T>
    {
        public T min { get; }
        public T max { get; }

        public EffectValueDriver<T>[] drivers { get; set; }

        public MinMaxEffectValue() { }

        public MinMaxEffectValue( T value )
        {
            this.min = value;
            this.max = value;
        }

        public MinMaxEffectValue( T min, T max )
        {
            this.min = min;
            this.max = max;
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

        public T GetMin()
        {
            T accMin = min;

            if( drivers == null || drivers.Length == 0 )
                return min;

            foreach( var driver in drivers )
            {
                accMin = driver.Curve.Interpolator.Multiply( accMin, driver.Curve.Evaluate( driver.Getter.Get() ) );
            }

            return accMin;
        }

        public T GetMax()
        {
            T accMax = max;

            if( drivers == null || drivers.Length == 0 )
                return max;

            foreach( var driver in drivers )
            {
                accMax = driver.Curve.Interpolator.Multiply( accMax, driver.Curve.Evaluate( driver.Getter.Get() ) );
            }

            return accMax;
        }

        public (T min, T max) Get()
        {
            T accMin = min;
            T accMax = max;

            if( drivers == null || drivers.Length == 0 )
                return (min, max);

            foreach( var driver in drivers )
            {
                accMin = driver.Curve.Interpolator.Multiply( accMin, driver.Curve.Evaluate( driver.Getter.Get() ) );
                accMax = driver.Curve.Interpolator.Multiply( accMax, driver.Curve.Evaluate( driver.Getter.Get() ) );
            }

            return (accMin, accMax);
        }


        [MapsInheritingFrom( typeof( MinMaxEffectValue<> ) )]
        public static SerializationMapping MinMaxEffectValueMapping<T>()
        {
            return new MemberwiseSerializationMapping<MinMaxEffectValue<T>>()
                .WithReadonlyMember( "min", o => o.min )
                .WithReadonlyMember( "max", o => o.max )
                .WithFactory<T, T>( ( min, max ) => new MinMaxEffectValue<T>( min, max ) )
                .WithMember( "drivers", o => o.drivers );
        }
    }
}