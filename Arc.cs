﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCAD
{
    public class Arc : Drawable
    {
        public Point2D Center { get; set; }
        public float Radius { get; set; }
        public float StartAngle { get; set; }
        public float EndAngle { get; set; }

        [Browsable(false)]
        public float X { get { return Center.X; } }
        [Browsable(false)]
        public float Y { get { return Center.Y; } }

        public Arc(Point2D center, float radius, float startAngle, float endAngle)
        {
            Center = center;
            Radius = radius;
            StartAngle = startAngle;
            EndAngle = endAngle;
        }

        public Arc(float x, float y, float radius, float startAngle, float endAngle)
            : this(new Point2D(x, y), radius, startAngle, endAngle)
        {
            ;
        }

        public override void Draw(DrawParams param)
        {
            using (Pen pen = OutlineStyle.CreatePen(param))
            {
                // Represent curved features by at most 4 pixels
                float curveLength = param.ModelToView(Math.Abs(EndAngle - StartAngle) * Radius);
                int n = (int)Math.Max(4, curveLength / 4);
                float a = StartAngle;
                float da = (EndAngle - StartAngle) / (float)n;
                PointF[] pts = new PointF[n + 1];
                for (int i = 0; i <= n; i++)
                {
                    pts[i] = new PointF(X + Radius * (float)Math.Cos(a), Y + Radius * (float)Math.Sin(a));
                    a += da;
                }
                param.Graphics.DrawLines(pen, pts);
            }
        }

        public override Extents GetExtents()
        {
            Extents extents = new Extents();
            extents.Add(X - Radius, Y - Radius);
            extents.Add(X + Radius, Y + Radius);
            return extents;
        }

        public override void TransformBy(TransformationMatrix2D transformation)
        {
            Point2D p = Center;
            p.TransformBy(transformation);
            Center = p;

            Vector2D dir = Vector2D.XAxis * Radius;
            dir.TransformBy(transformation);
            Radius = dir.Length;

            Vector2D a1 = Vector2D.FromAngle(StartAngle);
            Vector2D a2 = Vector2D.FromAngle(EndAngle);
            a1.TransformBy(transformation);
            a2.TransformBy(transformation);
            StartAngle = a1.Angle;
            EndAngle = a2.Angle;
        }

        public override bool Contains(Point2D pt, float pickBoxSize)
        {
            Vector2D dir = pt - Center;
            float dist = dir.Length;
            if (dist > Radius + pickBoxSize / 2 || dist < Radius - pickBoxSize / 2)
            {
                return false;
            }
            float ang = dir.Angle;
            if (StartAngle < EndAngle)
            {
                return ang >= StartAngle && ang <= EndAngle;
            }
            else
            {
                return ang >= EndAngle && ang <= StartAngle;
            }
        }
    }
}
