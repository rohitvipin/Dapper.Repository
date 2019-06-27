using System;
using System.Collections.Generic;

namespace CompBioAnalyticsApi.DataAccess.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Slices the given array into sub arrays
        /// Converts a 1-D array to 2-D array
        /// </summary>
        /// <param name="input"></param>
        /// <param name="segmentSize"></param>
        /// <returns></returns>
        public static IEnumerable<IList<T>> Slice<T>(this T[] input, int segmentSize)
        {
            var segments = new List<IList<T>>();

            if (input.Length <= segmentSize || segmentSize == 0)
            {
                segments.Add(new ArraySegment<T>(input));
                return segments;
            }

            var overflow = input.Length % segmentSize;

            if (overflow > 0)
            {
                segments.Add(new ArraySegment<T>(input, 0, overflow));
            }

            for (var i = overflow; i < input.Length; i += segmentSize)
            {
                segments.Add(new ArraySegment<T>(input, i, segmentSize));
            }

            return segments;
        }
    }
}