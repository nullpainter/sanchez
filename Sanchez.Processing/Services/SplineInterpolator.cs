using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanchez.Processing.Services
{
    /// <summary>
    ///     Spline interpolation, from <see cref="https://gist.github.com/dreikanter/3526685"/>
    /// </summary>
    public class SplineInterpolator
    {
        private readonly float[] _a;

        private readonly float[] _h;
        private readonly float[] _keys;

        private readonly float[] _values;

        /// <param name="nodes">
        ///     Collection of known points for further interpolation.
        ///     Should contain at least two items.
        /// </param>
        public SplineInterpolator(IDictionary<float, float> nodes)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));

            var n = nodes.Count;

            if (n < 2) throw new ArgumentException("At least two point required for interpolation.");

            _keys = nodes.Keys.ToArray();
            _values = nodes.Values.ToArray();
            _a = new float[n];
            _h = new float[n];

            for (var i = 1; i < n; i++) _h[i] = _keys[i] - _keys[i - 1];

            if (n <= 2) return;
            
            var sub = new float[n - 1];
            var diag = new float[n - 1];
            var sup = new float[n - 1];

            for (var i = 1; i <= n - 2; i++)
            {
                diag[i] = (_h[i] + _h[i + 1]) / 3;
                sup[i] = _h[i + 1] / 6;
                sub[i] = _h[i] / 6;
                _a[i] = (_values[i + 1] - _values[i]) / _h[i + 1] - (_values[i] - _values[i - 1]) / _h[i];
            }

            SolveTridiag(sub, diag, sup, ref _a, n - 2);
        }

        /// <summary>
        ///     Gets interpolated value for specified argument.
        /// </summary>
        /// <param name="key">
        ///     Argument value for interpolation. Must be within
        ///     the interval bounded by lowest ang highest <see cref="_keys" /> values.
        /// </param>
        public float GetValue(float key)
        {
            var gap = 0;
            var previous = float.MinValue;

            // At the end of this iteration, "gap" will contain the index of the interval
            // between two known values, which contains the unknown z, and "previous" will
            // contain the biggest z value among the known samples, left of the unknown z
            for (var i = 0; i < _keys.Length; i++)
                if (_keys[i] < key && _keys[i] > previous)
                {
                    previous = _keys[i];
                    gap = i + 1;
                }

            var x1 = key - previous;
            var x2 = _h[gap] - x1;

            return ((-_a[gap - 1] / 6 * (x2 + _h[gap]) * x1 + _values[gap - 1]) * x2 +
                    (-_a[gap] / 6 * (x1 + _h[gap]) * x2 + _values[gap]) * x1) / _h[gap];
        }


        /// <summary>
        ///     Solve linear system with tridiagonal n*n matrix "a"
        ///     using Gaussian elimination without pivoting.
        /// </summary>
        private static void SolveTridiag(float[] sub, float[] diag, float[] sup, ref float[] b, int n)
        {
            int i;

            for (i = 2; i <= n; i++)
            {
                sub[i] = sub[i] / diag[i - 1];
                diag[i] = diag[i] - sub[i] * sup[i - 1];
                b[i] = b[i] - sub[i] * b[i - 1];
            }

            b[n] = b[n] / diag[n];

            for (i = n - 1; i >= 1; i--) b[i] = (b[i] - sup[i] * b[i + 1]) / diag[i];
        }
    }
}