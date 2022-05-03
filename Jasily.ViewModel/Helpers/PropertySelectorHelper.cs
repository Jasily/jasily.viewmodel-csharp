using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Jasily.ViewModel.Helpers
{
    public static class PropertySelectorHelper
    {
        /// <summary>
        /// Gets property names from a <see cref="Expression"/>.
        /// 
        /// For x => x.A.B, return "A", "B".
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static IEnumerable<string> GetPropertyNames<TModel>(Expression<Func<TModel, object>> propertySelector)
        {
            if (propertySelector is null)
            {
                throw new ArgumentNullException(nameof(propertySelector));
            }

            var names = new List<string>();
            VisitPropertyAccess(propertySelector.Body, names);
            names.Reverse();
            return names;

            void VisitPropertyAccess(Expression e, List<string> names)
            {
                while (e != null)
                {
                    switch (e.NodeType)
                    {
                        case ExpressionType.MemberAccess:
                            var me = (MemberExpression)e;
                            if (me.Member.MemberType != MemberTypes.Property)
                                throw new ArgumentException($"{propertySelector} is not property access expression.");
                            names.Add(me.Member.Name);
                            e = me.Expression;
                            break;

                        case ExpressionType.Convert:
                            e = ((UnaryExpression)e).Operand;
                            break;

                        case ExpressionType.Parameter:
                            return;

                        default:
                            throw new ArgumentException($"{propertySelector} is not property access expression.");
                    }
                }
            }
        }
    }
}
