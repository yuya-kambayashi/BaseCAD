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
        public Arc() { }

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

        public override void Draw(Renderer renderer)
        {
            renderer.DrawArc(Style.ApplyLayer(Layer), Center, Radius, StartAngle, EndAngle);
        }

        public override Extents2D GetExtents()
        {
            Extents2D extents = new Extents2D();
            extents.Add(X - Radius, Y - Radius);
            extents.Add(X + Radius, Y + Radius);
            return extents;
        }

        public override void TransformBy(Matrix2D transformation)
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
                new ControlPoint("Center point", Center),
                new ControlPoint("Radius", ControlPoint.ControlPointType.Distance, Center, Center + Radius * Vector2D.FromAngle((StartAngle + EndAngle) / 2)),
                new ControlPoint("Start angle", ControlPoint.ControlPointType.Angle, Center, Center + Radius * Vector2D.FromAngle(StartAngle)),
                new ControlPoint("End angle", ControlPoint.ControlPointType.Angle, Center, Center + Radius * Vector2D.FromAngle(EndAngle)),
            };
        }
        public override void TransformControlPoint(int index, Matrix2D transformation)
        {
            if (index == 0)
                Center = Center.Transform(transformation);
            else if (index == 1)
                Radius = Vector2D.XAxis.Transform(transformation).Length * Radius;
            else if (index == 2)
                StartAngle = Vector2D.FromAngle(StartAngle).Transform(transformation).Angle;
            else if (index == 3)
                EndAngle = Vector2D.FromAngle(EndAngle).Transform(transformation).Angle;
        }
        public override void Load(DocumentReader reader)
        {
            base.Load(reader);
            Center = reader.ReadPoint2D();
            Radius = reader.ReadFloat();
            StartAngle = reader.ReadFloat();
            EndAngle = reader.ReadFloat();
        }

        public override void Save(DocumentWriter writer)
        {
            base.Save(writer);
            writer.Write(Center);
            writer.Write(Radius);
            writer.Write(StartAngle);
            writer.Write(EndAngle);
        }
    }
}
