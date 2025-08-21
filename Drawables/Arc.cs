﻿using BaseCAD.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCAD.Drawables
{
    public class Arc : Drawable
    {
        private Point2D center;
        private float radius;
        private float startAngle;
        private float endAngle;

        public Point2D Center { get => center; set { center = value; NotifyPropertyChanged(); } }
        public float Radius { get => radius; set { radius = value; NotifyPropertyChanged(); } }
        public float StartAngle { get => startAngle; set { startAngle = value; NotifyPropertyChanged(); } }
        public float EndAngle
        {
            get => endAngle; set { endAngle = value; NotifyPropertyChanged(); }
        }

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

        public override void Draw(Graphics param)
        {
            using (Pen pen = Style.CreatePen(param))
            {
                // Represent curved features by at most 4 pixels
                float sweep = EndAngle - StartAngle;
                while (sweep < 0) sweep += 2 * MathF.PI;
                while (sweep > 2 * MathF.PI) sweep -= 2 * MathF.PI;
                float curveLength = param.ModelToView(sweep * Radius);
                int n = (int)Math.Max(4, curveLength / 4);
                float a = StartAngle;
                float da = sweep / n;
                PointF[] pts = new PointF[n + 1];
                for (int i = 0; i <= n; i++)
                {
                    pts[i] = new PointF(X + Radius * MathF.Cos(a), Y + Radius * MathF.Sin(a));
                    a += da;
                }
                param.Graphics.DrawLines(pen, pts);
            }
        }

        public override Extents2D GetExtents()
        {
            Extents2D extents = new Extents2D();
            extents.Add(X - Radius, Y - Radius);
            extents.Add(X + Radius, Y + Radius);
            return extents;
        }

        public override void TransformBy(TransformationMatrix2D transformation)
        {
            Center = Center.Transform(transformation);

            Radius = (Vector2D.XAxis * Radius).Transform(transformation).Length;
            StartAngle = Vector2D.FromAngle(StartAngle).Transform(transformation).Angle;
            EndAngle = Vector2D.FromAngle(EndAngle).Transform(transformation).Angle;
        }

        public override bool Contains(Point2D pt, float pickBoxSize)
        {
            Vector2D dir = pt - Center;
            float dist = dir.Length;
            return dist >= Radius - pickBoxSize / 2 && dist <= Radius + pickBoxSize / 2 &&
                dir.IsBetween(Vector2D.FromAngle(StartAngle), Vector2D.FromAngle(EndAngle));
        }
        public override ControlPoint[] GetControlPoints()
        {
            return new[]
            {
                new ControlPoint("Center"),
                new ControlPoint("Radius", ControlPoint.ControlPointType.Distance, Center, Center + Radius * Vector2D.FromAngle((StartAngle + EndAngle) / 2)),
                new ControlPoint("StartAngle", ControlPoint.ControlPointType.Angle, Center, Center + Radius * Vector2D.FromAngle(StartAngle)),
                new ControlPoint("EndAngle", ControlPoint.ControlPointType.Angle, Center, Center + Radius * Vector2D.FromAngle(EndAngle)),
            };
        }
        public Arc(BinaryReader reader) : base(reader)
        {
            Center = new Point2D(reader);
            Radius = reader.ReadSingle();
            StartAngle = reader.ReadSingle();
            EndAngle = reader.ReadSingle();
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            Center.Save(writer);
            writer.Write(Radius);
            writer.Write(StartAngle);
            writer.Write(EndAngle);
        }
    }
}
