using System;
using System.Diagnostics;

namespace Hypnocube.LargeArtDriver.Model.ImageGenerator.Graphics
{
    public static class HslUtils
    {
        #region HSLV

        /// <summary>
        /// Convert Hue,Saturation,Luminance (HSL) 
        /// or Hue,Saturation,Value (HSV) to 
        /// Red,Green,Blue (RGB) in 0,1.
        /// If Hue were in [0,360): 
        ///    red    = 0
        ///    violet = 60
        ///    blue   = 120
        ///    cyan   = 180
        ///    green  = 240
        ///    yellow = 300
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="lv"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="useValue"></param>
        public static void HslvToRgb(double h, double s, double lv, out double r, out double g, out double b,
            bool useValue)
        {
            // make sure h in [0,1)
            h -= Math.Floor(h);

            if (Math.Abs(s) < 0.000001)
            {
                r = g = b = lv; // achromatic
            }
            else
            {

                double c, m;
                if (useValue)
                {
                    c = lv * s;
                    m = lv - c;
                }
                else
                {
                    c = (1 - Math.Abs(2 * lv - 1)) * s; // chroma
                    m = lv - c * 0.5;
                }
                double hp = 6 * h;
                double ab = (hp / 2 - Math.Floor(hp / 2)) * 2; // hp mod 2
                double x = c * (1 - Math.Abs(ab - 1));
                r = g = b = 0;
                if (hp < 1)
                {
                    r = c;
                    g = x;
                }
                else if (hp < 2)
                {
                    r = x;
                    g = c;
                }
                else if (hp < 3)
                {
                    g = c;
                    b = x;
                }
                else if (hp < 4)
                {
                    g = x;
                    b = c;
                }
                else if (hp < 5)
                {
                    b = c;
                    r = x;
                }
                else if (hp < 6)
                {
                    b = x;
                    r = c;
                }
                r += m;
                g += m;
                b += m;


                double tolerance = 0.005;
                Trace.Assert(0 - tolerance <= r && r <= 1.0 + tolerance);
                Trace.Assert(0 - tolerance <= g && g <= 1.0 + tolerance);
                Trace.Assert(0 - tolerance <= b && b <= 1.0 + tolerance);
                r = Clamp01(r, tolerance);
                g = Clamp01(g, tolerance);
                b = Clamp01(b, tolerance);
            }
        }

        public static void HslToRgb(double h, double s, double l, out double r, out double g, out double b)
        {
            HslvToRgb(h, s, l, out r, out g, out b, false);
        }

        public static void RgbToHsl(double r, double g, double b, out double h, out double s, out double l)
        {
            double temp, temp2;
            RgbToHslv(r, g, b, out h, out s, out temp, out l, out temp2);
        }

        /// <summary>
        /// Given RGB in [0,1], compute HSLV each in [0,1]
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="h"></param>
        /// <param name="sHsl"></param>
        /// <param name="sHsv"></param>
        /// <param name="l"></param>
        /// <param name="v"></param>
        public static void RgbToHslv(
            double r, double g, double b,
            out double h,
            out double sHsl, out double sHsv,
            out double l, out double v)
        {
            Trace.Assert(0 <= r && r <= 1);
            Trace.Assert(0 <= g && g <= 1);
            Trace.Assert(0 <= b && b <= 1);
            const double tolerance = 0.00001;

            double max, min; // max, min of color components
            if (r > g)
            {
                max = Math.Max(r, b);
                min = Math.Min(g, b);
            }
            else
            {
                max = Math.Max(g, b);
                min = Math.Min(r, b);
            }
            double c = max - min; // chroma in [0,1]

            Trace.Assert(0 <= c && c <= 1);

            h = 0; // default case if c == 0

            if (Math.Abs(c) > tolerance)
            {
                if (Math.Abs(max - r) < tolerance)
                {
                    h = (g - b) / c; // h = (R-G)/C + 4 in [4,5]
                    if (h < 0)
                        h += 6;
                }
                else if (Math.Abs(max - g) < tolerance)
                    h = (b - r) / c + 2; // h = (B-R)/C + 2 in [1,3]
                else if (Math.Abs(max - b) < tolerance)
                    h = (r - g) / c + 4; // h = (R-G)/C + 4 in [4,5]
            }
            h /= 6.0;
            if (h >= 1) h -= 1;
            Trace.Assert(0 <= h && h < 1);
            v = max;
            l = (min + max) / 2.0;

            Trace.Assert(0 <= v && v <= 1);
            Trace.Assert(0 <= l && l <= 1);

            sHsv = 0; // default if v = 0
            if (Math.Abs(v) > tolerance)
                sHsv = Clamp01(c / v); // sHsv = C/V with C in [0,m1] and V in [0,m2], answer in [0,m2]

            sHsl = 0; // default if l = 0 or l = m2
            if (tolerance < l && l < 1 - tolerance)
                sHsl = Clamp01(c / (1 - Math.Abs(2 * l - 1)));

            Trace.Assert(0 <= sHsl && sHsl <= 1);
            Trace.Assert(0 <= sHsv && sHsv <= 1);
        }

