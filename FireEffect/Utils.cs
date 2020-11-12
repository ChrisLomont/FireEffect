using System;
using System.Collections.Generic;

namespace Hypnocube.LargeArtDriver.Model.ImageGenerator.Graphics
{
    public static class Utils
    {

        /// <summary>
        /// compute the nth term in a sub-random sequence in [min,max] given
        /// the initial value. A sub-random sequence covers the space nicely and uniformly,
        /// and looks better for generating colors, for example.
        /// http://en.wikipedia.org/wiki/Low-discrepancy_sequence
        /// </summary>
        /// <param name="startValue"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public static double LowDiscrepancySequence(double startValue, double minValue, double maxValue, int term)
        {
            // 2/(1+sqrt(5)) = 1-phi = 1/phi = 0.61803.... 
            const double goldenMean = 0.618033988749894848204586834366;
            var delta = maxValue - minValue;
            return PositiveMod(goldenMean * term * delta + startValue - minValue, delta) + minValue;
        }


        /// <summary>
        /// for b!=0, return c so that there is an integral r such that 
        /// |a| = r |b| +c and 0 le c lt |b|.
        /// If b==0, return 0
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double PositiveMod(double a, double b)
        {
            if (b == 0) return 0;
            a = Math.Abs(a);
            b = Math.Abs(b);
            var r = Math.Floor(a / b);
            return a - r * b;
        }
        /// <summary>
        /// for b!=0, return c so that there is an integral r such that 
        /// |a| = r |b| +c and 0 le c lt |b|.
        /// If b==0, return 0
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int PositiveMod(int a, int b)
        {
            if (b == 0) return 0;
            a = Math.Abs(a);
            b = Math.Abs(b);
            var r = a / b;
            return a - r * b;
        }

        // given a list of length, want to find where in this list a parameter lies
        // determines the span 0+, the distance into that span, updates the start for repeating
        // return true if wrapped
        public static bool GetPhase(
            double[] times,
            double elapsedTime,
            ref double lastPeriodTime,
            out int phaseIndex,
            out double phasePart
        )
        {
            var retval = false;
            var delta = elapsedTime - lastPeriodTime;

            while (true)
            {
                phaseIndex = 0;
                while (phaseIndex < times.Length && times[phaseIndex] < delta)
                {
                    delta -= times[phaseIndex];
                    ++phaseIndex;
                }
                if (phaseIndex == times.Length)
                { // wrapped
                    retval = true;
                }
                else
                    break;
            }

            if (retval)
                lastPeriodTime = elapsedTime - delta;
            phasePart = delta;
            return retval;
        }

        public static double LinearInterpolate(double a, double b, double value)
        {
            return a * value + b * (1 - value);
        }

        /// <summary>
        /// Enumerate enum values
        /// Usage:
        ///    var values = GetValues<Foos>();
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] GetValues<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        /// <summary>
        ///     Given start and end lattice points in n-dimensional space, walk
        ///     start to destination (inclusive) and choose nearest lattice points,
        ///     similar to line drawing algorithms. Perform given action on each.
        /// </summary>
        /// <param name="startPoint">The start n-dimensional point</param>
        /// <param name="endPoint">The end n-dimensional point</param>
        /// <param name="actionToPerform">The action to perform on each point walked.</param>
        public static void DDAHelper(List<int> startPoint, List<int> endPoint, Action<List<int>> actionToPerform)
        {
            if ((startPoint == null) || (endPoint == null) || (startPoint.Count != endPoint.Count))
                throw new ArgumentException("start and endpoints cannot be null and must be the same length");
            var del = new List<int>(); // absolute deltas * 2, used in error comparisons
            var sgn = new List<int>(); // signs, used to step the coordinates
            var err = new List<int>(); // error counters
            var cur = new List<int>(); // current point
            var length = -1;
            var size = startPoint.Count;
            var k = -1; // index of the largest delta
            for (var i = 0; i < size; ++i)
            {
                var d = endPoint[i] - startPoint[i];
                if (Math.Abs(d) > length)
                {
                    k = i; // save index of longest delta
                    length = Math.Abs(d);
                }
                del.Add(Math.Abs(d) << 1);
                sgn.Add(Math.Sign(d));
                cur.Add(startPoint[i]);
            }

            // create the ei
            var shift = del[k] >> 1;
            for (var i = 0; i < size; ++i)
            {
                var e = -shift;
                if (sgn[i] > 0) e += 1;
                err.Add(e);
            }
            var delK = del[k]; // keep a copy since used so often
            // walk points (length+1 points)
            while (length-- >= 0)
            {
                actionToPerform(cur);
                for (var i = 0; i < size; ++i)
                {
                    err[i] += del[i];
                    if (err[i] > 0)
                    {
                        cur[i] += sgn[i];
                        err[i] -= delK;
                    }
                }
            }
        }

    }
}
