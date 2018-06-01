using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public static class GenericTestExtensionMethods
    {
        public static bool Any<T>(this IEnumerable<T> self, Func<T,int,bool> func)
        {
            return self.Where(func).Any();
        }

        public static bool AnyParamReverse<T>(this IEnumerable<T> self, Func<T, T, bool> func)
        {
            return self.Any(t => func(t,t));
        }

        public static bool Lambda<T>(this T self, Func<T, bool> func)
        {
            return func(self);
        }
    }
}