        /// <summary>
        /// Clamp value into [0,1]
        /// </summary>
        /// <param name="value"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static double Clamp01(double value, double tolerance = 0.00001)
        {
            if (0 <= value && value <= 1) return value;
            if (value < 0 && -tolerance < value) return 0;
            if (value > 1 && 1 + tolerance > value) return 1;

            Trace.TraceError("Clamp01 color parse error");
            throw new Exception("Value out of tolerance");
        }

        /// <summary>
        /// Wrap value into [0,1)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double Wrap01(double value)
        {
            // make sure h in [0,1)
            return value - Math.Floor(value);
        }

        #endregion



        // convert color in [0,1] t0 [0,255]
        public static byte Upscale(double realColor)
        {
            realColor = Clamp01(realColor); // snap ends to [0,1]

            // 256 bins to fall into, each 1/256 wide, bin # thus Floor(v/(1/256))
            // 0 index by subtracting 1
            var bin = Math.Floor(realColor * 256) - 1;
            if (bin < 0) bin = 0;
            if (bin > 255) bin = 255; // not possible?
            return (byte)bin;
        }

       /* public static double HslColorDistance(HslColor color1, HslColor color2)
        {
            var dh = color1.Hue - color2.Hue;
            var ds = color1.Saturation - color2.Saturation;
            var dl = color1.Lightness - color2.Lightness;

            // hue - pick smaller way around
            dh = Math.Abs(dh);
            if (dh > 0.5)
                dh = 1.0 - dh;

            return Math.Sqrt(dh * dh + ds * ds + dl * dl);
        } */

        /// <summary>
        /// Return a hue in [0,1)
        /// </summary>
        /// <param name="hue"></param>
        /// <returns></returns>
        public static double Normalize(double hue)
        {
            return hue - Math.Floor(hue);
        }

        /// <summary>
        /// Normalize hueMin to [0,1), and normalize hueMax to smallest 
        /// equivalent hue strictly greater than hueMin.
        /// </summary>
        /// <param name="hueMin"></param>
        /// <param name="hueMax"></param>
        /// <returns></returns>
        public static void Normalize(ref double hueMin, ref double hueMax)
        {
            const double tolerance = 0.0001;
            var delta = hueMax - hueMin;
            hueMin = Normalize(hueMin);
            hueMax = Normalize(hueMax);
            if (hueMax <= hueMin + tolerance)
                hueMax += 1.0;
            if (Math.Abs(hueMax - hueMin) < tolerance && Math.Abs(delta - 1.0) < tolerance)
                hueMax += 1.0; // restore this spacing property
        }

        /// <summary>
        /// Normalize hueMin to [0,1), and normalize hueMax to smallest 
        /// equivalent hue strictly greater than hueMin. Finally normalize
        /// hueMid to between hueMin and hueMax, inclusive.
        /// If hueMid cannot be placed there, throw exception
        /// </summary>
        /// <returns></returns>
        public static void Normalize(ref double hueMin, ref double hueMid, ref double hueMax)
        {
            Normalize(ref hueMin, ref hueMax);
            hueMid = Normalize(hueMid);
            if (hueMid < hueMin)
                hueMid += 1.0;
            if (hueMax < hueMid)
            {
                Trace.TraceError("hue normalize error");
                throw new Exception("HueTools middle out of range");
            }
        }

