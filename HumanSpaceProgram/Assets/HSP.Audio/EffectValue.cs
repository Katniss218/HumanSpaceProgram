using UnityPlus.Serialization;

namespace HSP.Audio
{
    internal static class EffectValue_Mappings
    {
        [MapsInheritingFrom( typeof( ConstantEffectValue<> ) )]
        public static SerializationMapping ConstantEffectValueMapping<T>()
        {
            return new MemberwiseSerializationMapping<ConstantEffectValue<T>>()
                .WithReadonlyMember( "value", o => o.value )
                .WithFactory<T>( ( value ) => new ConstantEffectValue<T>( value ) )
                .WithMember( "drivers", o => o.drivers );
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

        [MapsInheritingFrom( typeof( EffectValueDriver<> ) )]
        public static SerializationMapping EffectValueDriverMapping<T>()
        {
            return new MemberwiseSerializationMapping<EffectValueDriver<T>>()
                .WithMember( "getter", o => o.Getter )
                .WithMember( "curve", o => o.Curve );
        }
    }

    public struct EffectValueDriver<T>
    {
        // translate them to builtin unity particle system stuff.
        public IValueGetter<T> Getter { get; set; }
        public IMappingCurve<T> Curve { get; set; }
    }

    public class ConstantEffectValue<T>
    {
        public T value { get; }

        public EffectValueDriver<T>[] drivers { get; set; }

        public ConstantEffectValue() { }

        public ConstantEffectValue( T value )
        {
            this.value = value;
        }

        public void OnInit<TDriven>( TDriven handle )
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
    }

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

        public void OnInit<TDriven>( TDriven handle )
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
    }
}