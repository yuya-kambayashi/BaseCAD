﻿using BaseCAD.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BaseCAD.Drawables
{
    public class Ellipse : Drawable
    {
        private Point2D center;

        public Point2D Center { get => center; set { center = value; UpdatePolyline(); NotifyPropertyChanged(); } }
        [Browsable(false)]
        public float X { get { return Center.X; } }
        [Browsable(false)]
        public float Y { get { return Center.Y; } }

        private float semiMajorAxis;
        private float semiMinorAxis;
        private float rotation;

        public float SemiMajorAxis { get => semiMajorAxis; set { semiMajorAxis = value; UpdatePolyline(); NotifyPropertyChanged(); } }
        public float SemiMinorAxis { get => semiMinorAxis; set { semiMinorAxis = value; UpdatePolyline(); NotifyPropertyChanged(); } }
        public float Rotation { get => rotation; set { rotation = value; UpdatePolyline(); NotifyPropertyChanged(); } }

        private Polyline poly;
        private float curveLength = 4;
        private float cpSize = 0;
        
        public Ellipse(Point2D center, float semiMajor, float semiMinor, float rotation = 0)
        {
            Center = center;
            SemiMajorAxis = semiMajor;
            SemiMinorAxis = semiMinor;
            Rotation = rotation;
            UpdatePolyline();
        }
        public Ellipse(float x, float y, float semiMajor, float semiMinor, float rotation = 0)
            : this(new Point2D(x, y), semiMajor, semiMinor, rotation)
        {
            ;
        }

        private void UpdatePolyline()
        {
            poly = new Polyline();
            // Represent curved features by at most 4 pixels
            int n = (int)Math.Max(4, curveLength / 4);
            float da = 2 * MathF.PI / n;
            float a = 0;
            for (int i = 0; i < n; i++)
            {
                float x = SemiMajorAxis * MathF.Cos(a);
                float y = SemiMinorAxis * MathF.Sin(a);
                poly.Points.Add(x, y);
                a += da;
            }
            poly.Closed = true;
            poly.TransformBy(Matrix2D.Rotation(Rotation));
            poly.TransformBy(Matrix2D.Translation(Center.X, Center.Y));
        }
        public override void Draw(Renderer renderer)
        {
            // Approximate perimeter (Ramanujan)
            float p = 2 * MathF.PI * (3 * (SemiMajorAxis + SemiMinorAxis) - MathF.Sqrt((3 * SemiMajorAxis + SemiMinorAxis) * (SemiMajorAxis + 3 * SemiMinorAxis)));
            float newCurveLength = renderer.View.WorldToScreen(new Vector2D(p, 0)).X;
            if (!MathF.IsEqual(newCurveLength, curveLength))
            {
                curveLength = newCurveLength;
                UpdatePolyline();
            }
            poly.Style = Style;
            renderer.Draw(poly);
        }

        public override Extents2D GetExtents()
        {
            return poly.GetExtents();
        }

        public override void TransformBy(Matrix2D transformation)
        {
            Center = Center.Transform(transformation);
            Rotation = Vector2D.FromAngle(Rotation).Transform(transformation).Angle;
            SemiMajorAxis = (Vector2D.XAxis * SemiMajorAxis).Transform(transformation).Length;
            SemiMinorAxis = (Vector2D.XAxis * SemiMinorAxis).Transform(transformation).Length;
        }

        public override bool Contains(Point2D pt, float pickBoxSize)
        {
            return poly.Contains(pt, pickBoxSize);
        }
        public override ControlPoint[] GetControlPoints()
        {
            return new[]
            {
                new ControlPoint("Center"),
                new ControlPoint("SemiMajorAxis", ControlPoint.ControlPointType.Distance, Center, Center + SemiMajorAxis * Vector2D.FromAngle(Rotation)),
                new ControlPoint("SemiMinorAxis", ControlPoint.ControlPointType.Distance, Center, Center + SemiMinorAxis * Vector2D.FromAngle(Rotation).Perpendicular),
                new ControlPoint("Rotation", ControlPoint.ControlPointType.Angle, Center, Center + (SemiMajorAxis + cpSize) * Vector2D.FromAngle(Rotation)),
            };
        }
        public Ellipse(BinaryReader reader) : base(reader)
        {
            Center = new Point2D(reader);
            SemiMajorAxis = reader.ReadSingle();
            SemiMinorAxis = reader.ReadSingle();
            Rotation = reader.ReadSingle();
            UpdatePolyline();
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            Center.Save(writer);
            writer.Write(SemiMajorAxis);
            writer.Write(SemiMinorAxis);
            writer.Write(Rotation);
        }
    }
}