        /// <summary>
        /// Compute min distance between two hues, allowing wrapping
        /// Always nonnegative
        /// </summary>
        /// <param name="hue1"></param>
        /// <param name="hue2"></param>
        /// <returns></returns>
        public static double HueDistance(double hue1, double hue2)
        {
            hue1 = Normalize(hue1);
            hue2 = Normalize(hue2);
            var dh = Math.Abs(hue1 - hue2);
            if (dh > 0.5) // other direction shorter
                dh = 1.0 - dh;
            return dh;
        }

        /// <summary>
        /// Return true if hue is in [min,max)
        /// </summary>
        /// <param name="hueMin"></param>
        /// <param name="hueMax"></param>
        /// <param name="hue"></param>
        /// <returns></returns>
        public static bool HueContained(double hueMin, double hueMax, double hue)
        {
            Normalize(ref hueMin, ref hueMax);
            hue = Normalize(hue);
            if (hue < hueMin)
            { // NOTE: wrapping hue up one fails due to numerical instabilities
                hueMax -= 1.0;
                hueMin -= 1.0;
                //hue += 1.0; // wrap up one
            }
            return hueMin <= hue && hue < hueMax;
        }


        // gamma correction table from our HypnoLight LED strand
        // mathematica code f[g_] := Table[Round[255 (n/255)^g], {n, 0, 255}]; f[2.8]
        static byte[] gammaCorrectionTable = {
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  1,  1,  1,  1,
            1,  1,  1,  1,  1,  1,  1,  1,  1,  2,  2,  2,  2,  2,  2,  2,
            2,  3,  3,  3,  3,  3,  3,  3,  4,  4,  4,  4,  4,  5,  5,  5,
            5,  6,  6,  6,  6,  7,  7,  7,  7,  8,  8,  8,  9,  9,  9, 10,
            10, 10, 11, 11, 11, 12, 12, 13, 13, 13, 14, 14, 15, 15, 16, 16,
            17, 17, 18, 18, 19, 19, 20, 20, 21, 21, 22, 22, 23, 24, 24, 25,
            25, 26, 27, 27, 28, 29, 29, 30, 31, 32, 32, 33, 34, 35, 35, 36,
            37, 38, 39, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 50,
            51, 52, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 66, 67, 68,
            69, 70, 72, 73, 74, 75, 77, 78, 79, 81, 82, 83, 85, 86, 87, 89,
            90, 92, 93, 95, 96, 98, 99,101,102,104,105,107,109,110,112,114,
            115,117,119,120,122,124,126,127,129,131,133,135,137,138,140,142,
            144,146,148,150,152,154,156,158,160,162,164,167,169,171,173,175,
            177,180,182,184,186,189,191,193,196,198,200,203,205,208,210,213,
            215,218,220,223,225,228,231,233,236,239,241,244,247,249,252,255 };


