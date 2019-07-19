using System;
using System.Collections.Generic;
using System.Text;

namespace FracVisualisationSoftware.Misc.Extensions
{
    public static class MathExtensionMethods
    {
        /// <summary>
        /// Makes sure the value stays between the min and max.
        /// If trying to clamp a negative value, make sure that the negative value is properly defined as negative first.
        /// Example: Rather than -5.Clamp(0,5); put (-5).Clamp(0,5);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            else if (value.CompareTo(max) > 0) return max;
            else return value;
        }

        public static bool IsNumeric<T>(this T value)
        {
            return value is sbyte
                   || value is byte
                   || value is short
                   || value is ushort
                   || value is int
                   || value is uint
                   || value is long
                   || value is ulong
                   || value is float
                   || value is double
                   || value is decimal;
        }
    }
}
