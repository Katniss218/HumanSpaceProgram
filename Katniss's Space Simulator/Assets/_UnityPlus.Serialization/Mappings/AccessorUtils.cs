using System;
using System.Linq.Expressions;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A method that returns a TMember that is a member of TSource.
    /// </summary>
    public delegate TMember Getter<TSource, TMember>( TSource item );

    /// <summary>
    /// A method that sets a TMember that is a member of TSource.
    /// </summary>
    public delegate void Setter<TSource, TMember>( TSource item, TMember member );

    /// <summary>
    /// A method that sets a TMember that is a member of TSource.
    /// </summary>
    public delegate void RefSetter<TSource, TMember>( ref TSource item, TMember member );

    /// <summary>
    /// Contains static helper methods that create getter/setter lambdas from expressions.
    /// </summary>
    public static class AccessorUtils
    {
        /// <summary>
        /// Creates a getter method from the member access expression.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the expression is not a member access.</exception>
        public static Getter<TSource, TMember> CreateGetter<TSource, TMember>( Expression<Func<TSource, TMember>> memberExpression )
        {
            // Creates the following lambda: `(instance) => instance.member;`

            if( !(memberExpression.Body is MemberExpression memberExp) )
            {
                throw new ArgumentException( "Expression is not a member access expression" );
            }
            ParameterExpression instance = Expression.Parameter( typeof( TSource ), "instance" );

            MemberExpression memberAccess = Expression.MakeMemberAccess( instance, memberExp.Member );

            UnaryExpression convert = Expression.Convert( memberAccess, typeof( TMember ) );

            return Expression.Lambda<Getter<TSource, TMember>>( convert, instance )
                .Compile();
        }

        /// <summary>
        /// Creates a setter method from the member access expression.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the expression is not a member access.</exception>
        public static Setter<TSource, TMember> CreateSetter<TSource, TMember>( Expression<Func<TSource, TMember>> memberExpression )
        {
            // Creates the following lambda: `(instance, value) => instance.member = value;`
            if( !(memberExpression.Body is MemberExpression memberExp) )
            {
                throw new ArgumentException( "Expression is not a member access expression" );
            }

            ParameterExpression instance = Expression.Parameter( typeof( TSource ), "instance" );
            ParameterExpression value = Expression.Parameter( typeof( TMember ), "value" );

            MemberExpression memberAccess = Expression.MakeMemberAccess( instance, memberExp.Member );

            UnaryExpression convertedValue = Expression.Convert( value, memberExp.Type );

            BinaryExpression assignment = Expression.Assign( memberAccess, convertedValue );

            return Expression.Lambda<Setter<TSource, TMember>>( assignment, instance, value )
                .Compile();
        }

        /// <summary>
        /// Creates a setter method from the member access expression.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the expression is not a member access.</exception>
        public static RefSetter<TSource, TMember> CreateStructSetter<TSource, TMember>( Expression<Func<TSource, TMember>> memberExpression )
        {
            // Creates the following lambda: `(ref TSource instance, TMember value) => instance = value;`
            if( !(memberExpression.Body is MemberExpression memberExp) )
            {
                throw new ArgumentException( "Expression is not a member access expression" );
            }

            // Passing by ref allows member-wise loading of `struct` Source types.
            ParameterExpression instance = Expression.Parameter( typeof( TSource ).MakeByRefType(), "instance" );
            ParameterExpression value = Expression.Parameter( typeof( TMember ), "value" );

            MemberExpression memberAccess = Expression.MakeMemberAccess( instance, memberExp.Member );

            UnaryExpression convertedValue = Expression.Convert( value, memberExp.Type );

            BinaryExpression assignment = Expression.Assign( memberAccess, convertedValue );

            return Expression.Lambda<RefSetter<TSource, TMember>>( assignment, instance, value )
                .Compile();
        }
    }
}