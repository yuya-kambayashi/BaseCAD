﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCAD
{
    internal static class MathF
    {
        public const float Epsilon = 1e-7f;
        public const float PI = 3.14159265358979323846264338327950288419716939937510582f;

        public static bool IsZero(float value)
        {
            return value > -Epsilon && value < Epsilon;
        }

        public static bool IsEqual(float a, float b)
        {
            return IsZero(a - b);
        }

        public static float Sin(float a)
        {
            return (float)Math.Sin(a);
        }

        public static float Cos(float a)
        {
            return (float)Math.Cos(a);
        }

        public static float Asin(float a)
        {
            return (float)Math.Asin(a);
        }

        public static float Acos(float a)
        {
            return (float)Math.Acos(a);
        }

        public static float Atan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }

        public static float Sqrt(float a)
        {
            return (float)Math.Sqrt(a);
        }
        public static float Log(float a, float newBase)
        {
            return (float)Math.Log(a, newBase);
        }

        public static float Pow(float a, float power)
        {
            return (float)Math.Pow(a, power);
        }

        public static float Floor(float a)
        {
            return (float)Math.Floor(a);
        }

        public static float Ceiling(float a)
        {
            return (float)Math.Ceiling(a);
        }
    }
}
