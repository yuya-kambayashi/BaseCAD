﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCAD.Geometry
{
    [TypeConverter(typeof(Vector2DConverter))]
    public struct Vector2D : IPersistable
    {
        private readonly float _x;
        private readonly float _y;
        public float X { get { return _x; } }
        public float Y { get { return _y; } }
        public float Length { get { return MathF.Sqrt(X * X + Y * Y); } }
        public float Angle { get { return AngleTo(XAxis); } }
        public Vector2D Normal { get { float len = Length; return new Vector2D(X / len, Y / len); } }
        public Vector2D Perpendicular { get { return new Vector2D(-Y, X); } }
        public static Vector2D Zero { get { return new Vector2D(0, 0); } }
        public static Vector2D XAxis { get { return new Vector2D(1, 0); } }
        public static Vector2D YAxis { get { return new Vector2D(0, 1); } }

        public Vector2D(float x, float y)
        {
            _x = x;
            _y = y;
        }

        public Vector2D Transform(Matrix2D transformation)
        {
            float x = transformation.M11 * X + transformation.M12 * Y;
            float y = transformation.M21 * X + transformation.M22 * Y;
            return new Vector2D(x, y);
        }
        public float DotProduct(Vector2D v)
        {
            return X * v.X + Y * v.Y;
        }

        public float CrossProduct(Vector2D v)
        {
            return X * v.Y - Y * v.X;
        }

        public float AngleTo(Vector2D v)
        {
            return ClampAngle(SignedAngleTo(v));
        }

        public bool IsBetween(Vector2D a, Vector2D b)
        {
            float ang = ClampAngle(b.SignedAngleTo(a), true, false);
            float ang1 = ClampAngle(this.SignedAngleTo(a), true, false);
            float ang2 = ClampAngle(b.SignedAngleTo(this), true, false);

            return Math.Abs(ang2 + ang1 - ang) < 0.0001f;
        }

        private float SignedAngleTo(Vector2D v)
        {
            float dot = this.DotProduct(v);
            float det = X * v.Y - v.X * Y;
            float ang = -MathF.Atan2(det, dot);
            return ang;
        }

        private static float ClampAngle(float ang)
        {
            return ClampAngle(ang, true, true);
        }

        private static float ClampAngle(float ang, bool low, bool high)
        {
            if (low) { while (ang < 0) ang += 2 * MathF.PI; }
            if (high) { while (ang > 2 * MathF.PI) ang -= 2 * MathF.PI; }
            return ang;
        }

        public static Vector2D FromAngle(float angle)
        {
            return new Vector2D(MathF.Cos(angle), MathF.Sin(angle));
        }
        public static Vector2D operator +(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2D operator -(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2D operator *(Vector2D p, float f)
        {
            return new Vector2D(p.X * f, p.Y * f);
        }

        public static Vector2D operator *(float f, Vector2D p)
        {
            return new Vector2D(p.X * f, p.Y * f);
        }

        public static Vector2D operator /(Vector2D p, float f)
        {
            return new Vector2D(p.X / f, p.Y / f);
        }

        public static explicit operator System.Drawing.SizeF(Vector2D a)
        {
            return new System.Drawing.SizeF(a.X, a.Y);
        }

        public Point2D AsPoint2D()
        {
            return new Point2D(X, Y);
        }
        public string ToString(IFormatProvider provider)
        {
            return ToString("({0:F}, {1:F})", provider);
        }
        public string ToString(string format = "({0:F}, {1:F})", IFormatProvider provider = null)
        {
            return (provider == null) ?
                string.Format(format, X, Y) :
                string.Format(provider, format, X, Y);
        }
        public static bool TryParse(string s, out Vector2D result)
        {
            Vector2DConverter conv = new Vector2DConverter();
            if (conv.IsValid(s))
            {
                result = (Vector2D)conv.ConvertFrom(s);
                return true;
            }
            else
            {
                result = Vector2D.Zero;
                return false;
            }
        }
        public Vector2D(BinaryReader reader)
        {
            _x = reader.ReadSingle();
            _y = reader.ReadSingle();
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(_x);
            writer.Write(_y);
        }
    }
}
