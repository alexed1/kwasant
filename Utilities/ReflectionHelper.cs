using System;
using System.Linq.Expressions;

namespace Utilities
{
    public class ReflectionHelper<TOnType>
    {
        //This creates a statically typed reference to our supplied property. If we change it in the future, it won't compile (so it won't break at runtime).
        //Changing the property with tools like resharper will automatically update here.
        public string GetPropertyName<TReturnType>(Expression<Func<TOnType, TReturnType>> expression)
        {
            if (expression.Body.NodeType == ExpressionType.MemberAccess)
                return (expression.Body as dynamic).Member.Name;

            throw new Exception("Cannot contain complex expressions. An example of a supported expression is 'ev => ev.Id'");
        }
    }
}