        static readonly ushort[] HueScaleTable/*[HSLMAX + 1]*/ = {
            0, 2, 4, 5, 7, 9, 10, 12, 13, 15, 16, 18, 19, 21, 22, 23, 25, 26, 27,
            29, 30, 31, 32, 34, 35, 36, 37, 38, 40, 41, 42, 43, 44, 45, 46, 47,
            48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 58, 59, 60, 61, 62, 63,
            63, 64, 65, 66, 67, 67, 68, 69, 70, 70, 71, 72, 73, 73, 74, 75, 75,
            76, 77, 77, 78, 79, 79, 80, 80, 81, 82, 82, 83, 83, 84, 85, 85, 86,
            86, 87, 87, 88, 89, 89, 90, 90, 91, 92, 92, 93, 94, 94, 95, 96, 96,
            97, 98, 98, 99, 100, 101, 101, 102, 103, 104, 104, 105, 106, 107,
            107, 108, 109, 110, 111, 112, 113, 113, 114, 115, 116, 117, 118, 119,
            120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 133, 134,
            135, 136, 137, 139, 140, 141, 142, 144, 145, 146, 148, 149, 150, 152,
            153, 155, 156, 158, 159, 161, 163, 164, 166, 168, 169, 171, 173, 175,
            176, 178, 180, 181, 183, 184, 186, 187, 189, 190, 192, 193, 195, 196,
            197, 199, 200, 201, 202, 204, 205, 206, 207, 208, 209, 211, 212, 213,
            214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 224, 225, 226, 227,
            228, 228, 229, 230, 231, 232, 233, 234, 234, 235, 236, 237, 238, 238,
            239, 240, 241, 241, 242, 243, 243, 244, 245, 246, 246, 247, 248, 248,
            249, 249, 250, 251, 251, 252, 253, 253, 254, 254, 255, 255, 256, 257,
            257, 258, 258, 259, 259, 260, 261, 261, 262, 263, 263, 264, 264, 265,
            266, 266, 267, 268, 269, 269, 270, 271, 271, 272, 273, 274, 274, 275,
            276, 277, 278, 278, 279, 280, 281, 282, 283, 284, 284, 285, 286, 287,
            288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 298, 299, 300, 301,
            303, 304, 305, 306, 307, 308, 310, 311, 312, 313, 315, 316, 317, 319,
            320, 322, 323, 325, 326, 328, 329, 331, 332, 334, 336, 337, 339, 341,
            343, 344, 346, 348, 349, 351, 353, 354, 356, 357, 359, 360, 362, 363,
            364, 366, 367, 368, 370, 371, 372, 373, 375, 376, 377, 378, 379, 381,
            382, 383, 384, 385, 386, 387, 388, 389, 390, 391, 392, 393, 394, 395,
            396, 397, 398, 399, 399, 400, 401, 402, 403, 404, 405, 405, 406, 407,
            408, 408, 409, 410, 411, 411, 412, 413, 414, 414, 415, 416, 416, 417,
            418, 418, 419, 420, 420, 421, 422, 422, 423, 423, 424, 425, 425, 426,
            426, 427, 427, 428, 429, 429, 430, 430, 431, 432, 432, 433, 433, 434,
            435, 435, 436, 437, 437, 438, 439, 439, 440, 441, 442, 442, 443, 444,
            445, 445, 446, 447, 448, 449, 449, 450, 451, 452, 453, 454, 454, 455,
            456, 457, 458, 459, 460, 461, 462, 463, 464, 465, 466, 467, 468, 469,
            470, 471, 472, 474, 475, 476, 477, 478, 480, 481, 482, 483, 485, 486,
            487, 489, 490, 491, 493, 494, 496, 497, 499, 500, 502, 503, 505, 507,
            508, 510};

        /*

            Scaling: from our mathematica analysis, we took a parabola, rotated it, and
            used it to scale. The result is a function f(x,h) with stretch parameter h, which is

            f[x_, h_] := (-(1/Sqrt[2]) + 2 h - 2 h x + Sqrt[1/2 - 2 Sqrt[2] h + 4 h^2 + 4 Sqrt[2] h x])/(2 h)

            h can be in 0 to 0.35 or so.

            Then we map a hue in [0,1) to [0,1) via

            scale[hue_, t_] := Module[{ht, hi, hf, ha},
              hf = FractionalPart[hue*6];
              hi = Floor[hue*6 - hf];
              hi = Mod[hi, 6];
              ha = If[OddQ[hi], 1 + hi - f[1 - hf, t], hi + f[hf, t]];
              Return[ha/6]
              ]

            for Christmas lights we used h=0.10.

        */

        /* remap hue using a nice scaling, which pulls colors out a little more richly */
        public static double ScaleHue(double hue)
        {
            hue = Utils.PositiveMod(hue, 1.0);

            var hi = (int)(hue * HueScaleTable.Length);
            hi %= HueScaleTable.Length;
            return HueScaleTable[hi] / 512.0;
        }

    }
}
